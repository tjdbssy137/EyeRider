using System;
using UniRx;
using UnityEngine;

public class ParticleObject : BaseObject
{
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

        Observable.Timer(TimeSpan.FromSeconds(5f))
        .Subscribe(_ =>
        {
            Managers.Resource.Destroy(this.gameObject);                        
        })
        .AddTo(_disposables);
        return true;
    }

    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);

    }
}
