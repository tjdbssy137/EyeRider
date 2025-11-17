using UniRx.Triggers;
using UnityEngine;
using UniRx;
using System;

public partial class CarController : BaseObject
{
    private float _fuelAmount = 100;
    private float _condition = 100;
    [Range(10, 120)]
    public float _verticalDefaultSpeed = 40;
    private float _verticalAccelerationSpeed = 0;
    public float _maxAcceleration = 20f;
    public float _accelPerSec = 30f;
    public float _decelPerSec = 40f;
    //private bool _isAccelerateKeyPressed = false;

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

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        if (_rigidbody == null)
        {
            Debug.LogWarning("_rigidbody id NULL");
        }
        return true;
    }
    public override bool OnSpawn()
    {
        if (false == base.OnSpawn())
        {
            return false;
        }
        Debug.Log("CarController On");

        this.FixedUpdateAsObservable()
            .Subscribe(_ =>
            {
                //InputKeyBoard();
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

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                InputKeyBoard(); 
            }).AddTo(_disposables);

        Contexts.InGame.OnEnterCorner.Subscribe(degrees =>
        {
            this.Steer(degrees);
        }).AddTo(_disposables);

        return true;
    }

    private void InputKeyBoard()
    {
        //_isAccelerateKeyPressed = Keyboard.current.wKey.isPressed;
        if (Contexts.InGame.WKey)
        {
            WheelEffect(false);
        }
    
        if (Contexts.InGame.AKey)
        {
            float left = -1;
            this.HorizontalMove(left);
        }
        if (Contexts.InGame.DKey)
        {
            float right = 1;
            this.HorizontalMove(right);
        }
    }

    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
    }
#region Move
    private void Accelerate()
    {
        float targetAccel = Contexts.InGame.WKey ? _maxAcceleration : 0f;
        float rate = Contexts.InGame.WKey ? _accelPerSec : _decelPerSec;

        _verticalAccelerationSpeed = Mathf.MoveTowards(
            _verticalAccelerationSpeed,
            targetAccel,
            rate * Time.fixedDeltaTime
        );
    }
    private void Reverse()
    {
        _frontLeftMesh.transform.localRotation = Quaternion.identity;
        _frontRightMesh.transform.localRotation = Quaternion.identity;
        Vector3 move = (_rigidbody.rotation * transform.forward * -1) * _reverseSpeed * Time.fixedDeltaTime;
        _rigidbody.MovePosition(_rigidbody.position + move);
    }
    private void VerticalMove()
    {
        float speed =_verticalDefaultSpeed + _verticalAccelerationSpeed;
        Vector3 move = (_rigidbody.rotation * Vector3.forward) * speed * Time.fixedDeltaTime;
        _rigidbody.MovePosition(_rigidbody.position + move);
    }
    private void HorizontalMove(float dir)
    {
        if (Mathf.Approximately(dir, 0f))
            return;

        Vector3 sideDir = _rigidbody.rotation * Vector3.right;
        Vector3 move = sideDir * dir * _horizontalSpeed * Time.fixedDeltaTime;

        _rigidbody.MovePosition(_rigidbody.position + move);
        WheelEffect(true);
    }
    public void Steer(float degrees)
    {
        _isRotating = true;
        _rotLerpTime = 0f;

        _startYaw = _rigidbody.rotation.eulerAngles.y;
        _targetYaw = Mathf.Repeat(_startYaw + degrees, 360f);

        // Quaternion targetRot = Quaternion.Euler(0f, direction * 40f, 0f);
        // _currentWheelRotation = Quaternion.RotateTowards(
        //     _currentWheelRotation, 
        //     targetRot, 
        //     _turnSpeed * Time.fixedDeltaTime
        // );

        // _frontLeftMesh.transform.localRotation = _currentWheelRotation;
        // _frontRightMesh.transform.localRotation = _currentWheelRotation;
        
        // float turn = direction * _turnSpeed * Time.fixedDeltaTime;
        // Quaternion quaternion = _rigidbody.rotation * Quaternion.Euler(0f, turn, 0f);
        // _rigidbody.MoveRotation(quaternion);
        WheelEffect(_isRotating);
    }
#endregion
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
    private void ConsumeFuel()
    {
        //_fuelAmount -= fuelConsumptionRate * Time.deltaTime;
    }


}
