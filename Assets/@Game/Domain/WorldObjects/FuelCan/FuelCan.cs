using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class FuelCan : BaseObject
{
    private Animator _animator;
    public BoxCollider _collider;
    // fuel 데이터 필요할 듯
    public override bool Init()
	{
		if (base.Init() == false)
			return false;

        _animator = this.GetComponentInChildren<Animator>();
        if (_animator == null)
        {
            Debug.LogWarning("_animator is NULL");
        }
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

        _collider.OnTriggerEnterAsObservable()
            .Where(collision => collision.gameObject.CompareTag("Player"))
            .Subscribe(_ =>
            {
                
                Contexts.InGame.Car.RefillFuel(50); // 임시
            }).AddTo(_disposables);

        _collider.OnTriggerExitAsObservable()
            .Where(collision => collision.gameObject.CompareTag("Player"))
            .Subscribe(_ =>
            {
                _animator.SetTrigger("IsGet");
                Managers.Object.Despawn(this);
            }).AddTo(_disposables);

        return true;
    }
    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
    }
}
