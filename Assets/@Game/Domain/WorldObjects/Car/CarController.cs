using UniRx.Triggers;
using UnityEngine;
using UniRx;
using System;

public partial class CarController : BaseObject
{
    // 앞뒤 이동
    [Range(10, 120)] public float _verticalDefaultSpeed = 40;
    public float _maxAcceleration = 30f;
    public float _accelPerSec = 30f;
    public float _decelPerSec = 40f;

    // 좌우 이동
    [Range(10, 120)] public float _horizontalSpeed = 15;

    // 기본
    public Rigidbody _rigidbody;

    // 파티클
    public ParticleSystem _RLWParticleSystem;
    public ParticleSystem _RRWParticleSystem;

    // 폭풍의 눈
    private bool _isOutside = false;
    private float _lastDistance = 0f;

    // 자동차 이동 방향
    private Vector3 _center;
    private Vector3 _worldForward;
    private Vector3 _worldRight;

    // 코너 회전
    private bool _isRotating = false;
    private float _rotLerpTime = 0f;
    private float _rotDuration = 1f;
    private float _startYaw = 0f;
    private float _targetYaw = 0f;
    private bool _pendingRotation = false;
    private float _pendingDegrees = 0f;
    private Vector3 _targetCenter;

    // Panic
    private float _distancePanic;
    private float _eventPanic;
    private float _conditionPanic;
    private float _fuelPanic;
    

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        if (_rigidbody == null)
        {
            Debug.LogWarning("_rigidbody is NULL");
        }

        _animator = this.GetComponentInChildren<Animator>();
        if (_animator == null)
        {
            Debug.LogWarning("_animator is NULL");
        }

        return true;
    }

    public override bool OnSpawn()
    {
        if (false == base.OnSpawn())
        {
            return false;
        }

        _isOutside = false;
        _shakeIntensity = 0f;

        Contexts.InGame.WorldRightDir
            .Subscribe(r => _worldRight = r)
            .AddTo(_disposables);

        Contexts.InGame.WorldForwardDir
            .Subscribe(f => _worldForward = f)
            .AddTo(_disposables);

        Contexts.InGame.CurrentMapXZ
           .Subscribe(pos =>
           {
               _targetCenter = pos;
           })
           .AddTo(_disposables);

        if (_center == Vector3.zero)
        {
            _center = _rigidbody.position;
        }

        BindSubscriptions();

        this.FixedUpdateAsObservable()
            .Subscribe(_ =>
            {
                if (true == Contexts.InGame.IsGameOver)
                {
                    return;
                }
                if (true == Contexts.InGame.IsPaused)
                {
                    return;
                }

                _center = Vector3.Lerp(_center, _targetCenter, Time.fixedDeltaTime * 5f);
                
                PanicPointCaculator();
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

        if (Contexts.InGame.AKey)
        {
            horizontal = -1f;
        }
        
        if (Contexts.InGame.DKey)
        {
            horizontal = 1f;
        }

        if (0f < _shakeIntensity)
        {
            horizontal += UnityEngine.Random.Range(-_shakeIntensity, _shakeIntensity);
        }

        if (0.01f < Mathf.Abs(horizontal))
        {
            HorizontalMove(horizontal);
        }
        else
        {
            WheelEffect(false);
        }
    }

    private void Accelerate()
    {
        float scale = 1f - ControlDifficulty;
        float target = 0f;

        if(0 < Contexts.InGame.IsCollisionObstacle)
        {
            target = -_maxAcceleration * 1.2f * scale;
        }
        else if (Contexts.InGame.WKey)
        {
            // 전진 가속
            target = _maxAcceleration * scale;
        }
        else if (Contexts.InGame.SKey)
        {
            // 후진 가속
            target = -_maxAcceleration * 0.3f * scale;
        }
        else
        {
            // 키 안 누르면 기본
            target = 0f;
        }

        float rate = _decelPerSec;

        if (0f < target && Contexts.Car.VerticalAccelerationSpeed < target)
        {
            rate = _accelPerSec;
        }

        Contexts.Car.VerticalAccelerationSpeed = Mathf.MoveTowards(Contexts.Car.VerticalAccelerationSpeed, target, rate * Time.fixedDeltaTime);
    }


    private void VerticalMove()
    {
        float baseSpeed = _verticalDefaultSpeed + Contexts.Car.VerticalAccelerationSpeed;
        float curveBoost = _isRotating ? (baseSpeed * 0.25f) : 0f;
        float finalSpeed = baseSpeed + curveBoost;

        Vector3 move = (_rigidbody.rotation * Vector3.forward) * finalSpeed * Time.fixedDeltaTime;

        _rigidbody.MovePosition(_rigidbody.position + move);
    }

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
        {
            decay = Mathf.Clamp01(1f - t);
        }
        else if (0f < distSide && 0f < dir)
        {
            decay = Mathf.Clamp01(1f - t);
        }

        if (decay <= 0f)
        {
            _rigidbody.MovePosition(_rigidbody.position);
            return;
        }

        _rigidbody.MovePosition(_rigidbody.position + (move * decay));
        WheelEffect(true);
    }

    public void Steer(float degrees)
    {
        _isRotating = true;
        _rotLerpTime = 0f;

        _startYaw = _rigidbody.rotation.eulerAngles.y;
        _targetYaw = Mathf.Repeat(_startYaw + degrees, 360f);

        WheelEffect(true);
    }

    private void UpdateRotation()
    {
        if (!_isRotating)
        {
            return;
        }

        _rotLerpTime += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(_rotLerpTime / _rotDuration);

        float currentYaw = Mathf.LerpAngle(_startYaw, _targetYaw, t);
        Quaternion newRot = Quaternion.Euler(0f, currentYaw, 0f);
        _rigidbody.MoveRotation(newRot);

        if (1f <= t)
        {
            Quaternion finalRot = Quaternion.Euler(0f, _targetYaw, 0f);
            _rigidbody.MoveRotation(finalRot);

            _isRotating = false;
            WheelEffect(false);
        }
    }
}