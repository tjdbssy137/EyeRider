using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Obstacle : BaseObject
{
    public BoxCollider _collider;
    public ObstacleData _data;
    public float _destroyDistance = 80;
    public GameObject _particle;
    
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _collider = this.GetComponentInChildren<BoxCollider>();
        if (_collider == null)
        {
            Debug.LogWarning("_collider is NULL");
        }

        if(_particle == null)
        {
            Debug.LogWarning("_particle is NULL");
        }

        return true;
    }

    public override bool OnSpawn()
    {
        if (false == base.OnSpawn())
        {
            return false;
        }
        //Debug.Log("Obstacle OnSpawn");

        _collider.OnTriggerExitAsObservable()
            .Where(collision => collision.gameObject.CompareTag("Player"))
            .Subscribe(_ =>
            {
            })
            .AddTo(_disposables);

        _collider.OnTriggerEnterAsObservable()
            .Where(collision => collision.gameObject.CompareTag("Player"))
            .Subscribe(_ =>
            {
                //Debug.Log($"collision : {other.name}");
                Managers.Object.Spawn<ParticleObject>($"{_particle.name}", this.transform.position , 0, 0);
                //// 차의 속도를 80% 깎기
                Contexts.InGame.OnCollisionObstacle.OnNext(_data.CrashDamage);
                Managers.Resource.Destroy(gameObject);
            })
            .AddTo(_disposables);

        Transform car = Contexts.InGame.Car.transform;
        this.UpdateAsObservable()
            .Subscribe(_=>
            {
                if(!this.gameObject.activeSelf)
                {
                    return;
                }
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
    protected override void OnDestroy()
    {
        base.OnDestroy();
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
