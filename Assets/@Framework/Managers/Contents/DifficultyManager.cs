using UnityEngine;
using UniRx;
using System;

public class DifficultyManager
{
    public Subject<Unit> OnMetreDifficultyUp = new Subject<Unit>();

    private float _currentMetre;
    private float _maxMetre;
    public float MaxMetre 
    {
        get { return _maxMetre; }
    }
    private AnimationCurve _metreCurve;

    private float _endGamePenaltyMul = 1f;
    public float EndGamePenaltyMul
    {
        get { return _endGamePenaltyMul; }
    }

    public Data.DifficultyData Current { get; private set; }

    public AnimationCurve LevelCurve_Random = AnimationCurve.Linear(0f, 1f, 1f, 1.5f);
    public AnimationCurve LevelCurve_Approach = AnimationCurve.Linear(0f, 1f, 1f, 1.3f);
    public AnimationCurve LevelCurve_Repel = AnimationCurve.Linear(0f, 1f, 1f, 1.3f);

    public float CurrentLevel01
    {
        get
        {
            return Mathf.Clamp01((float)(Contexts.GameProfile.CurrentLevel - 1) / (Contexts.InGame.MaxLevel - 1));
        }
    }

    public float Metre01
    {
        get
        {
            if (_maxMetre <= 0f)
            {
                return 0f;
            }
            return Mathf.Clamp01(_currentMetre / _maxMetre);
        }
    }

    public float MetreDifficulty
    {
        get { return _metreCurve.Evaluate(Metre01); }
    }

    public float StormSpeed
    {
        get { return Current.StormSpeed * MetreDifficulty; }
    }

    public float ObstacleDensity
    {
        get { return Current.ObstacleDensity * MetreDifficulty; }
    }

    public float EyeSize
    {
        get { return Mathf.Lerp(Current.EyeSize, Current.EyeSize - 1.7f, MetreDifficulty); }
    }

    public float PM_RandomMul
    {
        get
        {
            return Current.RandomMoveMul
                * (1f + MetreDifficulty * 0.2f)
                * LevelCurve_Random.Evaluate(CurrentLevel01);
        }
    }

    public float PM_ApproachMul
    {
        get
        {
            return Current.ApproachMul
                * (1f + MetreDifficulty * 0.15f)
                * LevelCurve_Approach.Evaluate(CurrentLevel01);
        }
    }

    public float PM_RepelMul
    {
        get
        {
            return Current.RepelMul
                * (1f + MetreDifficulty * 0.15f)
                * LevelCurve_Repel.Evaluate(CurrentLevel01);
        }
    }


    public void Init()
    {
        _currentMetre = 0f;
        _metreCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    }

    public void SetDifficult()
    {
        _currentMetre = 0f;

        int key = Contexts.GameProfile.CurrentLevel + 10000;
        if (Managers.Data.DifficultyDic.ContainsKey(key) == true)
        {
            _maxMetre = Managers.Data.DifficultyDic[key].DistanceM;
        }
        else
        {
            _maxMetre = 800f;
        }

        _metreCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    }


    public void CurrentLevel(int level)
    {
        int key = 0;

        if (level <= 0)
        {
            key = 100001;
        }
        else
        {
            key = level + 10000;
        }

        if (Managers.Data.DifficultyDic.ContainsKey(key) == true)
        {
            Current = Managers.Data.DifficultyDic[key];
        }
    }


    public void UpdateMetre(float metre)
    {
        _currentMetre = metre;
        Debug.Log($"_currentMetre {_currentMetre}");
        //CheckLevelUp();
        //CheckEndPenalty();
    }


    private void CheckLevelUp()
    {
        float p = Metre01;

        if (0.25f <= p)
        {
            OnMetreDifficultyUp.OnNext(Unit.Default);
        }

        if (0.50f <= p)
        {
            OnMetreDifficultyUp.OnNext(Unit.Default);
        }

        if (0.75f <= p)
        {
            OnMetreDifficultyUp.OnNext(Unit.Default);
        }

        if (0.93f <= p)
        {
            OnMetreDifficultyUp.OnNext(Unit.Default);
        }

        if (1.0f <= p)
        {
            Debug.Log("Contexts.InGame.IsGameOver = true;");
            Contexts.InGame.OnEndGame.OnNext(Unit.Default);
            Managers.UI.ShowPopupUI<UI_ResultPopup>();
        }
    }


    private void CheckEndPenalty()
    {
        float p = Metre01;

        if (0.93f <= p)
        {
            float t = Mathf.InverseLerp(0.93f, 1f, p);
            _endGamePenaltyMul = Mathf.Lerp(1f, 0.6f, t);
        }
        else
        {
            _endGamePenaltyMul = 1f;
        }
    }
}
