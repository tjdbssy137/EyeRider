using System.Collections;
using System.Collections.Generic;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_LevelCellPanel : UI_Base
{
    private enum GameObjects
    {
        LevelCellRoot,
    }
    
    private ScrollRect _scrollRect;
    private Transform _levelCellRoot = null;
    private VerticalLayoutGroup _layoutGroup;
    private List<GameObject> _itemList = new List<GameObject>();
    private float _amplitude = 200f;
    private float _frequency = 0.6f;

    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }
        BindObjects(typeof(GameObjects));
        _levelCellRoot = GetObject((int)GameObjects.LevelCellRoot).transform;
        
        _scrollRect = GetComponentInChildren<ScrollRect>();
        _layoutGroup = GetComponentInChildren<VerticalLayoutGroup>();
        return true;
    }
    
    public void SetInfo()
    {
        // 아이템이 세팅된 후 스크롤 위치 조정
       // Debug.Log($"setInfo");
        SetLevelCells();
    }
    
    private void SetLevelCells()
    {
        AllPush();
        
        foreach (var item in Managers.Data.DifficultyDic.Values)
        {
            //Debug.Log($"item : {item.Id}");
            SpawnItem(item.Level);
        }

        // 아이템 생성 직후 스크롤 위치 조정
        Observable.NextFrame()
        .Subscribe(_ =>
        {
            Canvas.ForceUpdateCanvases();
            ScrollToTargetCoroutine();
            //_scrollRect.verticalNormalizedPosition = 0f;
            ApplyWave();
        })
        .AddTo(this);
    }

    private void AllPush()
    {
        foreach(var _item in _itemList)
        {
            Managers.Resource.Destroy(_item.gameObject);
        }
        _itemList.Clear();
    }

    private void SpawnItem(int level)
    {
        var item = Managers.UI.MakeSubItem<UI_LevelCell>(parent: _levelCellRoot, pooling: true);
        int score = SecurePlayerPrefs.GetInt($"Level_{level}", 0);
        item.SetInfo(level, score);
        _itemList.Add(item.gameObject);
    }

    private void ApplyWave()
    {
        int index = 0;
        foreach (Transform t in _levelCellRoot)
        {
            if (!t.gameObject.activeSelf)
            {
                continue;
            }

            UI_LevelCell cell = t.GetComponent<UI_LevelCell>();
            if (cell == null)
            {
                continue;
            }

            float x = Mathf.Sin(index * _frequency) * _amplitude;
            cell.SetOffsetX(x);
            index++;
        }
    }

     private void ScrollToTargetCoroutine()
    {
        // 현재 유저 도전 단계
        //Debug.Log($"Contexts.GameProfile.NextLevel : {Contexts.GameProfile.NextLevel}");
        int targetIndex = Contexts.GameProfile.NextLevel;

        RectTransform content = _scrollRect.content;
        
        if (0 <= targetIndex && targetIndex < content.childCount)
        {
            RectTransform targetItem = content.GetChild(targetIndex).GetComponent<RectTransform>();

            float targetPosition = CalculateTargetPosition(targetItem, content);
            float startPosition = _scrollRect.verticalNormalizedPosition;
            float duration = 0.3f;
            float startTime = Time.time;

            Observable.EveryUpdate()
                .TakeWhile(_ => Time.time - startTime < duration)
                .Subscribe(_ =>
                {
                    float t = (Time.time - startTime) / duration;
                    float smooth = t * t * (3 - 2 * t);
                    _scrollRect.verticalNormalizedPosition = Mathf.Lerp(startPosition, targetPosition, smooth);
                },
                () => _scrollRect.verticalNormalizedPosition = targetPosition)
                .AddTo(this);
        }
    }
    
    private float CalculateTargetPosition(RectTransform targetItem, RectTransform content)
    {
        float contentHeight = content.rect.height;
        
        float viewportHeight = _scrollRect.viewport.rect.height;
        
        float scrollableHeight = contentHeight - viewportHeight;
        
        if (scrollableHeight <= 0) 
        {
            return 0f;
        }
        
        float targetPositionFromTop = -targetItem.anchoredPosition.y;
        
        float targetCenter = targetPositionFromTop - (viewportHeight / 2) + (targetItem.rect.height / 2);
        
        float normalizedPosition = 1 - (targetCenter / scrollableHeight);
        
        normalizedPosition = Mathf.Clamp01(normalizedPosition);
        
        return normalizedPosition;
    }
}