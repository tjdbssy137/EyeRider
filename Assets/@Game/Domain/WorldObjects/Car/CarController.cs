using UniRx.Triggers;
using UnityEngine;
using UniRx;
using System;

public partial class CarController : BaseObject
{
    [Range(10, 120)] public float _verticalDefaultSpeed = 40;
    private float _verticalAccelerationSpeed = 0;
    public float _maxAcceleration = 30f;
    public float _accelPerSec = 30f;
    public float _decelPerSec = 40f;

    [Range(10, 120)] public float _horizontalSpeed = 15;
    [Range(10, 120)] public float _reverseSpeed = 20;
    [Range(10, 120)] public float _turnSpeed = 100;

    private bool _isRotating = false;
    private float _rotLerpTime = 0f;
    private float _rotDuration = 1f;
    private float _startYaw = 0f;
    private float _targetYaw = 0f;

    public Rigidbody _rigidbody;
    public GameObject _frontLeftMesh;
    public GameObject _frontRightMesh;

    private Quaternion _currentWheelRotation = Quaternion.identity;
    public ParticleSystem _RLWParticleSystem;
    public ParticleSystem _RRWParticleSystem;

    private bool _isOutside = false;
    private float _lastDistance = 0f;

    private Vector3 _center;

    private Vector3 _worldForward;
    private Vector3 _worldRight;

    private bool _pendingRotation = false;
    private float _pendingDegrees = 0f;
    private float _disableCorrectionTimer = 0f;
    private Vector3 _targetCenter;


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

        Contexts.InGame.OnEnterCorner
            .Subscribe(deg =>
            {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;

                Vector3 f = transform.forward;
                f.y = 0f;
                transform.forward = f.normalized;

                Vector3 r = transform.right;
                r.y = 0f;
                transform.right = r.normalized;

                _pendingRotation = true;
                _pendingDegrees = deg;

                Observable.Timer(TimeSpan.FromSeconds(0.08f))
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


        Contexts.InGame.OnExitEye
            .Subscribe(dist =>
            {
                _isOutside = true;
                _lastDistance = dist;
            })
            .AddTo(_disposables);

        Contexts.InGame.OnEnterEye
            .Subscribe(_ =>
            {
                _isOutside = false;
                _animator.SetFloat("Distance", 0f);
            })
            .AddTo(_disposables);

        Observable.Interval(TimeSpan.FromSeconds(0.2f))
            .Subscribe(_ =>
            {
                if (true == _isOutside)
                {
                    float dmg = DistancePenalty(_lastDistance);
                    Contexts.InGame.Car.DamageCondition(dmg);
                }
            })
            .AddTo(_disposables);

        _isOutside = false;
        _shakeIntensity = 0f;

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
                if (0f < _disableCorrectionTimer)
                {
                    _disableCorrectionTimer -= Time.fixedDeltaTime;
                }

                _center = Vector3.Lerp(_center, _targetCenter, Time.fixedDeltaTime * 5f);


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
        else if (Contexts.InGame.DKey)
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

        if (Contexts.InGame.WKey)
        {
            // 전진 가속
            target = _maxAcceleration * scale;
        }
        else if (Contexts.InGame.SKey)
        {
            // 후진 가속
            target = -_maxAcceleration * scale;
        }
        else
        {
            // 키 안 누르면 0 쪽으로 복귀
            target = 0f;
        }

        float rate = _decelPerSec;

        if (0f < target && _verticalAccelerationSpeed < target)
        {
            rate = _accelPerSec;
        }

        _verticalAccelerationSpeed = Mathf.MoveTowards(_verticalAccelerationSpeed, target, rate * Time.fixedDeltaTime);
    }


    private void VerticalMove()
    {
        float baseSpeed = _verticalDefaultSpeed + _verticalAccelerationSpeed;
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
        _disableCorrectionTimer = 0.2f;

        WheelEffect(true);
    }

    private void UpdateRotation()
    {
        if (false == _isRotating)
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

            ApplyCornerCorrection();
        }
    }

    private void ApplyCornerCorrection()
    {
        if (_disableCorrectionTimer > 0f)
            return;

        float distSide = Vector3.Dot(_rigidbody.position - _center, _worldRight);
        float clamped = Mathf.Clamp(distSide, -40f, 40f);
        float diff = clamped - distSide;

        if (0.05f < Mathf.Abs(diff))
        {
            Vector3 newPos = _rigidbody.position + (_worldRight * diff);
            _rigidbody.position = newPos;
        }
    }

    private void WheelEffect(bool drifting)
    {
        if (true == drifting)
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
