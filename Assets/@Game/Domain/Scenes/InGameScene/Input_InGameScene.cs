using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.InputSystem;


public class Input_InGameScene : IInputSystem
{
    public InGameScene Scene => Managers.Scene.CurrentScene as InGameScene;
    
    public void Init()
    {
        // Contexts.InGame.OnKeyW.Subscribe(isPressed =>
        // {
        //     if(!isPressed)
        //     {
        //         return;
        //     }
        //     Contexts.InGame.KeyW = true;
        // }).AddTo(Scene);
        // Contexts.InGame.OnKeyA.Subscribe(isPressed =>
        // {
        //     if(!isPressed)
        //     {
        //         return;
        //     }
        //     Contexts.InGame.KeyA = true;
        // }).AddTo(Scene);
        // Contexts.InGame.OnKeyS.Subscribe(isPressed =>
        // {
        //     if(!isPressed)
        //     {
        //         return;
        //     }
        //     Contexts.InGame.KeyS = true;
        // }).AddTo(Scene);
        // Contexts.InGame.OnKeyD.Subscribe(isPressed =>
        // {
        //     if(!isPressed)
        //     {
        //         return;
        //     }
        //     Contexts.InGame.KeyD = true;
        // }).AddTo(Scene);
    }
    
    public void OnKeyAction()
    {
        if (Contexts.InGame.IsEnd)
        {
            return;
        }

        if (Contexts.InGame.IsPaused)
        {
            return;
        }
        Contexts.InGame.WKey = Keyboard.current.wKey.isPressed;
        Contexts.InGame.AKey = Keyboard.current.aKey.isPressed;
        Contexts.InGame.SKey = Keyboard.current.sKey.isPressed;
        Contexts.InGame.DKey = Keyboard.current.dKey.isPressed;
    }

}