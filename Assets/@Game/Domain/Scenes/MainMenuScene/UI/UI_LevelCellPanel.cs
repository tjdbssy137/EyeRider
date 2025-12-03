using System.Collections;
using System.Collections.Generic;
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
    private List<GameObject> _itemList = new List<GameObject>();

    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }
        BindObjects(typeof(GameObjects));
        _levelCellRoot = GetObject((int)GameObjects.LevelCellRoot).transform;
        
        // ScrollRect 컴포넌트 찾기 (Level Panel 하위에 있다고 가정)
        _scrollRect = GetComponentInChildren<ScrollRect>();
        
        return true;
    }
    
    private void OnEnable()
    {
        // 아이템이 세팅된 후 스크롤 위치 조정
        //StartCoroutine(DelayedScrollToTarget());
    }
    
    private void SetLevelCells()
    {
        AllPush();
        
        foreach (var item in Managers.Data.DifficultyDic.Values)
        { 
            SpawnItem(item.Level);
        }
        
        // 아이템 생성 직후 스크롤 위치 조정
        //ScrollToCurrentEvolutionLevel();
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
    
    // private void ScrollToCurrentEvolutionLevel()
    // {
    //     if (_scrollRect == null) 
    //     {
    //         return;
    //     }
    //     // 이전 스크롤 코루틴이 있다면 중지
    //     if (_scrollCoroutine != null)
    //     {
    //         StopCoroutine(_scrollCoroutine);
    //     }
        
    //     // 새로운 스크롤 코루틴 시작
    //     _scrollCoroutine = StartCoroutine(ScrollToTargetCoroutine());
    // }
    
    // private IEnumerator DelayedScrollToTarget()
    // {
    //     yield return null;
    //     ScrollToCurrentEvolutionLevel();
    // }
    // private IEnumerator ScrollToTargetCoroutine()
    // {
    //     yield return null;
        
    //     // 현재 유저의 진화 레벨
    //     int currentLevel = Managers.Game.UserInfo.EvolutionSetLevel;
        
    //     // 아이템이 역순으로 정렬되어 있으므로 인덱스 계산을 변경
    //     // 총 아이템 개수 - 현재 레벨 - 1
    //     int totalItems = _scrollRect.content.childCount;
    //     int targetIndex = totalItems - currentLevel - 1;
        
    //     RectTransform content = _scrollRect.content;
        
    //     if (targetIndex >= 0 && targetIndex < content.childCount)
    //     {
    //         RectTransform targetItem = content.GetChild(targetIndex).GetComponent<RectTransform>();

    //         float targetPosition = CalculateTargetPosition(targetItem, content);
            
    //         float startPosition = _scrollRect.verticalNormalizedPosition;
            
    //         float elapsedTime = 0f;
    //         float duration = 0.3f;
            
    //         while (elapsedTime < duration)
    //         {
    //             elapsedTime += Time.deltaTime;
    //             float t = elapsedTime / duration;
                
    //             float smoothT = t * t * (3f - 2f * t);
                
    //             float newPosition = Mathf.Lerp(startPosition, targetPosition, smoothT);
    //             _scrollRect.verticalNormalizedPosition = newPosition;
                
    //             yield return null;
    //         }
    //         _scrollRect.verticalNormalizedPosition = targetPosition;
    //     }
    // }
    
    // private float CalculateTargetPosition(RectTransform targetItem, RectTransform content)
    // {
    //     float contentHeight = content.rect.height;
        
    //     float viewportHeight = _scrollRect.viewport.rect.height;
        
    //     float scrollableHeight = contentHeight - viewportHeight;
        
    //     if (scrollableHeight <= 0) 
    //     {
    //         return 0f;
    //     }
        
    //     float targetPositionFromTop = -targetItem.anchoredPosition.y;
        
    //     float targetCenter = targetPositionFromTop - (viewportHeight / 2) + (targetItem.rect.height / 2);
        
    //     float normalizedPosition = 1 - (targetCenter / scrollableHeight);
        
    //     normalizedPosition = Mathf.Clamp01(normalizedPosition);
        
    //     return normalizedPosition;
    // }
}