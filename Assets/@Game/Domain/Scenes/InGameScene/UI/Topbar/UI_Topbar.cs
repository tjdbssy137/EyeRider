using UnityEngine;
using UniRx;

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
        _conditionPanel = GetObject((int)Objects.Conditions).GetComponent<UI_FilledPanel>();
        _conditionPanel.Init();

        _fuelPanel = GetObject((int)Objects.Fuels).GetComponent<UI_FilledPanel>();
        _fuelPanel.Init();

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
        float ratio = progress / Managers.Difficulty.MaxMetre;
        GetSlider((int)Sliders.GameProgressBar).value = Mathf.Clamp01(ratio);
    }
}