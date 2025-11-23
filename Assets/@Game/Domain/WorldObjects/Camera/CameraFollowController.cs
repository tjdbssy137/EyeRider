using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class CameraFollowController : BaseObject
{
    [Header("자동차 참조")]
    [SerializeField] private Transform _car;
    [SerializeField] private Transform _cameraTarget;

    [Header("카메라 설정")]
    [SerializeField] private float _followDamping = 10f;
    [SerializeField] private float _rotateDamping = 6f;

    [Header("좌우 바운더리")]
    [SerializeField] private float _sideLimit = 40f;

    private Vector3 _center;
    private Vector3 _smoothPos;
    private float _currentYaw = 0f;

    public override bool OnSpawn()
    {
        if (base.OnSpawn() == false)
        {
            return false;
        }

        _car = Contexts.InGame.Car.transform;
        _cameraTarget = _car.transform.Find("CameraTargetObject");

        this.FixedUpdateAsObservable()
            .Subscribe(_ =>
            {
                UpdateCamera();
            }).AddTo(_disposables);

        Contexts.InGame.CurrentMapXZ
        .Subscribe(mapPos =>
        {
            _center = mapPos;
        }).AddTo(_disposables);

        return true;
    }

    public override void SetInfo(int id)
    {
        _car = Contexts.InGame.Car.transform;
        _cameraTarget = _car.transform.Find("CameraTargetObject");
    }


private void UpdateCamera()
{
    if(_car == null)
    {
        return;
    }

    // ---------------------------------------
    // 1) 자동차의 수평 forward만 사용
    // ---------------------------------------
    Vector3 flatForward = _car.forward;
    flatForward.y = 0f;
    flatForward.Normalize();

    // ---------------------------------------
    // 2) 기본 desired pos
    // ---------------------------------------
    Vector3 desiredPos =
        _car.position
        + (-flatForward * 5f)
        + (Vector3.up * 30f);

    // ---------------------------------------
    // 3) 스무딩
    // ---------------------------------------
    _smoothPos = Vector3.Lerp(
        _smoothPos,
        desiredPos,
        Time.deltaTime * _followDamping
    );

    // ---------------------------------------
    // 4) 좌우 바운더리: 스무딩 값 자체를 클램프
    // ---------------------------------------
    float leftLimit = _center.x - _sideLimit;
    float rightLimit = _center.x + _sideLimit;

    if(_smoothPos.x < leftLimit)
    {
        _smoothPos.x = leftLimit;
    }
    else if(rightLimit < _smoothPos.x)
    {
        _smoothPos.x = rightLimit;
    }

    // transform에 최종 적용
    transform.position = _smoothPos;

    // ---------------------------------------
    // 5) 회전: 반드시 클램프되기 전 방향 기반으로!
    // ---------------------------------------
    Vector3 yawDir = _car.position - _smoothPos;
    yawDir.y = 0f;
    yawDir.Normalize();

    float targetYaw = Mathf.Atan2(yawDir.x, yawDir.z) * Mathf.Rad2Deg;

    _currentYaw = Mathf.LerpAngle(
        _currentYaw,
        targetYaw,
        Time.deltaTime * _rotateDamping
    );

    float pitch = 55f;

    Quaternion finalRot = Quaternion.Euler(pitch, _currentYaw, 0f);
    transform.rotation = finalRot;
}

}