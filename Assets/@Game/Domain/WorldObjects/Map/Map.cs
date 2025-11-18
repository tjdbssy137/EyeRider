using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

public class Map : BaseObject
{
    public BoxCollider _collider;
    private MapData _data;
    public override bool Init()
	{
		if (base.Init() == false)
			return false;
        if (_collider == null)
        {
            Debug.Log("_collider is NULL");
        }
		return true;
	}
	
	public override bool OnSpawn()
    {
		if (false == base.OnSpawn())
        {
            return false;
        }
        _collider.OnCollisionExitAsObservable()
            .Where(collision => collision.gameObject.CompareTag("Player"))
            .Subscribe(_ =>
            {
                Managers.Object.Despawn(this);
                Contexts.Map.OnDeSpawnRoad.OnNext(Unit.Default);
                //Contexts.Map.OnSpawnRoad.OnNext(Unit.Default);
                // var car = Contexts.InGame.Car;
                // if (car == null)
                // {
                //     Debug.LogWarning("car is NULL");
                //     return;
                // }

                // float dist = Vector3.Distance(transform.position, car.transform.position);

                // if (20f <= dist)
                // {
                //     Managers.Object.Despawn(this);
                //     Contexts.Map.OnDeSpawnRoad.OnNext(Unit.Default);
                //     Contexts.Map.OnSpawnRoad.OnNext(Unit.Default);
                // }
            })
            .AddTo(_disposables);
        return true;
    }
    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
        _data = Managers.Data.MapDatas.GetValueOrDefault(dataTemplate);

        _collider.OnCollisionEnterAsObservable()
            .Where(collision => collision.gameObject.CompareTag("Player"))
            .Subscribe(_ =>
            {
                //Debug.Log("OnCollisionEnterAsObservable");
                if(_data.Direction == RoadDirection.none)
                {
                    return;
                }
                if(_data.Direction == RoadDirection.Right)
                {
                    Contexts.InGame.OnEnterCorner.OnNext(90);
                }
                if(_data.Direction == RoadDirection.Left)
                {
                    Contexts.InGame.OnEnterCorner.OnNext(-90);
                }
            })
            .AddTo(this);
    }
}