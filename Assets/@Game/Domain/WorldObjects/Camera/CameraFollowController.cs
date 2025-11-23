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

        Quaternion camRot = Quaternion.Euler(0f, _smoothYaw, 0f);

        float height = 25f;
        float back = 5f;

        Vector3 desiredOffset =
            camRot * new Vector3(0f, 0f, -back)
            + new Vector3(0f, height, 0f);

        Vector3 desiredPos = _car.position + desiredOffset;

        if (!_initialized)
        {
            _initialized = true;
            _smoothPos = desiredPos;
        }

        Vector3 nextPos = Vector3.Lerp(
            _smoothPos,
            desiredPos,
            Time.fixedDeltaTime * _followDamping
        );

        float dist = Vector3.Dot(nextPos - _center, _worldRight);
        float absDist = Mathf.Abs(dist);

        // 경계에 가까울수록 더 강하게 감쇠
        float t = absDist / _sideLimit;
        float decay = Mathf.Clamp01(1f - t);

        // dist의 부호대로 movement를 줄임
        Vector3 move = nextPos - _smoothPos;
        Vector3 rightComponent = Vector3.Project(move, _worldRight);
        Vector3 nonRight = move - rightComponent;

        Vector3 limitedMove = nonRight + rightComponent * decay;

        _smoothPos += limitedMove;

        transform.position = _smoothPos;

        float pitch = 55f;
        transform.rotation = Quaternion.Euler(pitch, _smoothYaw, 0f);
    }

}
