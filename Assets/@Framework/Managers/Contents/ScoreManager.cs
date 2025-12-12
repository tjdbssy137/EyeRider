using UnityEngine;
using UniRx;
using System;

public class ScoreManager : MonoBehaviour
{
    private float _playStartTime;
    private float _lastEyeStateTime;

    private float _eyeInsideTime;
    private float _eyeOutsideTime;

    private bool _isInsideEye = true;

    private int _collisionCount;
    private float _panicAccum;
    private int _panicSampleCount;

    public int FinalScore { get; private set; }
    public int Star { get; private set; }

    private const float EyeBonusMul = 0.5f;
    private const float StabilityBonusMul = 0.3f;
    private const float CollisionPenaltyMul = 0.05f;
    public int FinalGold { get; private set; }

    private const float DistanceGoldFactor = 0.08f;
    private const float TimeGoldFactor = 0.5f;

    private const float FailGoldMul = 0.6f;

    public void Init()
    {
        Bind();
    }

    private void Bind()
    {
        Contexts.InGame.OnStartGame
            .Subscribe(_ =>
            {
                ResetData();
            })
            .AddTo(this);

        Contexts.InGame.OnEnterEye
            .Subscribe(_ =>
            {
                if (!_isInsideEye)
                {
                    _eyeOutsideTime += Time.time - _lastEyeStateTime;
                    _lastEyeStateTime = Time.time;
                    _isInsideEye = true;
                }
            })
            .AddTo(this);

        Contexts.InGame.OnExitEye
            .Subscribe(_ =>
            {
                if (_isInsideEye)
                {
                    _eyeInsideTime += Time.time - _lastEyeStateTime;
                    _lastEyeStateTime = Time.time;
                    _isInsideEye = false;
                }
            })
            .AddTo(this);

        // 충돌 카운트
        Contexts.InGame.OnCollisionObstacle
            .Subscribe(_ =>
            {
                _collisionCount++;
            })
            .AddTo(this);

        // Panic 누적
        Observable.Interval(TimeSpan.FromSeconds(0.2f))
            .Subscribe(_ =>
            {
                if (Contexts.InGame.IsPaused || Contexts.InGame.IsEnd)
                {
                    return;
                }

                _panicAccum += Contexts.InGame.PanicPoint;
                _panicSampleCount++;
            })
            .AddTo(this);

        Contexts.InGame.OnEndGame
            .Subscribe(_ =>
            {
                CalculateFinalScore();
                CalculateStar();
                CalculateFinalGold();
            })
            .AddTo(this);
    }

    private void ResetData()
    {
        _playStartTime = Time.time;
        _lastEyeStateTime = Time.time;

        _eyeInsideTime = 0f;
        _eyeOutsideTime = 0f;

        _isInsideEye = true;

        _collisionCount = 0;
        _panicAccum = 0f;
        _panicSampleCount = 0;

        FinalScore = 0;
        Star = 0;
        FinalGold = 0;
    }
    public void GetResult()
    {
        CalculateFinalScore();
        CalculateStar();
        CalculateFinalGold();
    }
    private void CalculateFinalScore()
    {
        float metre = Contexts.InGame.Metre;
        int distanceScore = Mathf.FloorToInt(metre);

        // 마지막 Eye 상태 보정
        if (_isInsideEye)
        {
            _eyeInsideTime += Time.time - _lastEyeStateTime;
        }
        else
        {
            _eyeOutsideTime += Time.time - _lastEyeStateTime;
        }

        float totalTime = _eyeInsideTime + _eyeOutsideTime;
        float eyeRatio = 0 < totalTime ? _eyeInsideTime / totalTime : 0f;

        float avgPanic = 0 < _panicSampleCount ? _panicAccum / _panicSampleCount : 0f;

        float eyeBonus = distanceScore * eyeRatio * EyeBonusMul;
        float stabilityBonus = distanceScore * (1f - avgPanic) * StabilityBonusMul;
        float collisionPenalty = distanceScore * CollisionPenaltyMul * _collisionCount;

        FinalScore = Mathf.Max(0,
            Mathf.RoundToInt(
                distanceScore
                + eyeBonus
                + stabilityBonus
                - collisionPenalty
            )
        );

        Debug.Log(
            $"[Score] Dist:{distanceScore} Eye:{eyeBonus:F0} Stability:{stabilityBonus:F0} " +
            $"Collision:-{collisionPenalty:F0} => Final:{FinalScore}"
        );
    }

    private void CalculateStar()
    {
        float metre = Contexts.InGame.Metre;

        float avgPanic = 0 < _panicSampleCount ? _panicAccum / _panicSampleCount : 1f;

        float totalTime = _eyeInsideTime + _eyeOutsideTime;
        float eyeRatio = 0 < totalTime ? _eyeInsideTime / totalTime : 0f;

        if (Managers.Difficulty.MaxMetre <= metre&&
            0.8f <= eyeRatio &&
            avgPanic <= 0.25f &&
            _collisionCount <= 1)
        {
            Star = 3;
            return;
        }

        if (Managers.Difficulty.MaxMetre <= metre &&
            0.5 <= eyeRatio &&
            avgPanic <= 0.5f &&
            _collisionCount <= 3)
        {
            Star = 2;
            return;
        }

        if (Managers.Difficulty.MaxMetre <= metre)
        {
            Star = 1;
            return;
        }

        Star = 0;
    }
    private void CalculateFinalGold()
    {
        float metre = Contexts.InGame.Metre;

        // 마지막 Eye 상태 보정 (Score와 동일한 타이밍 보장)
        if (_isInsideEye)
        {
            _eyeInsideTime += Time.time - _lastEyeStateTime;
        }
        else
        {
            _eyeOutsideTime += Time.time - _lastEyeStateTime;
        }

        float totalTime = _eyeInsideTime + _eyeOutsideTime;
        float playTime = totalTime;

        float baseGold = (metre * DistanceGoldFactor) + (playTime * TimeGoldFactor);

        float difficultyMul =
            1f + (Managers.Difficulty.Current.Level * 0.02f);

        float starMul = 1f;
        switch (Star)
        {
            case 2: starMul = 1.3f; break;
            case 3: starMul = 1.7f; break;
        }

        float riskRatio = 0 < totalTime ? _eyeOutsideTime / totalTime : 0f;
        float riskBonusMul = 1f + Mathf.Clamp01(riskRatio) * 0.3f;

        float finalGold =
            baseGold *
            difficultyMul *
            starMul *
            riskBonusMul;

        if (Contexts.InGame.IsEnd)
        {
            finalGold *= FailGoldMul;
        }

        FinalGold = Mathf.Max(0, Mathf.RoundToInt(finalGold));
    }

}
