using UniRx.Triggers;
using UnityEngine;
using UniRx;
using System;

public class CameraSideAnchorController : BaseObject
{
    private Transform _parentTransform;
    private Vector3 _center;
    private float _sideLimit = 15f;
    private float _damping = 10f;

    private float _currentLocalX;
    private float _smoothVel;
    public override bool Init()
    {
        if (base.Init() == false)
            return false;


        return true;
    }

    public override bool OnSpawn()
    {
        if (false == base.OnSpawn())
        {
            return false;
        }

        Contexts.InGame.CurrentMapXZ
           .Subscribe(pos =>
           {
               _center = pos;
           })
           .AddTo(_disposables);
        _parentTransform = Contexts.InGame.Car.transform;

        this.FixedUpdateAsObservable()
            .Subscribe(_ =>
            {
                CaculateDistance();
            }).AddTo(_disposables);

        return true;
    }
    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
    }


    private void CaculateDistance()
    {
        float lateral = Vector3.Dot((_center - _parentTransform.position), _parentTransform.right);

        float clampedX = Mathf.Clamp(lateral, -_sideLimit, _sideLimit);

        _currentLocalX = Mathf.SmoothDamp(
            _currentLocalX,
            clampedX,
            ref _smoothVel,
            1f / _damping
        );

        Vector3 lp = transform.localPosition;
        lp.x = _currentLocalX;
        transform.localPosition = lp;
    }
}
