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

    public Data.DifficultyData Current => Managers.Data.DifficultyDic[1]; // 시큐리티로 가져오기

    public void TimeLevelUp()
    {
        OnTimeDifficultyUp.OnNext(Unit.Default);
    }

    public float StormSpeed => Current.StormSpeed * TimeDifficulty;

    public float ObstacleDensity => Current.ObstacleDensity * TimeDifficulty;

    public float EyeSize => Current.EyeSize * (1f + TimeDifficulty * 0.1f);

    public float PM_RandomMul => Current.RandomMoveMul * (1f + TimeDifficulty * 0.2f);

    public float PM_ApproachMul => Current.ApproachMul * (1f + TimeDifficulty * 0.15f);

    public float PM_RepelMul => Current.RepelMul * (1f + TimeDifficulty * 0.15f);


    public void TimeToLevelup(float time)
    {
        if(time < 20)
        {
            TimeLevelUp();
        }
        else if(time < 40)
        {
            TimeLevelUp();
        }
        else if(time < 60)
        {
            TimeLevelUp();
        }
        else if(time < 75)
        {
            TimeLevelUp();
        }
    }
}
