using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class InputManagerEx
{
    public Action KeyAction = null;
    public Action<Define.EMouseEvent> MouseAction = null;
    bool _mousePressed = false;

    public void OnUpdate()
    {
        if (Keyboard.current.anyKey.isPressed && KeyAction != null)
        {
            KeyAction?.Invoke();
        }

        if (MouseAction != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                this.MouseAction.Invoke(Define.EMouseEvent.Press);
                _mousePressed = true;
            }
            else
            {
                if (_mousePressed)
                {
                    this.MouseAction.Invoke(Define.EMouseEvent.Click);
                }
                _mousePressed = false;
            }
        }
    }
}