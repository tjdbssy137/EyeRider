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
    public float _speed = 20;
    [Range(10, 120)]
    public float _reverseSpeed = 20;
    [Range(10, 120)]
    public float _turnSpeed = 100;
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
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                InputKeyBoard();
            });

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
        if (Keyboard.current.aKey.isPressed)
        {
            float left = -1;
            this.Steer(left);
        }
        if (Keyboard.current.dKey.isPressed)
        {
            float right = 1;
            this.Steer(right);
        }
    }
    void FixedUpdate()
    {
        InputKeyBoard();
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
        Vector3 move = (_rigidbody.rotation * Vector3.forward) * _speed * Time.fixedDeltaTime;
        _rigidbody.MovePosition(_rigidbody.position + move);
    }
    private void Reverse()
    {
        _frontLeftMesh.transform.localRotation = Quaternion.identity;
        _frontRightMesh.transform.localRotation = Quaternion.identity;
        Vector3 move = (_rigidbody.rotation * transform.forward * -1) * _reverseSpeed * Time.fixedDeltaTime;
        _rigidbody.MovePosition(_rigidbody.position + move);
    }

    private void Steer(float direction)
    {
        Quaternion targetRot = Quaternion.Euler(0f, direction * 40f, 0f);

        _currentWheelRotation = Quaternion.RotateTowards(
            _currentWheelRotation, 
            targetRot, 
            _turnSpeed * Time.fixedDeltaTime
        );

        _frontLeftMesh.transform.localRotation = _currentWheelRotation;
        _frontRightMesh.transform.localRotation = _currentWheelRotation;
        
        float turn = direction * _turnSpeed * Time.fixedDeltaTime;
        Quaternion quaternion = _rigidbody.rotation * Quaternion.Euler(0f, turn, 0f);
        _rigidbody.MoveRotation(quaternion);
        WheelEffect(true);
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
