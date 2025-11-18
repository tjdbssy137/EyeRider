using UnityEngine;

public class UI_InGameScene : UI_Scene
{
    
    public override bool Init()
    {
        if (base.Init() == false)
			return false;


		return true;
    }

    public void SetInfo()
    {
        Managers.UI.ShowBaseUI<UI_Eye>();
    }


}
