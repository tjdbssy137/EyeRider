using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class PointMover : UI_Base
{
    public RectTransform _point;
    public RectTransform _canvasRect;

    [Header("랜덤 이동 설정")]
    [Tooltip("랜덤 목적지로 이동하는 속도 (픽셀/초 느낌)")]
    public float _randomMoveSpeed = 200f;

    [Tooltip("몇 초마다 새로운 랜덤 목적지를 잡을지")]
    public float _randomTargetInterval = 2f; // 나중엔 이 시간도 랜덤으로.

    [Header("입력 기반 끌림 설정")]
    [Tooltip("입력 방향이 폭풍의눈 쪽을 향할 때, 얼마나 많이 차 쪽으로 끌어당길지 (0~1 비율)")]
    [Range(0f, 1f)]
    public float _inputPullFactor = 0.5f; // 0.5면 거리의 절반 정도만 줄어듦

    [Tooltip("입력에 의해 생기는 오프셋이 따라가는 속도")]
    public float _inputFollowSpeed = 8f;

    [Tooltip("입력 방향이 폭풍의 눈 방향과 얼마나 비슷해야 '끌어당기기'로 인정할지 (1=완전 같음, 0=-반대)")]
    [Range(-1f, 1f)]
    public float _directionThreshold = 0.45f; // 0.3 정도면 대충 같은 쪽

    [Header("거리 기반 접근/이탈 속도")]
    public float _approachSpeed = 70f; // 차 쪽으로
    public float _repelSpeed = 120f;    // 차에서 멀어지게

    [Header("좌우 전용 접근/이탈 속도")]
    public float _sideApproachSpeed = 80f; // 중앙(0) 쪽으로
    public float _sideRepelSpeed = 120f;     // 좌/우로 더 멀어지게

    // ==========================
    // 내부 상태
    // ==========================
    private Vector2 _randomPos;
    private Vector2 _randomTarget;
    private Vector2 _inputOffset;
    private Vector2 _targetOffset;

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

        this.UpdateAsObservable().Subscribe(_=>
        {
            Move();
        }).AddTo(this);

        return true;
    }

    private void SetRandomTarget()
    {
        float halfWidth = _canvasRect.rect.width * 0.5f;
        float halfHeight = _canvasRect.rect.height * 0.5f;

        // 캔버스 중앙 기준 랜덤 위치 선택
        float x = Random.Range(-halfWidth, halfWidth);
        float y = Random.Range(-halfHeight, halfHeight);

        _randomTarget = new Vector2(x, y);
    }

    private void Move()
    {
        float dt = Time.deltaTime;

        //랜덤 이동
        UpdateRandomPosition(dt);

        //입력 방향 계산
        Vector2 inputDir = ReadInputDirection();

        if (0.0001f < inputDir.sqrMagnitude)
        {
            //앞뒤 접근/이탈
            HandleForwardApproachRepel(inputDir, dt);

            //좌우 접근/이탈
            HandleSideApproachRepel(inputDir, dt);
        }
        else
        {
            // 입력이 없으면 오프셋 목표를 0으로
            _targetOffset = Vector2.zero;
        }

        //오프셋 Lerp
        UpdateInputOffset(dt);

        //최종 위치 적용 + 클램프
        ApplyFinalPosition();
    }

    private void UpdateRandomPosition(float dt)
    {
        _randomPos = Vector2.MoveTowards(
            _randomPos,
            _randomTarget,
            _randomMoveSpeed * dt
        );
    }

    private void HandleForwardApproachRepel(Vector2 inputDir, float dt)
    {
        Vector2 stormPos = _randomPos; // 차(0,0) 기준 폭풍의 눈 위치
        if (stormPos.sqrMagnitude <= 0.0001f)
        {
            _targetOffset = Vector2.zero;
            return;
        }

        Vector2 inputNorm = inputDir.normalized;
        Vector2 carToStorm = stormPos.normalized; // 차 -> 폭풍의 눈 방향
        float dot = Vector2.Dot(inputNorm, carToStorm);

        if (_directionThreshold < dot)
        {
            // 1) 자동차가 폭풍의 눈 방향으로 움직일 때 → 가까워짐
            Vector2 stormToCar = -stormPos; // 폭풍의 눈 -> 차 방향

            // 자석 느낌(일시적인 오프셋)
            _targetOffset = stormToCar * _inputPullFactor;

            // 기준 위치 자체를 차 쪽으로 이동
            float align = Mathf.InverseLerp(_directionThreshold, 1f, dot); // 0~1
            Vector2 approachDir = stormToCar.normalized;

            _randomPos += approachDir * _approachSpeed * dt * align;
        }
        else if (dot < -_directionThreshold)
        {
            // 2) 자동차가 폭풍의 눈의 반대 방향으로 움직일 때 → 멀어짐
            float repelAlign = Mathf.InverseLerp(-1f, -_directionThreshold, dot); // 0~1
            Vector2 repelDir = carToStorm; // 차 -> 폭풍의 눈 방향으로 밀어냄

            _randomPos += repelDir * _repelSpeed * dt * repelAlign;

            // 반대 방향일 땐 자석 오프셋은 제거
            _targetOffset = Vector2.zero;
        }
        else
        {
            // 3) 그 사이 각도(대충 옆으로 움직일 때) → 중립
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

        // 폭풍의 눈이 화면 어느 쪽에 있는지
        float stormSide = 0f;
        const float sideDeadZone = 5f;

        if (sideDeadZone < stormPos.x)
        {
            stormSide = 1f;
        }
        else if (stormPos.x < -sideDeadZone)
        {
            stormSide = -1f;
        }

        if (Mathf.Abs(stormSide) <= 0.01f)
        {
            return;
        }

        // 같은 방향 → 중앙(0) 쪽으로 붙어가는 느낌
        if (0f < sideInput * stormSide)
        {
            _randomPos.x = Mathf.MoveTowards(
                _randomPos.x,
                0f,
                _sideApproachSpeed * dt
            );
        }
        // 반대 방향 → 더 멀어지는 느낌
        else if (sideInput * stormSide < 0f)
        {
            _randomPos.x += stormSide * _sideRepelSpeed * dt;
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

        if (Contexts.InGame.AKey)
        {
            dir.x -= 1f;
        }
        if (Contexts.InGame.DKey)
        {
            dir.x += 1f;
        }

        if (Contexts.InGame.WKey)
        {
            dir.y += 1f;
        }
        if (Contexts.InGame.SKey)
        {
            dir.y -= 1f;
        }

        return dir;
    }
}
