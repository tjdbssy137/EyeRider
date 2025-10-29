using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PointMover : UI_Base
{
    public RectTransform _point;
    public RectTransform _canvasRect;
    public float _moveSpeed = 200f;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        
        
        return true;
    }

    void Update()
    {
        Vector2 delta = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) delta.y += 1;
        if (Keyboard.current.sKey.isPressed) delta.y -= 1;
        if (Keyboard.current.aKey.isPressed) delta.x -= 1;
        if (Keyboard.current.dKey.isPressed) delta.x += 1;

        delta.Normalize();
        Vector2 pos = _point.anchoredPosition + delta * _moveSpeed * Time.deltaTime;

        // 캔버스 범위 Clamp
        float halfWidth = _canvasRect.rect.width * 0.5f;
        float halfHeight = _canvasRect.rect.height * 0.5f;

        pos.x = Mathf.Clamp(pos.x, -halfWidth, halfWidth);
        pos.y = Mathf.Clamp(pos.y, -halfHeight, halfHeight);

        _point.anchoredPosition = pos;
    }
}
