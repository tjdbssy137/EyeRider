using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class UI_Topbar : UI_Base
{
    private enum Objects
    {
        Conditions,
        Fuels,
    }
    private enum Sliders
    {
        GameProgressBar
    }
    private enum Images
    {
        PuaseButton
    }

    private UI_FilledPanel _conditionPanel;
    private UI_FilledPanel _fuelPanel;

    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        BindObjects(typeof(Objects));
        BindSliders(typeof(Sliders));
        BindImages(typeof(Images));

        _conditionPanel = GetObject((int)Objects.Conditions).GetComponent<UI_FilledPanel>();
        _conditionPanel.Init();

        _fuelPanel = GetObject((int)Objects.Fuels).GetComponent<UI_FilledPanel>();
        _fuelPanel.Init();

        GetSlider((int)Sliders.GameProgressBar).value = 0;
        GetImage((int)Images.PuaseButton).gameObject.BindEvent(OnClick_PuaseButton, EUIEvent.Click);

        Contexts.InGame.Car.OnFuelChanged
            .Subscribe(val =>
            {
                float current = val.Item2;
                float max = Contexts.Car.MaxFuel;
                _fuelPanel.UpdateValue(current, max);
            })
            .AddTo(this);

        Contexts.InGame.Car.OnConditionChanged
            .Subscribe(val =>
            {
                float current = val.Item2;
                float max = Contexts.Car.MaxCondition;
                _conditionPanel.UpdateValue(current, max);
            })
            .AddTo(this);
        Observable.EveryUpdate()
            .Subscribe(_ =>
            {
                //Debug.Log("EveryUpdate");
                UpdateGameProgress(Contexts.InGame.Metre);
            }).AddTo(this);
        return true;
    }

    public void UpdateGameProgress(float progress)
    {
        //Debug.Log($"Context.InGame.Metre: {Contexts.InGame.Metre}");   
        float ratio = progress / Managers.Difficulty.MaxMetre;
        GetSlider((int)Sliders.GameProgressBar).value = Mathf.Clamp01(ratio);
    }

    private void OnClick_PuaseButton(PointerEventData eventData)
    {
        Sequence seq = DOTween.Sequence().SetLink(GetImage((int)Images.PuaseButton).gameObject);

        seq.Append(GetImage((int)Images.PuaseButton).gameObject.transform.DOScale(0.9f, 0.05f).SetEase(Ease.OutQuad));
        seq.Append(GetImage((int)Images.PuaseButton).gameObject.transform.DOScale(1f, 0.05f).SetEase(Ease.OutQuad));

        seq.OnComplete(() =>
        {
            Managers.UI.ShowPopupUI<UI_Puase>();
        });

    }
}