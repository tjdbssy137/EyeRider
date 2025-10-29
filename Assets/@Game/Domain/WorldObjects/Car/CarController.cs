using UnityEngine;

public class CarController : BaseObject
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }
    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);   
    }
}
