using UniRx.Triggers;
using UnityEngine;
using UniRx;
using System;

public partial class CarController : BaseObject
{
    [Range(10, 120)]
    public float _verticalDefaultSpeed = 40;
    private float _verticalAccelerationSpeed = 0;
    public float _maxAcceleration = 20f;
    public float _accelPerSec = 30f;
    public float _decelPerSec = 40f;

    [Range(10, 120)]
    public float _horizontalSpeed = 15;
    [Range(10, 120)]
    public float _reverseSpeed = 20;
    [Range(10, 120)]
    public float _turnSpeed = 100;

    private bool _isRotating = false;
    private float _rotLerpTime = 0f;
    private float _rotDuration = 1f;
    private float _startYaw;
    private float _targetYaw;

    public Rigidbody _rigidbody;
    public GameObject _frontLeftMesh;
    public GameObject _frontRightMesh;
    private Quaternion _currentWheelRotation = Quaternion.identity;
    public ParticleSystem _RLWParticleSystem;
    public ParticleSystem _RRWParticleSystem;

    private bool _isOutside = false;
    private float _lastDistance = 0f;

    private Vector3 _center;

    // 도로 기준 방향
    private Vector3 _worldForward;
    private Vector3 _worldRight;

    // ★ 회전 지연 컨트롤
    private bool _pendingRotation = false;
    private float _pendingDegrees = 0f;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        if (_rigidbody == null)
            Debug.LogWarning("_rigidbody is NULL");

        _animator = this.GetComponentInChildren<Animator>();
        if (_animator == null)
            Debug.LogWarning("_animator is NULL");

