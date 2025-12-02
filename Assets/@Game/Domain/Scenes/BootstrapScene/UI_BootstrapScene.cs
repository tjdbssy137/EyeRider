using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class UI_BootstrapScene : UI_Scene
{
    private enum Images
    {
        
    }
    public override bool Init()
    {
        if (base.Init() == false)
			return false;

        BindImages(typeof(Images));
		return true;
    }

    //Managers.Scene.LoadScene(EScene.InGameScene);

}