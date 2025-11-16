using UniRx.Triggers;
using UnityEngine;
using UnityEngine.InputSystem;
using UniRx;
using System;

public partial class CarController : BaseObject
{
    private float _fuelAmount = 100;
    private float _condition = 100;
    [Range(10, 120)]
    public float _verticalSpeed = 40;
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
        if (Keyboard.current.wKey.isPressed)
        {
            this.Accelerate();
            WheelEffect(false);
        }
        if (Keyboard.current.sKey.isPressed)
        {
            this.Reverse();
            WheelEffect(false);
        }
        if (Keyboard.current.aKey.isPressed) //if (Keyboard.current.aKey.wasReleasedThisFrame)
        {
            this.HorizontalMove(Vector3.left);
            //float left = -1;
            //this.Steer(-90);
            //Contexts.InGame.OnEnterCorner.OnNext(-90); // 작동 ㄴ
        }
        if (Keyboard.current.dKey.isPressed) //if (Keyboard.current.dKey.wasReleasedThisFrame)
        {
            this.HorizontalMove(Vector3.right);
            //float right = 1;
            //this.Steer(90);
            //Contexts.InGame.OnEnterCorner.OnNext(90);
        }
    }

    void FixedUpdate()
    {
        InputKeyBoard();

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
    }

    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
    }
#region Move
    private void Accelerate()
    {
        _frontLeftMesh.transform.localRotation = Quaternion.identity;
        _frontRightMesh.transform.localRotation = Quaternion.identity;
        Vector3 move = (_rigidbody.rotation * Vector3.forward) * _verticalSpeed * Time.fixedDeltaTime;
        _rigidbody.MovePosition(_rigidbody.position + move);
    }
    private void Reverse()
    {
        _frontLeftMesh.transform.localRotation = Quaternion.identity;
        _frontRightMesh.transform.localRotation = Quaternion.identity;
        Vector3 move = (_rigidbody.rotation * transform.forward * -1) * _reverseSpeed * Time.fixedDeltaTime;
        _rigidbody.MovePosition(_rigidbody.position + move);
    }
    private void HorizontalMove(Vector3 direction)
    {
        Vector3 move = direction * _horizontalSpeed * Time.fixedDeltaTime;
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
