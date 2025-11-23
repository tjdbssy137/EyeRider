using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class CameraFollowController : BaseObject
{
    [SerializeField] private Transform _car;
    [SerializeField] private float _followDamping = 12f;
    [SerializeField] private float _rotateDamping = 8f;
    [SerializeField] private float _sideLimit = 40f;

    private Vector3 _center;
    private Vector3 _smoothPos;
    private float _smoothYaw = 0f;
    private bool _initialized = false;

    private Vector3 _worldRight;
    private Vector3 _worldForward;

    public override bool OnSpawn()
    {
        if (!base.OnSpawn()) return false;

        _car = Contexts.InGame.Car.transform;
        if (_car == null)
        {
            Debug.Log("_car is NULL");
        }

        Contexts.InGame.WorldRightDir
            .Subscribe(r => _worldRight = r)
            .AddTo(_disposables);

        Contexts.InGame.WorldForwardDir
            .Subscribe(f => _worldForward = f)
            .AddTo(_disposables);

        this.FixedUpdateAsObservable()
            .Subscribe(_ => UpdateCamera())
            .AddTo(_disposables);

        Contexts.InGame.CurrentMapXZ
            .Subscribe(mapPos =>
            {
                _center = mapPos;
            }).AddTo(_disposables);

        return true;
    }

    private void UpdateCamera()
    {
        float carYaw = _car.rotation.eulerAngles.y;

        _smoothYaw = Mathf.LerpAngle(
            _smoothYaw,
            carYaw,
            Time.fixedDeltaTime * _rotateDamping
        );

        Quaternion smoothRot = Quaternion.Euler(0f, _smoothYaw, 0f);

        Vector3 desiredPos = _car.position + smoothRot * new Vector3(0f, 0f, -5f) + new Vector3(0f, 25f, 0f);

        if (!_initialized)
        {
            _initialized = true;
            _smoothPos = desiredPos;
        }

        _smoothPos = Vector3.Lerp(
            _smoothPos,
            desiredPos,
            Time.fixedDeltaTime * _followDamping
        );

        float dist = Vector3.Dot(_smoothPos - _center, _worldRight);

        if (dist < -_sideLimit)
        {
            _smoothPos = _center + _worldRight * -_sideLimit;
        }
        else if (_sideLimit < dist)
        {
            _smoothPos = _center + _worldRight * _sideLimit;
        }

        transform.position = _smoothPos;

        float pitch = 55f;
        transform.rotation = Quaternion.Euler(pitch, _smoothYaw, 0f);
    }
}
