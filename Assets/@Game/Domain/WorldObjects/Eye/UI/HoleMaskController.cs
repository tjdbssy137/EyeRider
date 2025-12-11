using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class HoleMaskController : UI_Base
{
    [Range(0f, 1f)]
    public float _backgroundAlpha = 1f;
    public Image _backgroundImage;
    public RectTransform _point; // 움직이는 점
    public RectTransform _canvasRect;
    public RectTransform _eyeImages;

    public float _rotateSpeed = 15f;

    public override bool Init()
    {
        if (!base.Init())
            return false;

        Vector2 canvasSize = new Vector2(_canvasRect.rect.width, _canvasRect.rect.height);
        _backgroundImage.material.SetVector("_CanvasSize", canvasSize);

        _backgroundImage.material.SetFloat("_BackgroundAlpha", _backgroundAlpha);

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                if (true == Contexts.InGame.IsGameOver)
                {
                    return;
                }
                if (true == Contexts.InGame.IsPaused)
                {
                    return;
                }
                Move();
                RotateEyeCloud();
            })
            .AddTo(this);

        return true;
    }

    private void Move()
    {
        if (_backgroundImage.material == null)
            return;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, _point.position);

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect, screenPos, null, out localPos
        );

        Vector2 canvasPos = localPos + _canvasRect.rect.size * 0.5f;

        _backgroundImage.material.SetVector("_CenterPx", canvasPos);

        float radiusPx = _point.rect.width * _point.lossyScale.x * 0.4f; 
        _backgroundImage.material.SetFloat("_RadiusPx", radiusPx);

        // float featherPx = radiusPx * 0.3f;
        // _backgroundImage.material.SetFloat("_FeatherPx", featherPx);

        _backgroundImage.material.SetVector("_HoleSize", new Vector2(1f, 1f));
    }



    private void RotateEyeCloud()
    {
        _eyeImages.Rotate(0f, 0f, _rotateSpeed * Time.deltaTime);
    }

}
