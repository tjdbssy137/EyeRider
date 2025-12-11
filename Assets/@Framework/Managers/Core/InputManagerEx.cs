using System;
using UnityEngine;
using UnityEngine.EventSystems;
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

        //if (!Mouse.current.leftButton.isPressed)
        //{
        //    bool isHover = false;

        //    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        //    {
        //        isHover = true;
        //    }
        //    else
        //    {
        //        Camera cam = Camera.main;
        //        if (cam != null)
        //        {
        //            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        //            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        //            {
        //                isHover = true;
        //            }
        //        }
        //    }

        //    if (isHover)
        //    {
        //        MouseAction.Invoke(Define.EMouseEvent.Hover);
        //    }
        //}
    }
}