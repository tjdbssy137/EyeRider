using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class CameraChase : BaseObject
{
    [Header("자동차 Transform")]
    public Transform _car;

    [Header("좌우 경계 (자동차 기준 로컬 좌/우)")]
    public float _maxSide = 40f;

    [Header("중심점 (Map이 설정하는 자동차 기준 중심)")]
    public Vector3 _center;

    public override bool OnSpawn()
    {
        if (base.OnSpawn() == false)
            return false;

        _car = Contexts.InGame.Car.transform;

        if (_car == null)
        {
            Debug.Log("_car is null");
        }

        this.LateUpdateAsObservable()
            .Subscribe(_ =>
            {
                Chase();
            }).AddTo(_disposables);

        Contexts.InGame.CurrentMapXZ
        .Subscribe(mapPos =>
        {
            _center = mapPos;
        }).AddTo(_disposables);

        return true;
    }

    private void Chase()
	{
		if (_car == null)
        {
            Debug.LogWarning("[CameraClampDebug] Car is NULL");
            return;
        }

        // 카메라 현재 위치
        Vector3 camPos = transform.position;

        // 자동차 기준 radius(좌우 판별용)
        Vector3 radius = camPos - _center;
        radius.y = 0f;

        // 자동차의 Right(좌우 기준 축)
        Vector3 right = _car.transform.right;

        // 자동차 기준 좌우 거리
        float distSide = Vector3.Dot(radius, right);

        // 정규화된 비율
        float t = Mathf.Abs(distSide) / _maxSide;

        // 디버그 로그
        Debug.Log(
            $"[CameraClampDebug]" +
            $"\n CarPos={_car.position}" +
            $"\n CamPos={camPos}" +
            $"\n Center={_center}" +
            $"\n CarRight={right}" +
            $"\n radius(local)={radius}" +
            $"\n distSide={distSide}" +
            $"\n |distSide|={Mathf.Abs(distSide)}" +
            $"\n maxSide={_maxSide}" +
            $"\n ratio(t)={t}" +
            $"\n OUTSIDE?={(Mathf.Abs(distSide) > _maxSide)}"
        );

        // 경계 넘었으면 보정
        if (distSide < -_maxSide)
        {
            float correction = -_maxSide - distSide;
            camPos += right * correction;

            Debug.Log($"[CameraClampDebug] → LEFT OUTSIDE | correction={correction} | newCamPos={camPos}");
            transform.position = camPos;
        }
        else if (distSide > _maxSide)
        {
            float correction = _maxSide - distSide;
            camPos += right * correction;

            Debug.Log($"[CameraClampDebug] → RIGHT OUTSIDE | correction={correction} | newCamPos={camPos}");
            transform.position = camPos;
        }
	}
}
