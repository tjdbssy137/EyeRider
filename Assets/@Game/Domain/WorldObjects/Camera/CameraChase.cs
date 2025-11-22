using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class CameraChase : BaseObject
{
    public float _leftLimit = -40f;
    public float _rightLimit = 40f;
    private Transform _car;

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

        return true;
    }

    private void Chase()
	{
		Vector3 right = _car.transform.right;
		Vector3 forward = _car.transform.forward; 

		Vector3 offset = transform.position - _car.position;

		float localRight = Vector3.Dot(offset, right);
		float localForward = Vector3.Dot(offset, forward);

		localRight = Mathf.Clamp(localRight, _leftLimit, _rightLimit);
		
		Vector3 desired =
			_car.position +
			right * localRight +
			forward * localForward;

		transform.position = desired;
	}
}