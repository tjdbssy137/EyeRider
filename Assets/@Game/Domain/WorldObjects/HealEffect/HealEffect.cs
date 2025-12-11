using DamageNumbersPro;
using UnityEngine;

public class HealEffect : BaseObject
{
    DamageNumberMesh _mesh;
    public override bool Init()
    {
        if(false == base.Init())
        {
            return false;
        }
        _mesh = this.GetComponent<DamageNumberMesh>();
        if (_mesh == null)
        {
            Debug.LogError("DamageNumberMesh component is missing on the GameObject.");
            return false;
        }

        return true;
    }

    public void SetHeal(float amount)
    {
        _mesh.number = amount;
    }
}
