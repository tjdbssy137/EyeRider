using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class UI_Topbar : UI_Base
{
    private enum Objects
    {
        //Conditions,
        Fuels,
        PuaseButton
    }
    private enum Sliders
    {
        GameProgressBar
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

        //_conditionPanel = GetObject((int)Objects.Conditions).GetComponent<UI_FilledPanel>();
        //_conditionPanel.Init();

        _fuelPanel = GetObject((int)Objects.Fuels).GetComponent<UI_FilledPanel>();
        _fuelPanel.Init();

        GetSlider((int)Sliders.GameProgressBar).value = 0;
        GetObject((int)Objects.PuaseButton).gameObject.BindEvent(OnClick_PuaseButton, EUIEvent.Click);

        Contexts.InGame.Car.OnFuelChanged
            .Subscribe(val =>
            {
                float current = val.Item2;
                float max = Contexts.Car.MaxFuel;
                _fuelPanel.UpdateValue(current, max);
            })
            .AddTo(this);

        Observable.EveryUpdate()
            .Subscribe(_ =>
            {
                //Debug.Log("EveryUpdate");
                if (true == Contexts.InGame.IsEnd)
                {
                    return;
                }
                if (true == Contexts.InGame.IsPaused)
                {
                    return;
                }
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
        var btnTransform = GetObject((int)Objects.PuaseButton).transform;

        // 1. 기존 트윈 즉시 종료 및 스케일 초기화
        btnTransform.DOKill();
        btnTransform.localScale = Vector3.one;

        Sequence seq = DOTween.Sequence().SetLink(GetObject((int)Objects.PuaseButton).gameObject).SetUpdate(true); ;

        seq.Append(btnTransform.DOScale(0.8f, 0.05f).SetEase(Ease.OutQuad));
        seq.Append(btnTransform.DOScale(1f, 0.05f).SetEase(Ease.OutQuad));

        seq.OnComplete(() =>
        {
            Managers.UI.ShowPopupUI<UI_Puase>();
        });
    }
}