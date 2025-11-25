using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Obstacle : BaseObject
{
    public BoxCollider _collider;
    public ObstacleData _data;
    public float _destroyDistance = 80;
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

        _collider.OnCollisionExitAsObservable()
            .Where(collision => collision.gameObject.CompareTag("Player"))
            .Subscribe(_ =>
            {
                
            })
            .AddTo(_disposables);

        _collider.OnCollisionEnterAsObservable()
            .Where(collision => collision.gameObject.CompareTag("Player"))
            .Subscribe(_ =>
            {
                
            })
            .AddTo(_disposables);

        Transform car = Contexts.InGame.Car.transform;
        this.UpdateAsObservable()
            .Subscribe(_=>
            {
                Vector3 toObstacle = transform.position - car.position;

                float dot = Vector3.Dot(car.forward, toObstacle);

                float distance = toObstacle.magnitude;

                if(dot < 0 && _destroyDistance <= distance)
                {
                    Managers.Resource.Destroy(gameObject);
                }

            }).AddTo(_disposables);
            return true;
    }
    
    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
        Managers.Data.ObstacleData.TryGetValue(dataTemplate, out _data);
        if(_data == null)
        {
            Debug.LogWarning("_data is NULL");
        }
    }
}
