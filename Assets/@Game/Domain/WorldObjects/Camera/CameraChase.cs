using UnityEngine;
using UniRx;

public class CameraChase : BaseObject
{
	[Range(1, 10)]
	public float _followSpeed = 2;
	[Range(1, 10)]
	public float _lookSpeed = 5;
	
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

        return true;
    }
    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
        
    }
    
}