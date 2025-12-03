using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class CameraFollowController : BaseObject
{
    [SerializeField] private Transform _car;
    [SerializeField] private float _followDamping = 12f;
    [SerializeField] private float _rotateDamping = 8f;
    [SerializeField] private float _sideLimit = 15f;

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
private float _prevYaw = 0f;

private void UpdateCamera()
{
    float carYaw = _car.rotation.eulerAngles.y;

    // 부드러운 yaw 보정
    _smoothYaw = Mathf.LerpAngle(
        _smoothYaw,
        carYaw,
        Time.fixedDeltaTime * _rotateDamping
    );

    // 회전 변화량
    float deltaYaw = Mathf.DeltaAngle(_prevYaw, _smoothYaw);
    _prevYaw = _smoothYaw;

    Quaternion camRot = Quaternion.Euler(0f, _smoothYaw, 0f);

    float height = 25f;
    float back = 5f;

    Vector3 baseOffset =
        camRot * new Vector3(0f, 0f, -back)
        + new Vector3(0f, height, 0f);

    Vector3 desiredPos = _car.position + baseOffset;

    // -------------------------------------------------
    // ★ A버전 핵심: 회전 시 즉시 "강한" 좌우 이동
    // -------------------------------------------------
    if (Mathf.Abs(deltaYaw) > 0.01f)
    {
        // 많이 이동하도록 강한 스케일링
        float rotationFactor = Mathf.Clamp(deltaYaw / 45f, -1f, 1f);

        // 강한 이동: back 거리가 크므로 sideShift가 크게 적용됨
        float sideShift = rotationFactor * back * 0.55f;

        Vector3 side = Vector3.Cross(Vector3.up, _worldForward).normalized;

        desiredPos += side * sideShift;
    }
    // -------------------------------------------------

    if (!_initialized)
    {
        _initialized = true;
        _smoothPos = desiredPos;
    }

    // Follow Lerp
    Vector3 nextPos = Vector3.Lerp(
        _smoothPos,
        desiredPos,
        Time.fixedDeltaTime * _followDamping
    );

    // Soft boundary
    float dist = Vector3.Dot(nextPos - _center, _worldRight);
    float absDist = Mathf.Abs(dist);

    float t = absDist / _sideLimit;
    float decay = Mathf.Clamp01(1f - t);

    Vector3 move = nextPos - _smoothPos;
    Vector3 rightComponent = Vector3.Project(move, _worldRight);
    Vector3 nonRight = move - rightComponent;

    Vector3 limitedMove = nonRight + rightComponent * decay;

    Vector3 newPos = _smoothPos + limitedMove;

    // Hard boundary (기존 유지)
    if (_worldRight.x < 0.99f && _worldRight.z < 0.99f)
    {
        _smoothPos = newPos;
    }
    else
    {
        if (0.99f < _worldRight.z || _worldRight.z < -0.99f)
        {
            float camZ = newPos.z;
            float centerZ = _center.z;
            float minZ = centerZ - _sideLimit;
            float maxZ = centerZ + _sideLimit;

            camZ = Mathf.Clamp(camZ, minZ, maxZ);
            newPos.z = camZ;

            _smoothPos = newPos;
        }
        else if (0.99f < _worldRight.x || _worldRight.x < -0.99f)
        {
            float camX = newPos.x;
            float centerX = _center.x;
            float minX = centerX - _sideLimit;
            float maxX = centerX + _sideLimit;

            camX = Mathf.Clamp(camX, minX, maxX);
            newPos.x = camX;

            _smoothPos = newPos;
        }
        else
        {
            _smoothPos = newPos;
        }
    }

    transform.position = _smoothPos;
    transform.rotation = Quaternion.Euler(55f, _smoothYaw, 0f);
}



}