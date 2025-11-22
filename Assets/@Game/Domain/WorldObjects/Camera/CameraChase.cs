
using UniRx.Triggers;
using UnityEngine;
using UniRx;

public class CameraChase : BaseObject
{
    public float _halfWidth = 10;

    private Transform _car;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public override bool OnSpawn()
    {
        if (base.OnSpawn() == false)
            return false;

        _car = Contexts.InGame.Car.transform;
		if(_car == null)
        {
            Debug.Log("_car is NULL");
        }
        this.LateUpdateAsObservable()
            .Subscribe(_ =>
            {
                CameraMove();
            }).AddTo(_disposables);

        return true;
    }

    private void CameraMove()
    {
        Vector3 carPos = _car.position;

        // 자동차의 좌우축
        Vector3 right = _car.right;
        right.y = 0;
        right.Normalize();

        // CameraTarget이 car을 따라가고 싶은 목표 위치
        Vector3 desiredPos = carPos;

        // 현재 CameraTarget → Car 방향
        Vector3 toCar = carPos - transform.position;

        // 좌우 방향 거리
        float lateral = Vector3.Dot(toCar, right);

        // 좌우 Clamp
        float clampedLateral = Mathf.Clamp(lateral, -_halfWidth, _halfWidth);

        // Clamp 보정값
        float correction = clampedLateral - lateral;

        // X축 보정 (좌우만 제한)
        Vector3 newPos = transform.position + right * correction;

        // 전진/후진은 Car 따라감
        newPos.z = carPos.z;

        // 높이도 Car 따라감
        newPos.y = carPos.y;

        transform.position = newPos;
    }
}
