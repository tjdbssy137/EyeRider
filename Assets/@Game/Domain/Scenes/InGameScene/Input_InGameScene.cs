using UniRx;
using UniRx.Triggers;
using UnityEngine;


public class Input_InGameScene : IInputSystem
{
    public InGameScene Scene => Managers.Scene.CurrentScene as InGameScene;

    public void Init()
    {

    }
    
    public void OnKeyAction()
    {
        
    }

}