using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 1. Collider가 있어야합니다.
/// 2. Ignore Raycast 레이어에 속하는 객체에서는 호출되지 않습니다.
//  3. 다음 속성이 true로 설정된 경우 트리거로 표시된 Collider 및 2D Collider에서 호출됩니다.-
//  -For 3D physics: Physics.queriesHitTriggers
// - For 2D physics: Physics2D.queriesHitTriggers
/// </summary>
public class GO_EventHandler : MonoBehaviour
{
	public event Action OnMouseDownHandler = null;
	public event Action OnMouseDragHandler = null;
    public event Action OnMouseEnterHandler = null;
	public event Action OnMouseExitHandler = null;
    public event Action OnMouseOverHandler = null;
    public event Action OnMouseUpHandler = null;
	public event Action OnMouseUpAsButtonHandler = null;

    public event Action<Collider> OnTriggerEnterHandler = null;
    public event Action<Collider> OnTriggerStayHandler = null;
    public event Action<Collider> OnTriggerExitHandler = null;


    //마우스가 콜라이더 위에 있는 동안 매 프레임마다 호출됩니다.
    public void OnMouseOver()
    {
        OnMouseOverHandler?.Invoke();
    }

    //마우스가 Collider에 들어가면 호출됩니다.
    public void OnMouseEnter()
    {
        OnMouseEnterHandler?.Invoke();
    }

    //마우스가 더 이상 Collider 위에 없을 때 호출됩니다.
    public void OnMouseExit()
    {
        OnMouseExitHandler?.Invoke();
    }

    //	OnMouseDown은 사용자가 Collider 위에 있는 동안 마우스 왼쪽 버튼을 누르면 호출됩니다.
    public void OnMouseDown()
    {
        OnMouseDownHandler?.Invoke();
    }

    //	OnMouseDrag는 사용자가 Collider를 클릭하고 마우스를 계속 누르고 있는 경우 호출됩니다.
    public void OnMouseDrag()
    {
        OnMouseDragHandler?.Invoke();
    }

    //	OnMouseUp은 사용자가 마우스 버튼을 놓으면 호출됩니다.
    public void OnMouseUp()
    {
        OnMouseUpHandler?.Invoke();
    }

    //	OnMouseUpAsButton은 마우스를 눌렀던 것과 동일한 Collider 위에서 놓을 때만 호출됩니다
    public void OnMouseUpAsButton()
    {
        OnMouseUpAsButtonHandler?.Invoke();
    }


    public void OnTriggerEnter(Collider other)
    {
        OnTriggerEnterHandler?.Invoke(other);
    }


    public void OnTriggerStay(Collider other)
    {
        OnTriggerStayHandler?.Invoke(other);
    }


    public void OnTriggerExit(Collider other)
    {
        OnTriggerExitHandler?.Invoke(other);
    }
}



