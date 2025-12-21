using System;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;

public class UI_Obstacle : UI_Popup
{
    private enum Images
    {
        ObstacleIcon
    }
    private ObstacleData _obstacleData;
    private float _accumulatedAngle = 0f;
    private float _startOffsetAngle = 0f;
    private bool _isLaunched = false;
    public  override bool Init()
    {
        if (base.Init() == false)
            return false;
        
        BindImages(typeof(Images));
        
        Contexts.InGame.OnSpawnMissile
            .Subscribe(_ => 
            {
                Contexts.InGame.ObstacleQueue.Enqueue(this);
                Managers.UI.ClosePopupUI(this);
                //Managers.Resource.Destroy(gameObject);
            }).AddTo(this);

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
            UpdateOrbit(transform.parent.position, 60f, 100f, dt);
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

    public bool UpdateOrbit(Vector2 centerPos, float radius, float speed, float dt)
    {
        _accumulatedAngle = speed * dt;

        float currentAngle = _startOffsetAngle + _accumulatedAngle;
        float rad = currentAngle * Mathf.Deg2Rad;

        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        (transform as RectTransform).anchoredPosition = centerPos + offset;

        if (1440f <= _accumulatedAngle)
        {
            if(_isLaunched)
            {
                return false;
            }
            _isLaunched = true;
            Contexts.InGame.OnSpawnMissile.OnNext(_obstacleData);
            return true;
        }
        return false;
    }
}
