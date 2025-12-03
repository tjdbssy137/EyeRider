using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class VehicleCamera : BaseObject
{
    public Transform carTransform;
	[Range(1, 10)]
	public float followSpeed = 2;
	[Range(1, 10)]
	public float lookSpeed = 5;
	private Vector3 initialCameraPosition;
	private Vector3 initialCarPosition;
	private Vector3 absoluteInitCameraPosition;
	
	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		initialCameraPosition = gameObject.transform.position;
		initialCarPosition = carTransform.position;
		absoluteInitCameraPosition = initialCameraPosition - initialCarPosition;
		return true;
	}
	
	public override bool OnSpawn()
    {
		if (false == base.OnSpawn())
        {
            return false;
        }

		//Contexts.Car.OnCarMoving.OnNext();
		this.FixedUpdateAsObservable()
			.Subscribe(_ =>
				{
					Vector3 _lookDirection = (new Vector3(carTransform.position.x, carTransform.position.y, carTransform.position.z)) - transform.position;
					Quaternion _rot = Quaternion.LookRotation(_lookDirection, Vector3.up);
					transform.rotation = Quaternion.Lerp(transform.rotation, _rot, lookSpeed * Time.deltaTime);

					//Move to car
					Vector3 _targetPos = absoluteInitCameraPosition + carTransform.transform.position;
					transform.position = Vector3.Lerp(transform.position, _targetPos, followSpeed * Time.deltaTime);
				}
			);
        return true;
    }
    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
        
    }
    
}
