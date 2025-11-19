using UnityEngine;

public class UI_Car : UI_Base
{
    private enum Images
    {
        UI_Condition,
        UI_Fuel,

    }
    public override bool Init()
    {
        if (base.Init() == false)
			return false;

        BindImages(typeof(Images));

		return true;
    }
}
