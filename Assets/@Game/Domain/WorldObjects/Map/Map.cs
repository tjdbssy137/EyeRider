using System.Collections.Generic;
using UnityEngine;

public class Map : BaseObject
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

        return true;
    }
    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
        
    }
}