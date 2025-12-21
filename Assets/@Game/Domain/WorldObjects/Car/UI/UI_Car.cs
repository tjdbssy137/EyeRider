using System.Threading;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using DG.Tweening;
using NUnit.Framework;

public class UI_Car : UI_Base
{
    private enum Images
    {
        Warning_Condition,
    }

    private Canvas _worldCanvas;
    private Camera _mainCam;

    public Sprite _turnOff;
    public Sprite _turnOn;
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

        Contexts.Car.IsCritical
            .DistinctUntilChanged()
            .Where(isCritical => isCritical == true) // true가 된 순간만 필터링
            .Subscribe(_ => 
            {
                var warningImg = GetImage((int)Images.Warning_Condition);

                Observable.Interval(System.TimeSpan.FromSeconds(0.5f))
                    .TakeUntilDisable(this)
                    .Subscribe(x => {
                        warningImg.sprite = (x % 2 == 0) ? _turnOn : _turnOff;
                    }).AddTo(this);
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
