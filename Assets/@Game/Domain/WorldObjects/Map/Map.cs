using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Map : BaseObject
{
    public BoxCollider _collider;
    public override bool Init()
	{
		if (base.Init() == false)
			return false;
        if (_collider == null)
        {
            Debug.Log("_collider is NULL");
        }

        _collider.OnCollisionExitAsObservable()
            .Where(collision => collision.gameObject.CompareTag("Player"))
            .Subscribe(_ =>
            {
                Managers.Object.Despawn(this);
            })
            .AddTo(this);
		return true;
	}
	
	public override bool OnSpawn()
    {
		if (false == base.OnSpawn())
        {
            return false;
        }

        return true;
    }
    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
        
    }
}