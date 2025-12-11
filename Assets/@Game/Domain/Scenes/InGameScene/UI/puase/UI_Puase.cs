using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class UI_Puase : UI_Popup
{
    private enum Images
    {
        Index
    }
    private enum Toggles
    {
        Resume,
        ReStart,
        Home
    }
    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }
        BindImages(typeof(Images));
        BindToggles(typeof(Toggles));
        GetToggle((int)Toggles.Resume).gameObject.BindEvent(OnClick_ResumeButton, EUIEvent.Click);
        GetToggle((int)Toggles.ReStart).gameObject.BindEvent(OnClick_ReStartButton, EUIEvent.Click);
        GetToggle((int)Toggles.Home).gameObject.BindEvent(OnClick_HomeButton, EUIEvent.Click);

        GetToggle((int)Toggles.Resume).gameObject.BindEvent(OnHover_ResumeButton, EUIEvent.PointerEnter);
        GetToggle((int)Toggles.ReStart).gameObject.BindEvent(OnHover_ReStartButton, EUIEvent.PointerEnter);
        GetToggle((int)Toggles.Home).gameObject.BindEvent(OnHover_HomeButton, EUIEvent.PointerEnter);

        Contexts.InGame.IsPaused = true;
        Debug.Log("Game Paused");
        return true;
    }
    private void OnHover_ResumeButton(PointerEventData eventData)
    {
        SoftMoveIndex((int)Toggles.Resume);
    }
    private void OnClick_ResumeButton(PointerEventData eventData)
    {
        Debug.Log("Resume Clicked");
        Contexts.InGame.IsPaused = false;
        Managers.UI.ClosePopupUI(this);
    }
    private void OnHover_ReStartButton(PointerEventData eventData)
    {
        SoftMoveIndex((int)Toggles.ReStart);
    }
    private void OnClick_ReStartButton(PointerEventData eventData)
    {
        Debug.Log("ReStart Clicked");
        Contexts.InGame.IsPaused = false;
        Managers.UI.ClosePopupUI(this);
        Managers.Scene.LoadScene(EScene.InGameScene);
    }
    private void OnHover_HomeButton(PointerEventData eventData)
    {
        SoftMoveIndex((int)Toggles.Home);
    }
    private void OnClick_HomeButton(PointerEventData eventData)
    {
        Debug.Log("Home Clicked");
        Contexts.InGame.IsPaused = false;
        Managers.UI.ClosePopupUI(this);
        Managers.Scene.LoadScene(EScene.MainMenuScene);
    }

    private void SoftMoveIndex(int image)
    {
        for(int i = 0; i < 3; i++)
        {
            GetToggle(i).isOn = (image == i);
        }
        var indexObj = GetImage((int)Images.Index).rectTransform;
        float targetY = GetToggle(image).gameObject.transform.position.y;

        indexObj.DOKill();

        indexObj.DOMoveY(targetY, 0.2f).SetEase(Ease.OutQuint);
    }
}
