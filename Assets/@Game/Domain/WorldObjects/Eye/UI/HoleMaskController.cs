using UnityEngine;
using UnityEngine.UI;

public class HoleMaskController : InitBase
{
    [Range(0f, 1f)]
    public float _backgroundAlpha = 1f;
    public Image _backgroundImage;
    public RectTransform _point; // 움직이는 점
    public RectTransform _canvasRect;

    void Update()
    {
        if (_backgroundImage != null && _backgroundImage.material != null)
        {
            _backgroundImage.material.SetFloat("_BackgroundAlpha", _backgroundAlpha);
        }

        Vector2 pointPos = RectTransformUtility.WorldToScreenPoint(null, _point.position);
        Vector2 uvPos = new Vector2(pointPos.x / _canvasRect.rect.width, pointPos.y / _canvasRect.rect.height);

        Vector2 pointSize = new Vector2(_point.rect.width / _canvasRect.rect.width, _point.rect.height / _canvasRect.rect.height);

        _backgroundImage.material.SetVector("_HolePos", uvPos);
        _backgroundImage.material.SetVector("_HoleSize", pointSize);
    }
}
