using System.Threading;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using DG.Tweening;

public class UI_Car : UI_Base
{
    private enum Images
    {
        In_Condition,
        In_Fuel,
    }

    private Canvas _worldCanvas;
    private Camera _mainCam;

    private Tween _conditionTween;
    private Tween _fuelTween;
    public override bool Init()
    {
        if (base.Init() == false)
			return false;

        BindImages(typeof(Images));

        _worldCanvas = GetComponent<Canvas>();
        _mainCam = Camera.main;
        _worldCanvas.worldCamera = _mainCam;

        this.UpdateAsObservable().Subscribe(_=>
        {
            CanvasUpdate();
        }).AddTo(this);

		return true;
    }

    public void SetInfo(bool can)
    {
        if(!can)
        {
            return;
        }
        Contexts.InGame.Car.OnConditionChanged
        .Subscribe(newCondition =>
        {
            float target = newCondition.Item2 / 100f;
            _conditionTween?.Kill();
            _conditionTween = GetImage((int)Images.In_Condition).DOFillAmount(target, 0.5f).SetEase(Ease.OutCubic);
        }).AddTo(this);

        Contexts.InGame.Car.OnFuelChanged
        .Subscribe(newFuel =>
        {
            float target = newFuel.Item2 / 100f;
            _fuelTween?.Kill();
            _fuelTween = GetImage((int)Images.In_Fuel).DOFillAmount(target, 0.5f).SetEase(Ease.OutCubic);
        }).AddTo(this);
    }


    void CanvasUpdate()
    {
        if (_mainCam == null) return;

        Vector3 fwd = _mainCam.transform.forward;

        if (fwd.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(fwd, Vector3.up);

        transform.rotation = targetRot;
    }
}
