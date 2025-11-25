using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Obstacle : BaseObject
{
    public BoxCollider _collider;
    public ObstacleData _data;
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
