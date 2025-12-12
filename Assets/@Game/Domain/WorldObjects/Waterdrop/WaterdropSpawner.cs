using System;
using UnityEngine;
using UniRx;

public class WaterdropSpawner : BaseObject
{
    private Camera _camera;
    private Transform _spawnTransform;
    private readonly Vector2 BaseDir = new Vector2(-0.4f, -1f).normalized;
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
        if(_camera == null)
        {
            Debug.LogError("Camera is NULL");
        }
        _spawnTransform = _camera.transform.Find("WeatherBox");


        return true;
    }

    public override bool OnSpawn()
    {
        if (false == base.OnSpawn())
        {
            return false;
        }

        Managers.Difficulty.OnMetreDifficultyUp.Subscribe(_ =>
        {
            Observable.Interval(TimeSpan.FromSeconds(0.035f))
                .TakeUntil(Observable.Timer(TimeSpan.FromSeconds(2f)))
                .Subscribe(_ =>
                {
                    SpawnRainDrop();
                })
                .AddTo(_disposables);
        }).AddTo(this.gameObject);

        return true;
    }

    private void SpawnRainDrop()
    {
        Vector2 spawnPos = new Vector2(UnityEngine.Random.Range(2f, 12f), 22);
        Waterdrop drop = Managers.Object.Spawn<Waterdrop>(spawnPos, 0, 0, _spawnTransform);
        drop.transform.SetParent(_spawnTransform, false);
        RectTransform rt = drop.transform as RectTransform;

        float speed = UnityEngine.Random.Range(20f, 40f);

        drop.SetInfo(spawnPos, BaseDir, speed);
    }

}
