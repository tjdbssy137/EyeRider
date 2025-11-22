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

    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

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

        this.FixedUpdateAsObservable()
            .Subscribe(_ =>
            {
                if(Contexts.InGame.IsGameOver)
                {
                    return;
                }
                if(Contexts.InGame.IsPaused)
                {
                    return;
                }
                InputKeyBoard();
                VerticalMove();
                Accelerate();
                if (_isRotating)
                {
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
                        WheelEffect(_isRotating);
                    }
                }
            }).AddTo(_disposables);

        Contexts.InGame.OnEnterCorner
            .Subscribe(degrees =>
            {
                this.Steer(degrees);
            }).AddTo(_disposables);

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

        Observable.Interval(TimeSpan.FromSeconds(0.2f))
            .Subscribe(_ =>
            {
                if (_isOutside)
                {
                    float dmg = DistancePenalty(_lastDistance);
                    Contexts.InGame.Car.DamageCondition(dmg);
                }
            }).AddTo(_disposables);

        Contexts.InGame.CurrentMapXZ
            .Subscribe(mapPos =>
            {
                _center = mapPos;
            }).AddTo(_disposables);

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
        float targetAccel = Contexts.InGame.WKey ? (_maxAcceleration * scale) : 0f;
        float rate = Contexts.InGame.WKey ? _accelPerSec : _decelPerSec;
        _verticalAccelerationSpeed = Mathf.MoveTowards(_verticalAccelerationSpeed, targetAccel, rate * Time.fixedDeltaTime);
    }

    private void VerticalMove()
    {
        float speed = _verticalDefaultSpeed + _verticalAccelerationSpeed;
        Vector3 move = (_rigidbody.rotation * Vector3.forward) * speed * Time.fixedDeltaTime;
        _rigidbody.MovePosition(_rigidbody.position + move);
    }

    private void HorizontalMove(float dir)
    {
        float scale = 1f - ControlDifficulty;
        float newDir = dir * scale;
        Vector3 sideDir = _rigidbody.transform.right;
        Vector3 move = sideDir * newDir * _horizontalSpeed * Time.fixedDeltaTime;

        Vector3 radius = _rigidbody.position - _center;
        radius.y = 0f;

        float distSide = Vector3.Dot(radius, _rigidbody.transform.right);
        float absSide = Mathf.Abs(distSide);

        float boundary = 40f; // 맵 반쪽 좌우 사이즈의 80%
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

        _rigidbody.MovePosition(_rigidbody.position + move * decay);
        WheelEffect(true);
    }

    public void Steer(float degrees)
    {
        _isRotating = true;
        _rotLerpTime = 0f;
        _startYaw = _rigidbody.rotation.eulerAngles.y;
        _targetYaw = Mathf.Repeat(_startYaw + degrees, 360f);
        WheelEffect(_isRotating);
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