using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class PointMover : UI_Base
{
    public RectTransform _point;
    public RectTransform _canvasRect;

    [Header("랜덤 이동 설정")]
    public float _randomMoveSpeed = 200f;
    public float _randomTargetInterval = 2f;

    [Header("입력 기반 끌림 설정")]
    [Range(0f, 1f)]
    public float _inputPullFactor = 0.5f;
    public float _inputFollowSpeed = 8f;
    [Range(-1f, 1f)]
    public float _directionThreshold = 0.45f;

    [Header("거리 기반 접근/이탈 속도")]
    public float _approachSpeed = 70f;
    public float _repelSpeed = 120f;

    [Header("좌우 전용 접근/이탈 속도")]
    public float _sideApproachSpeed = 80f;
    public float _sideRepelSpeed = 120f;

    private Camera _camera;
    private Transform _car;

    private Vector2 _randomPos;
    private Vector2 _randomTarget;
    private Vector2 _inputOffset;
    private Vector2 _targetOffset;

    private float CurrentRandomSpeed => _randomMoveSpeed * Managers.Difficulty.PM_RandomMul * (1f + Managers.Difficulty.StormSpeed * 0.2f);

    private float CurrentApproachSpeed => _approachSpeed * Managers.Difficulty.PM_ApproachMul;

    private float CurrentRepelSpeed => _repelSpeed * Managers.Difficulty.PM_RepelMul;

    private float CurrentSideApproachSpeed => _sideApproachSpeed * Managers.Difficulty.PM_ApproachMul;

    private float CurrentSideRepelSpeed => _sideRepelSpeed * Managers.Difficulty.PM_RepelMul;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        if (_point == null || _canvasRect == null)
        {
            Debug.LogWarning("PointMover is NULL");
            return false;
        }

        _randomPos = _point.anchoredPosition;
        _randomTarget = _randomPos;
        _inputOffset = Vector2.zero;
        _targetOffset = Vector2.zero;

        Observable.Interval(System.TimeSpan.FromSeconds(_randomTargetInterval))
            .Subscribe(_ => SetRandomTarget())
            .AddTo(this);

        this.UpdateAsObservable().Subscribe(_ =>
        {
            if (true == Contexts.InGame.IsGameOver)
            {
                return;
            }
            if (true == Contexts.InGame.IsPaused)
            {
                return;
            }
            Move();
            UpdateEyeSize();
            CheckCarInsideEye();
        }).AddTo(this);

        _camera = Object.FindFirstObjectByType<Camera>();

        Observable.NextFrame()
        .Subscribe(_ =>
        {
            if (_camera == null)
                _camera = Object.FindFirstObjectByType<Camera>();

            if (_car == null)
                _car = Contexts.InGame.Car.GetComponent<Transform>();

            Vector3 carScreen = _camera.WorldToScreenPoint(_car.position);

            Vector2 uiPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect,
                new Vector2(carScreen.x, carScreen.y),
                null,
                out uiPos
            );

            _point.anchoredPosition = uiPos;
            _randomPos = uiPos;
            _randomTarget = uiPos;
            _inputOffset = Vector2.zero;
            _targetOffset = Vector2.zero;

            CheckCarInsideEye();
        })
        .AddTo(this);

        return true;
    }

    private void SetRandomTarget()
    {
        float halfWidth = _canvasRect.rect.width * 0.5f;
        float halfHeight = _canvasRect.rect.height * 0.5f;

        float x = Random.Range(-halfWidth, halfWidth);
        float y = Random.Range(-halfHeight, halfHeight);

        _randomTarget = new Vector2(x, y);
    }

    private void Move()
    {
        float dt = Time.deltaTime;

        UpdateRandomPosition(dt);

        Vector2 inputDir = ReadInputDirection();

        if (0.0001f < inputDir.sqrMagnitude)
        {
            HandleForwardApproachRepel(inputDir, dt);
            HandleSideApproachRepel(inputDir, dt);
        }
        else
        {
            _targetOffset = Vector2.zero;
        }

        UpdateInputOffset(dt);
        ApplyFinalPosition();
    }

    private void UpdateRandomPosition(float dt)
    {
        _randomPos = Vector2.MoveTowards(
            _randomPos,
            _randomTarget,
            CurrentRandomSpeed * dt
        );
    }

    private bool _wasInside = false;
    private void CheckCarInsideEye()
    {
        if (_car == null)
            _car = Contexts.InGame.Car.GetComponent<Transform>();

        Vector3 carScreen = _camera.WorldToScreenPoint(_car.position);
        Vector2 carPos = new Vector2(carScreen.x, carScreen.y);
        Vector2 eyeScreenPos = RectTransformUtility.WorldToScreenPoint(null, _point.position);

        float radius = (_point.rect.width * 0.5f) * _point.lossyScale.x;

        float dist = Vector2.Distance(carPos, eyeScreenPos);
        bool nowInside = dist <= radius;

        if (_wasInside && !nowInside)
        {
            float resultDistance = Mathf.Abs(dist - radius);
            Contexts.InGame.OnExitEye.OnNext(resultDistance);
        }
        else if (!_wasInside && nowInside)
        {
            Contexts.InGame.OnEnterEye.OnNext(Unit.Default);
        }

        _wasInside = nowInside;
    }

    private void HandleForwardApproachRepel(Vector2 inputDir, float dt)
    {
        Vector2 stormPos = _randomPos;
        if (stormPos.sqrMagnitude <= 0.0001f)
        {
            _targetOffset = Vector2.zero;
            return;
        }

        Vector2 inputNorm = inputDir.normalized;
        Vector2 carToStorm = stormPos.normalized;
        float dot = Vector2.Dot(inputNorm, carToStorm);

        if (_directionThreshold < dot)
        {
            Vector2 stormToCar = -stormPos;
            _targetOffset = stormToCar * _inputPullFactor;

            float align = Mathf.InverseLerp(_directionThreshold, 1f, dot);
            Vector2 approachDir = stormToCar.normalized;

            _randomPos += approachDir * CurrentApproachSpeed * dt * align;
        }
        else if (dot < -_directionThreshold)
        {
            float repelAlign = Mathf.InverseLerp(-1f, -_directionThreshold, dot);
            Vector2 repelDir = carToStorm;

            _randomPos += repelDir * CurrentRepelSpeed * dt * repelAlign;
            _targetOffset = Vector2.zero;
        }
        else
        {
            _targetOffset = Vector2.zero;
        }
    }

    private void HandleSideApproachRepel(Vector2 inputDir, float dt)
    {
        Vector2 stormPos = _randomPos;

        float sideInput = Mathf.Sign(inputDir.x);
        if (Mathf.Abs(sideInput) <= 0.01f)
        {
            return;
        }

        float stormSide = 0f;
        const float sideDeadZone = 5f;

        if (sideDeadZone < stormPos.x) stormSide = 1f;
        else if (stormPos.x < -sideDeadZone) stormSide = -1f;

        if (Mathf.Abs(stormSide) <= 0.01f)
            return;

        if (0f < sideInput * stormSide)
        {
            _randomPos.x = Mathf.MoveTowards(
                _randomPos.x,
                0f,
                CurrentSideApproachSpeed * dt
            );
        }
        else if (sideInput * stormSide < 0f)
        {
            _randomPos.x += stormSide * CurrentSideRepelSpeed * dt;
        }
    }

    private void UpdateInputOffset(float dt)
    {
        _inputOffset = Vector2.Lerp(
            _inputOffset,
            _targetOffset,
            _inputFollowSpeed * dt
        );
    }

    private void ApplyFinalPosition()
    {
        Vector2 finalPos = _randomPos + _inputOffset;

        float halfW = _canvasRect.rect.width * 0.5f;
        float halfH = _canvasRect.rect.height * 0.5f;

        finalPos.x = Mathf.Clamp(finalPos.x, -halfW, halfW);
        finalPos.y = Mathf.Clamp(finalPos.y, -halfH, halfH);

        _point.anchoredPosition = finalPos;
    }

    private Vector2 ReadInputDirection()
    {
        Vector2 dir = Vector2.zero;

        if (Contexts.InGame.AKey) dir.x -= 1f;
        if (Contexts.InGame.DKey) dir.x += 1f;
        if (Contexts.InGame.WKey) dir.y += 1f;
        if (Contexts.InGame.SKey) dir.y -= 1f;

        return dir;
    }

    private void UpdateEyeSize()
    {
        float s = Managers.Difficulty.EyeSize;
        _point.localScale = new Vector3(s, s, 1f);
    }

}