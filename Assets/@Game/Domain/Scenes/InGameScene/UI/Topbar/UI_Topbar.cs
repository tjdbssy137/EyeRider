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
        var btnObj = GetObject((int)Objects.PuaseButton).gameObject;
        var btnTransform = btnObj.transform;

        btnTransform.DOKill();
        btnTransform.localScale = Vector3.one;

        btnTransform.DOPunchScale(new Vector3(-0.2f, -0.2f, 0), 0.1f, vibrato: 1)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                Managers.UI.ShowPopupUI<UI_Puase>();
            });
    }
}