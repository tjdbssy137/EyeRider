using System;
using UniRx;
using UnityEngine;

public class Car : BaseObject
{
    CarController _carController;

    private float _fuelConsumptionRate = 50;
    public Subject<(float, float)> OnConditionChanged { get; private set; } = new Subject<(float, float)>();
    public Subject<(float, float)> OnFuelChanged { get; private set; } = new Subject<(float, float)>();

    public float Condition
    {
        get => _condition;
        set
        {
            float oldCondition = _condition;
            _condition = Mathf.Clamp(value, 0.0f, 100);
            OnConditionChanged.OnNext((oldCondition, _condition));
        }
    }
    private float _condition;

    public float Fuel
    {
        get => _fuel;
        set
        {
            float oldFuel = _fuel;
            Debug.Log($"oldFuel : {oldFuel}");
            _fuel = Mathf.Clamp(value, 0.0f, 100);
            OnFuelChanged.OnNext((oldFuel, _fuel));
        }
    }
    private float _fuel;
    
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

        Condition = 100;
        Fuel = 100;

        Observable.Interval(TimeSpan.FromSeconds(1.5f))
            .Subscribe(_ => 
                ConsumeFuel()
            ).AddTo(_disposables); 

        Contexts.InGame.Car = this;
        this.GetComponentInChildren<UI_Car>().SetInfo(Contexts.InGame.Car != null);
        return true;
    }
    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
    }

    public void ConsumeFuel()
    {
        Fuel -= 10;
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