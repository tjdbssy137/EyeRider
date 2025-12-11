using DamageNumbersPro;
using UnityEngine;

public class DamageEffect : BaseObject
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

    public void SetDamage(float damage)
    {
        _mesh.number = damage;
    }

}
