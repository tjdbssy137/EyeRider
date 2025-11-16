using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class CameraFollowPoint : BaseObject
{
    private Transform _car;
    private Rigidbody _carRigidbody;
    [Header("차의 로컬 좌표계 기준 오프셋")]
    [Tooltip("차의 forward 방향으로 얼마나 떨어질지 (양수 = 앞, 음수 = 뒤)")]
    public float _forwardOffset = 0f;

    [Tooltip("차의 up 방향으로 얼마나 올릴지")]
    public float _upOffset = 3f;

    [Header("따라가는 정도")]
    [Tooltip("앞/뒤(전진) 따라가는 속도")]
    public float _forwardFollowSpeed = 5f;

    [Tooltip("위/아래 따라가는 속도")]
    public float _upFollowSpeed = 5f;

    [Tooltip("좌우 따라가는 정도 (0 = 거의 안 따라감, 1 = 바로 붙음)")]
    [Range(0f, 1f)]
    public float _lateralFollowRatio = 0.02f;
    public override bool Init()
	{
		if (base.Init() == false)
			return false;

		return true;
	}
	
	public override bool OnSpawn()
    {
		if (false == base.OnSpawn())
        {
            return false;
        }

        Observable.EveryLateUpdate()
            .Subscribe(_ => Follow())
            .AddTo(this);
        return true;
    }
    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
    }
    public void SettingCar(Car car)
    {   
        if(car == null)
        {
            Debug.Log("car is NULL");
        }
        _car = car.transform;
        _carRigidbody = car.GetComponent<Rigidbody>();
    }
   private void Follow()
{
    if (_car == null)
        return;

    Vector3 forward = _car.forward;
    Vector3 right   = _car.right;
    Vector3 up      = _car.up;

    Vector3 rel = transform.position - _car.position;

    float curForward = Vector3.Dot(rel, forward);
    float curLateral = Vector3.Dot(rel, right);
    float curUp      = Vector3.Dot(rel, up);

    float desiredForward = _forwardOffset; // 차 로컬 앞/뒤 오프셋
    float desiredLateral = 0f;             // 차의 정중앙
    float desiredUp      = _upOffset;

    float dt = Time.deltaTime;

    // ✅ 차 속도에 따라 forward 따라가는 속도 조절
    float speed = 0f;
    if (_carRigidbody != null)
        speed = _carRigidbody.linearVelocity.magnitude;

    // 이 값들은 인스펙터로 노출해도 되고, 상수로 둬도 됨
    float minForwardFollowSpeed = 5f;   // 거의 멈춰있을 때 따라가는 속도
    float maxForwardFollowSpeed = 30f;  // 최고속도일 때 따라가는 속도
    float maxSpeedForNormalize  = 60f;  // 이 속도 기준으로 0~1 정규화 (대충 차 최고속도)

    float speed01 = Mathf.Clamp01(speed / maxSpeedForNormalize);
    float forwardFollowSpeed = Mathf.Lerp(minForwardFollowSpeed, maxForwardFollowSpeed, speed01);

    // ✔ 속도가 빠를수록 desiredForward로 더 빨리 수렴
    float newForward = Mathf.Lerp(curForward, desiredForward, forwardFollowSpeed * dt);

    // 위/아래는 기존처럼 속도로 보간
    float newUp = Mathf.Lerp(curUp, desiredUp, _upFollowSpeed * dt);

    // 좌우는 ratio로만 살짝씩만 따라오게 (0.02면 거의 안 따라감)
    float newLateral = Mathf.Lerp(curLateral, desiredLateral, _lateralFollowRatio);

    Vector3 newRel = newForward * forward
                   + newLateral * right
                   + newUp * up;

    transform.position = _car.position + newRel;

    // 회전은 너무 느리면 멀미나니까 지금처럼 꽤 강하게 붙이는 거 괜찮음
    transform.rotation = Quaternion.Lerp(
        transform.rotation,
        Quaternion.LookRotation(forward, up),
        5f * dt);
}

}