        return true;
    }

    public override bool OnSpawn()
    {
        if (false == base.OnSpawn())
            return false;

        Contexts.InGame.WorldRightDir
            .Subscribe(r => _worldRight = r)
            .AddTo(_disposables);

        Contexts.InGame.WorldForwardDir
            .Subscribe(f => _worldForward = f)
            .AddTo(_disposables);

        Contexts.InGame.CurrentMapXZ
            .Subscribe(mapPos => _center = mapPos)
            .AddTo(_disposables);

        if (_center == Vector3.zero)
            _center = _rigidbody.position;

        // ★ 회전 이벤트 – 즉시 Steer() 호출 X, 지연 후 실행
        Contexts.InGame.OnEnterCorner
            .Subscribe(deg =>
            {
                _pendingRotation = true;
                _pendingDegrees = deg;

                // 손대지 않는 CameraRig 쪽과의 싱크 맞추기 위해
                Observable.Timer(TimeSpan.FromSeconds(0.12f))
                    .Subscribe(__ =>
                    {
                        if (_pendingRotation)
                        {
                            _pendingRotation = false;
                            this.Steer(_pendingDegrees);
                        }
                    })
                    .AddTo(_disposables);
            })
            .AddTo(_disposables);

        // Eye
        Contexts.InGame.OnExitEye
            .Subscribe(distance =>
            {
                _isOutside = true;
                _lastDistance = distance;
            }).AddTo(_disposables);

        Contexts.InGame.OnEnterEye
            .Subscribe(_ =>
            {
                _isOutside = false;
                _animator.SetFloat("Distance", 0);
            }).AddTo(_disposables);

        // Damage timer
        Observable.Interval(TimeSpan.FromSeconds(0.2f))
            .Subscribe(_ =>
            {
                if (_isOutside)
                {
                    float dmg = DistancePenalty(_lastDistance);
                    Contexts.InGame.Car.DamageCondition(dmg);
                }
            }).AddTo(_disposables);

        _isOutside = false;
        _shakeIntensity = 0f;

        this.FixedUpdateAsObservable()
            .Subscribe(_ =>
            {
                if (Contexts.InGame.IsGameOver) return;
                if (Contexts.InGame.IsPaused) return;

                UpdateRotation();
                InputKeyBoard();
                VerticalMove();
                Accelerate();
            })
            .AddTo(_disposables);

        return true;
    }

    private void InputKeyBoard()
    {
        float horizontal = 0f;

        if (Contexts.InGame.AKey) horizontal = -1f;
        else if (Contexts.InGame.DKey) horizontal = 1f;

        if (_shakeIntensity > 0f)
            horizontal += UnityEngine.Random.Range(-_shakeIntensity, _shakeIntensity);

        if (Mathf.Abs(horizontal) > 0.01f)
            HorizontalMove(horizontal);
        else
            WheelEffect(false);
    }

    private void Accelerate()
    {
        float scale = 1f - ControlDifficulty;
        float targetAccel = Contexts.InGame.WKey ? (_maxAcceleration * scale) : 0f;
        float rate = Contexts.InGame.WKey ? _accelPerSec : _decelPerSec;

        _verticalAccelerationSpeed =
            Mathf.MoveTowards(_verticalAccelerationSpeed, targetAccel, rate * Time.fixedDeltaTime);
    }

    private void VerticalMove()
    {
        float baseSpeed = _verticalDefaultSpeed + _verticalAccelerationSpeed;

        float curveBoost = _isRotating ? (baseSpeed * 0.25f) : 0f;
        float finalSpeed = baseSpeed + curveBoost;

        Vector3 move = (_rigidbody.rotation * Vector3.forward)
                       * finalSpeed * Time.fixedDeltaTime;

        _rigidbody.MovePosition(_rigidbody.position + move);
    }

    // 좌우 이동
    private void HorizontalMove(float dir)
    {
        float scale = 1f - ControlDifficulty;
        float newDir = dir * scale;

        Vector3 sideDir = _rigidbody.transform.right;
        Vector3 move = sideDir * newDir * _horizontalSpeed * Time.fixedDeltaTime;

        float distSide = Vector3.Dot(_rigidbody.position - _center, _worldRight);
        float absSide = Mathf.Abs(distSide);

        float boundary = 40f;
        float t = absSide / boundary;
        float decay = 1f;

        if (distSide < 0f && dir < 0f)
            decay = Mathf.Clamp01(1f - t);
        else if (distSide > 0f && dir > 0f)
            decay = Mathf.Clamp01(1f - t);

        if (decay <= 0f)
        {
            _rigidbody.MovePosition(_rigidbody.position);
            return;
        }

        _rigidbody.MovePosition(_rigidbody.position + move * decay);
        WheelEffect(true);
    }

    // 회전 시작
    public void Steer(float degrees)
    {
        _isRotating = true;
        _rotLerpTime = 0f;

        _startYaw = _rigidbody.rotation.eulerAngles.y;
        _targetYaw = Mathf.Repeat(_startYaw + degrees, 360f);

        WheelEffect(true);
    }

    // 회전 업데이트
    private void UpdateRotation()
    {
        if (_isRotating == false)
            return;

        _rotLerpTime += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(_rotLerpTime / _rotDuration);

        float currentYaw = Mathf.LerpAngle(_startYaw, _targetYaw, t);
        Quaternion newRot = Quaternion.Euler(0f, currentYaw, 0f);
        _rigidbody.MoveRotation(newRot);

        if (t >= 1f)
        {
            Quaternion finalRot = Quaternion.Euler(0f, _targetYaw, 0f);
            _rigidbody.MoveRotation(finalRot);

            _isRotating = false;
            WheelEffect(false);

            // ★ 회전 종료 직후 보정
            ApplyCornerCorrection();
        }
    }

    // ★ 회전 중 튀어나간 위치 보정
    private void ApplyCornerCorrection()
    {
        float distSide = Vector3.Dot(_rigidbody.position - _center, _worldRight);
        float clamped = Mathf.Clamp(distSide, -40f, 40f);
        float diff = clamped - distSide;

        if (Mathf.Abs(diff) > 0.05f)
        {
            Vector3 newPos = _rigidbody.position + _worldRight * diff;
            _rigidbody.position = newPos;
        }
    }

    private void WheelEffect(bool isDrifting)
    {
        if (isDrifting)
        {
            _RLWParticleSystem.Play();
            _RRWParticleSystem.Play();
        }
        else
        {
            _RLWParticleSystem.Stop();
            _RRWParticleSystem.Stop();
        }
    }
}