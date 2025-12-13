using UniRx.Triggers;
using UnityEngine;
using UniRx;
using System;

public partial class CarController : BaseObject
{
    private void BindSubscriptions()
    {
        Contexts.InGame.OnEnterCorner
            .Subscribe(deg =>
            {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;

                Vector3 f = transform.forward;
                f.y = 0f;
                transform.forward = f.normalized;

                Vector3 r = transform.right;
                r.y = 0f;
                transform.right = r.normalized;

                _pendingRotation = true;
                _pendingDegrees = deg;

                Observable.Timer(TimeSpan.FromSeconds(0.1f))
                    .Subscribe(_ =>
                    {
                        if (_pendingRotation)
                        {
                            _pendingRotation = false;
                            this.Steer(_pendingDegrees);
                        }
                    })
                    .AddTo(_disposables);
            })
            .AddTo(_disposables);


        Contexts.InGame.OnExitEye
            .Subscribe(dist =>
            {
                _isOutside = true;
                _lastDistance= dist;
                _distancePanic = Mathf.Clamp01(_lastDistance/100);
            })
            .AddTo(_disposables);

        Contexts.InGame.OnEnterEye
            .Subscribe(_ =>
            {
                _isOutside = false;
                _lastDistance = 0;
                _animator.SetFloat("Distance", 0f); 
            })
            .AddTo(_disposables);

        Observable.Interval(TimeSpan.FromSeconds(0.2f))
            .Subscribe(_ =>
            {
                if (true == Contexts.InGame.IsEnd)
                {
                    return;
                }
                if (true == Contexts.InGame.IsPaused)
                {
                    return;
                }
                if (true == _isOutside)
                {
                    float dmg = DistancePenalty(_lastDistance);
                    Contexts.InGame.Car.DamageCondition(dmg);
                }
            })
            .AddTo(_disposables);

        Contexts.InGame.OnCollisionObstacle
        .Subscribe(damage =>
        {
            Contexts.InGame.IsCollisionObstacle++;
            DamageEffect effect = Managers.Object.Spawn<DamageEffect>($"DamageEffect", Contexts.InGame.Car.transform.position, 0, 0);
            effect.SetDamage(damage);
            _eventPanic += 0.2f; // 임시값
            Contexts.InGame.Car.DamageCondition(damage);
            Observable.Timer(TimeSpan.FromSeconds(1.2f))
                .Subscribe(_ =>
                {
                    _eventPanic -= 0.2f;
                    Contexts.InGame.IsCollisionObstacle--;
                    Contexts.InGame.IsCollisionObstacle = Mathf.Max(0, Contexts.InGame.IsCollisionObstacle);
                })
                .AddTo(_disposables);
        })
        .AddTo(_disposables);

        Contexts.InGame.Car.OnConditionChanged
        .Subscribe(newCondition =>
        {
            if (newCondition.Item2 <= 0)
            {
                Contexts.InGame.OnEndGame.OnNext(InGameContext.GameEndType.Lose);
            }
            _conditionPanic = Mathf.Clamp01(1 - newCondition.Item2/100);
        }).AddTo(this);

        Contexts.InGame.Car.OnFuelChanged
        .Subscribe(newFuel =>
        {
            if(newFuel.Item2 <= 0)
            {
                Contexts.InGame.OnEndGame.OnNext(InGameContext.GameEndType.Lose);
            }
            _fuelPanic = Mathf.Clamp01(1 - newFuel.Item2/100);
        }).AddTo(this);
    }
    
    public float DistancePenalty(float distance)
    {
        float target = 0f;
        
        //distance -= + Managers.Difficulty.TimeDifficulty;

        if(distance <= 30)
        {
            target = 0.2f;
        }
        else if(distance <= 70)
        {
            target = 0.4f;
        }
        else if(distance <= 90)
        {
            target = 0.6f;
        }
        else if(distance <= 100)
        {
            target = 0.8f;
        }
        else
        {
            target = 1f;
        }
        target = Mathf.Clamp(target, 0f, _maxShakeIntensity);

        _shakeIntensity = Mathf.Lerp(_shakeIntensity, target, Time.deltaTime * _shakeLerpSpeed);

        float normalizedValue = Mathf.InverseLerp(0f, 1f, target);
        _animator.SetFloat("Distance", normalizedValue);

        _controlDifficulty = normalizedValue;
        float baseDamage = 10 * target;

        // 막판엔 데미지 감소
        baseDamage *= Managers.Difficulty.EndGamePenaltyMul;
        return baseDamage;
    }

    private void WheelEffect(bool drifting)
    {
        if (drifting)
        {
            _RLWParticleSystem.Play();
            _RRWParticleSystem.Play();
        }
        else
        {
            _RLWParticleSystem.Stop();
            _RRWParticleSystem.Stop();
        }
    }

    private void PanicPointCaculator()
    {
        Contexts.InGame.PanicPoint = Mathf.Clamp01(_distancePanic + _eventPanic + _conditionPanic * 0.3f + _fuelPanic * 0.3f);
        //Debug.Log($"_distancePanic : {_distancePanic}, _eventPanic : {_eventPanic},  _conditionPanic : {_conditionPanic}, _fuelPanic : {_fuelPanic},PanicPoint : {Contexts.InGame.PanicPoint}");
    }
}