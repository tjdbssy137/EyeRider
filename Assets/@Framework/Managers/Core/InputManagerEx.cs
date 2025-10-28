using UnityEngine;
using System;

public class InputManagerEx
{
    public Action KeyAction = null;
    public Action<Define.EMouseEvent> MouseAction = null;
    bool _mousePressed = false;

    public void OnUpdate()
    {
        if (Input.anyKey && KeyAction != null)
        {
            KeyAction?.Invoke();
        }

        if (MouseAction != null)
        {
            if (Input.GetMouseButtonDown(0))
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