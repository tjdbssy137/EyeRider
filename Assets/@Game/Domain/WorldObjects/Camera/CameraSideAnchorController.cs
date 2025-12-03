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
    private float _currentLocalZ;
    private float _smoothVelZ;

    private float _forwardDamping = 0.7f;
    private float _forwardAmplitude = 0.8f;

    private float _followFactor = 0.5f;
    private float _edgeFalloffPower = 1.5f; // limit 근처에서 감쇠되는 정도


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

        float t = Mathf.Clamp01(Mathf.Abs(lateral) / _sideLimit);
        float edgeFalloff = Mathf.Pow(1f - t, _edgeFalloffPower); //중심에서 1, limit 근처 0 에 가까워짐

        float targetX = lateral * _followFactor * edgeFalloff;

        _currentLocalX = Mathf.SmoothDamp(
            _currentLocalX,
            targetX,
            ref _smoothVel,
            1f / _damping
        );

        float speed = Contexts.Car.VerticalAccelerationSpeed;
        float targetZ = -speed * _forwardAmplitude;

        _currentLocalZ = Mathf.SmoothDamp(
            _currentLocalZ,
            targetZ,
            ref _smoothVelZ,
            1f / _forwardDamping
        );

        Vector3 lp = transform.localPosition;
        lp.x = _currentLocalX;
        lp.z = _currentLocalZ;
        transform.localPosition = lp;
    }

}
