using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRectDrag : BaseObject, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ScrollRect _scrollRect;
    private Vector2 _startPos;

    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        _scrollRect = GetComponent<ScrollRect>();

        return true;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        _scrollRect.OnBeginDrag(eventData);
        _startPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _scrollRect.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _scrollRect.OnEndDrag(eventData);
    }
}
