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
            Managers.Sound.Play(Define.ESound.Hit, 0.5f, 0.8f);
            Contexts.InGame.IsCollisionObstacle++;
            _eventPanic += 0.2f; // 임시값
            Contexts.InGame.Car.DamageCondition(damage);
            Observable.Timer(TimeSpan.FromSeconds(1.2f))
                .Subscribe(_ =>
                {
                    _eventPanic -= 0.2f;
                    if (_eventPanic < 0)
                    {
                        _eventPanic = 0f;
                    }
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

        Contexts.InGame.OnCollisionMissile
            .Subscribe(damage =>
            {
                Managers.Sound.Play(Define.ESound.Hit, 0.5f, 0.8f);
                _eventPanic += 0.5f;
                Contexts.InGame.Car.DamageCondition(damage);
                Observable.Timer(TimeSpan.FromSeconds(1.0f))
                    .Subscribe(_ =>
                    {
                        _eventPanic -= 0.5f;
                        if (_eventPanic < 0)
                        {
                            _eventPanic = 0f;
                        }
                    })
                    .AddTo(_disposables);
            }).AddTo(_disposables);
    }
    
    private float GetDistanceStep(float distance)
    {
        if (distance <= 30)
        {
            return 0.2f;
        }
        else if (distance <= 70)
        {
            return 0.4f;
        }
        else if (distance <= 90)
        {
            return 0.6f;
        }
        else if (distance <= 100)
        {
            return 0.8f;
        }
        else
        {
            return 1f;
        }
    }
    public float DistancePenalty(float distance)
    {
        float step = GetDistanceStep(distance);

        _controlDifficulty = step;

        float baseDamage = 10 * step;
        baseDamage *= Managers.Difficulty.EndGamePenaltyMul;

        return baseDamage;
    }
    private void UpdateShakeAnimation()
    {
        // 거리로 인한 흔들림
        float distanceShake = _isOutside ? GetDistanceStep(_lastDistance) : 0f;

        // 흔들림 타겟 = 거리 흔들림 + 충돌 패닉 이벤트
        // _eventPanic이 UI용이라도, "충돌해서 패닉이 온 상황"이면 차가 흔들리는 게 자연스럽습니다.
        // 만약 너무 심하게 흔들리면 * 0.5f 등으로 조절
        float targetShake = distanceShake + _eventPanic;

        targetShake = Mathf.Clamp(targetShake, 0f, _maxShakeIntensity);
        _shakeIntensity = Mathf.Lerp(_shakeIntensity, targetShake, Time.deltaTime * _shakeLerpSpeed);

        float normalizedValue = Mathf.InverseLerp(0f, 1f, targetShake); // 혹은 그냥 targetShake 사용 (이미 0~1 범위 비슷하므로)
        _animator.SetFloat("Distance", normalizedValue);
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