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

    public override bool OnSpawn()
    {
        if (!base.OnSpawn()) return false;

        _car = Contexts.InGame.Car.transform;

        this.FixedUpdateAsObservable()
            .Subscribe(_ => UpdateCamera())
            .AddTo(_disposables);

        Contexts.InGame.CurrentMapXZ
            .Subscribe(xz =>
            {
                _center = xz;
            }).AddTo(_disposables);

        return true;
    }

    private void UpdateCamera()
    {
        if (_car == null) return;

        float carYaw = _car.rotation.eulerAngles.y;

        // 회전 자체도 부드럽게
        _smoothYaw = Mathf.LerpAngle(
            _smoothYaw,
            carYaw,
            Time.fixedDeltaTime * _rotateDamping
        );

        Quaternion smoothRot = Quaternion.Euler(0f, _smoothYaw, 0f);

        Vector3 desiredPos =
            (_car.position +
            smoothRot * new Vector3(0f, 0f, -5f) +
            new Vector3(0f, 20f, 0f));

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

        float left = _center.x - _sideLimit;
        float right = _center.x + _sideLimit;

        if (_smoothPos.x < left) _smoothPos.x = left;
        else if (right < _smoothPos.x) _smoothPos.x = right;

        transform.position = _smoothPos;

        float pitch = 55f;
        transform.rotation = Quaternion.Euler(pitch, _smoothYaw, 0f);
    }
}