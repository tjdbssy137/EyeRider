using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_LevelCell : UI_Base
{
    private enum StarsImage
    {
        Star1, //0
        Star2,//1
        Star3, //2
    }


    private enum Texts
    {
        Level_Text
    }

    public Sprite _star;
    public Sprite _emptyStar;
    private RectTransform _offsetRoot;

    public override bool Init()
    {
        if (base.Init() == false)
			return false;

        if(_star == null)
        {
            Debug.LogWarning("_stars is NULL");
        }
        if(_emptyStar == null)
        {
            Debug.LogWarning("_emptyStar is NULL");
        }

        BindImages(typeof(StarsImage));
        BindTexts(typeof(Texts));

        for(int i = 0; i < 3; i++)
        {
            GetImage(i).sprite = _emptyStar;
        }

        this.gameObject.BindEvent(OnClick_LevelCell, EUIEvent.Click);
        _offsetRoot = GetComponent<RectTransform>();
        if( _offsetRoot == null )
        {
            Debug.Log("_offsetRoot is NULL");
        }
        return true;
    }

    public void SetInfo(int level, int score)
    {
        GetText((int)Texts.Level_Text).text = $"{level}";
        int stars = Mathf.Clamp(score / 100, 0, 3);
        for(int i = 0; i < stars; i++)
        {
            GetImage(i).sprite = _star;
        }
        
    }

    private void OnClick_LevelCell(PointerEventData eventData)
    {
        this.transform.DOScale(0.9f, 0.15f).SetEase(Ease.OutQuad)
             .OnComplete(() =>
             {
                 transform.DOScale(1f, 0.15f).SetEase(Ease.OutQuad);
             });
        // show popup
    }

    public void SetOffsetX(float x)
    {
        //Debug.Log("SetOffsetX");
        var pos = _offsetRoot.anchoredPosition;
        pos.x += x;
        _offsetRoot.anchoredPosition = pos;
    }
}
