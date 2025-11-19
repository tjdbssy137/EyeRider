using System;
using UniRx;
using UnityEngine;

public class Car : BaseObject
{
    CarController _carController;

    public float Fuel { get; private set; } = 100f;
    public float Condition { get; private set; } = 100f;
    private float _fuelConsumptionRate = 10;
    
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

        _carController = this.gameObject.GetComponent<CarController>();
        if(_carController != null)
        {
            Debug.LogWarning("_carController is NULL");
        }
        _carController.OnSpawn();
        _carController.SetInfo(0);

        Observable.Interval(TimeSpan.FromSeconds(1.5f))
            .Subscribe(_ => 
                ConsumeFuel()
            ).AddTo(_disposables); 
        return true;
    }
    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
        Contexts.InGame.Car = this;
    }

    public void ConsumeFuel()
    {
        Fuel -= _fuelConsumptionRate * Time.deltaTime;
    }

    public void RefillFuel(float fuel)
    {
        Fuel += fuel;
    }

    public void DamageCondition(float damage)
    {
        Condition -= damage;
    }
    public void RepairCondition(float recover)
    {
        Condition += recover;
    }
}