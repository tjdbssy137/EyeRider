using UnityEngine;
using UniRx;
using System;

public class DifficultyManager
{
    public Subject<Unit> OnTimeDifficultyUp = new Subject<Unit>();

    public int LevelDifficulty { get; private set; }
    public float TimeDifficulty => _timeCurve.Evaluate(_elapsed / _maxTime);

    private float _elapsed;
    private float _maxTime = 80f;
    private AnimationCurve _timeCurve;

    public Data.DifficultyData Current { get; private set; } // 시큐리티로 가져오기

    public AnimationCurve LevelCurve_Random = AnimationCurve.Linear(0, 1, 1, 1.5f);
    public AnimationCurve LevelCurve_Approach = AnimationCurve.Linear(0, 1, 1, 1.3f);
    public AnimationCurve LevelCurve_Repel = AnimationCurve.Linear(0, 1, 1, 1.3f);

    public float CurrentLevel01 => Mathf.Clamp01((float)(Contexts.InGame.Level - 1) / (Contexts.InGame.MaxLevel - 1));

    public float StormSpeed => Current.StormSpeed * TimeDifficulty;
    public float ObstacleDensity => Current.ObstacleDensity * TimeDifficulty;
    public float EyeSize => Mathf.Lerp(Current.EyeSize, Current.EyeSize - 1.7f, TimeDifficulty);
    public float PM_RandomMul => Current.RandomMoveMul * (1f + TimeDifficulty * 0.2f) * LevelCurve_Random.Evaluate(CurrentLevel01);
    public float PM_ApproachMul => Current.ApproachMul * (1f + TimeDifficulty * 0.15f) * LevelCurve_Approach.Evaluate(CurrentLevel01);
    public float PM_RepelMul => Current.RepelMul * (1f + TimeDifficulty * 0.15f) * LevelCurve_Repel.Evaluate(CurrentLevel01);

    public void Init()
    {
        _elapsed = 0f;
        _maxTime = 80f;
        _timeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    }

    public void TimeLevelUp()
    {
        OnTimeDifficultyUp.OnNext(Unit.Default);
    }

    public void TimeToLevelup(float time)
    {
        _elapsed = time;
        if(_elapsed < 20)
        {
            TimeLevelUp();
        }
        else if(_elapsed < 40)
        {
            TimeLevelUp();
        }
        else if(_elapsed < 60)
        {
            TimeLevelUp();
        }
        else if(_elapsed < 75)
        {
            TimeLevelUp();
        }
    }

    public void CurrentLevel(int level)
    {
        if(level <= 0)
        {
            level = 100001;
        }
        else
        {
            level += 10000;
        }
        Current = Managers.Data.DifficultyDic[level];
    }

    private void TimeToEnd()
    {
        // 막판(80초)에 판정 후하게 해주도록 값 수정
    }
}
