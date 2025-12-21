using UnityEngine;
using UnityEngine.EventSystems;

public class ClickSoundEffect : BaseObject, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        Managers.Sound.Play(Define.ESound.Button, 1);
    }
}
