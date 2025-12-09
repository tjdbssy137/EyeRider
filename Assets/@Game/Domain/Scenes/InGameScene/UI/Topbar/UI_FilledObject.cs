using UnityEngine;
using DG.Tweening;
public class UI_FilledObject : UI_Base
{
    private enum Images
    {
        BG,
        Filled
    }

    private Tween _tween;

    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        BindImages(typeof(Images));
        return true;
    }

    public void SetFill(float amount)
    {
        var img = GetImage((int)Images.Filled);
        if (img == null)
        {
            Debug.LogError("Filled is NULL");
            return;
        }

        _tween?.Kill();
        _tween = img
            .DOFillAmount(amount, 0.25f)
            .SetEase(Ease.OutCubic);
    }
}
