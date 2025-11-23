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

        if(_cameraTarget == null)
        {
            return;
        }

        // ---------------------------------------
        // 1) 자동차의 수평 forward/right만 사용
        // ---------------------------------------
        Vector3 flatForward = _car.forward;
        flatForward.y = 0f;
        flatForward.Normalize();

        // ---------------------------------------
        // 2) 탑뷰 + 뒤 오프셋
        // ---------------------------------------
        Vector3 desiredPos =
            _car.position
            + (-flatForward * 5f)     // 뒤로 거리감
            + (Vector3.up * 30f);     // 탑뷰

        // ---------------------------------------
        // 3) 흔들림 제거용 스무딩
        // ---------------------------------------
        _smoothPos = Vector3.Lerp(
            _smoothPos,
            desiredPos,
            Time.deltaTime * _followDamping
        );

        // ---------------------------------------
        // 4) 좌우 바운더리 클램프 (X축만)
        // ---------------------------------------
        float leftLimit = _center.x - _sideLimit;
        float rightLimit = _center.x + _sideLimit;

        Vector3 clampPos = _smoothPos;

        if(clampPos.x < leftLimit)
        {
            clampPos.x = leftLimit;
        }
        else if(rightLimit < clampPos.x)
        {
            clampPos.x = rightLimit;
        }

        transform.position = clampPos;

        // ---------------------------------------
        // 5) 회전: Yaw만 따라가고 Pitch 고정
        // ---------------------------------------
        Vector3 dir = _car.position - clampPos;
        dir.y = 0f;
        dir.Normalize();

        float targetYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

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