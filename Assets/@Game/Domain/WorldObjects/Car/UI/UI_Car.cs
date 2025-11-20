using System.Threading;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class UI_Car : UI_Base
{
    private enum Images
    {
        In_Condition,
        In_Fuel,
    }

    Canvas _worldCanvas;
    Camera _mainCam;

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
            GetImage((int)Images.In_Condition).fillAmount = newCondition.Item2/100;
        }).AddTo(this);

        
        Contexts.InGame.Car.OnFuelChanged
        .Subscribe(newFuel =>
        {                
            GetImage((int)Images.In_Fuel).fillAmount = newFuel.Item2/100;
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
