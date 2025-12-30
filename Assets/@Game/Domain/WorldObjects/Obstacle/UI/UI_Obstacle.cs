using System;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;

public class UI_Obstacle : UI_Base
{
    private enum Images
    {
        ObstacleIcon
    }
    private ObstacleData _obstacleData;
    public ObstacleData ObstacleData => _obstacleData;
    private float _accumulatedAngle = 0f;
    private float _startOffsetAngle = 0f;
    private bool _isLaunched = false;
    public  override bool Init()
    {
        if (base.Init() == false)
            return false;
        
        BindImages(typeof(Images));
        
        Observable.EveryUpdate().Subscribe(_ =>
        {
            if (Contexts.InGame.IsPaused)
            {
                return;
            }
            if (Contexts.InGame.IsEnd)
            {
                return;
            }
            float dt = Time.deltaTime;
            UpdateOrbit(60f, 100f, dt);
        }).AddTo(this);
        return true;
    }
    public void SetInfo(ObstacleData data)
    {
        if(data == null)
        {
            Debug.LogWarning("UI_Obstacle SetInfo data is NULL");
            return;
        }
        _isLaunched = false;
        _obstacleData = data;
        GetImage((int)Images.ObstacleIcon).sprite = _obstacleData.Sprite;
        _startOffsetAngle = Contexts.InGame.ObstacleQueue.Count * 90f;
        _accumulatedAngle = 0f;
    }

    public void UpdateOrbit(float radius, float speed, float dt)
    {
        if (_isLaunched)
        {
            return;
        }
        _accumulatedAngle += speed * dt;

        float currentAngle = _startOffsetAngle + _accumulatedAngle;
        float rad = currentAngle * Mathf.Deg2Rad;

        // »ï°¢ÇÔ¼ö¸¦ ÀÌ¿ëÇÑ ±Ëµµ ÁÂÇ¥ °è»ê
        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;

        (transform as RectTransform).anchoredPosition = offset;

        if (1440f <= _accumulatedAngle)
        {
            _isLaunched = true;
            Contexts.InGame.OnSpawnMissile.OnNext(ObstacleData);
            if (0 < Contexts.InGame.ObstacleQueue.Count)
            {
                Contexts.InGame.ObstacleQueue.Dequeue();
            }
            Managers.Resource.Destroy(this.gameObject);
        }
    }

}
