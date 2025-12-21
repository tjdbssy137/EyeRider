using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class UI_TryAgainPopup : UI_Popup
{
    private enum Buttons
    {
        Home,
        Retry
    }
    private enum Texts
    {
        //Stage,
        Score,
        Compensation
    }
    private List<Vector3> _originScales = new List<Vector3>();

    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        GetButton((int)Buttons.Home).gameObject.BindEvent(OnClick_HomeButton, EUIEvent.Click);
        GetButton((int)Buttons.Retry).gameObject.BindEvent(OnClick_RetryButton, EUIEvent.Click);

        GetButton((int)Buttons.Home).gameObject.BindEvent(OnEnter_HomeButton, EUIEvent.PointerEnter);
        GetButton((int)Buttons.Retry).gameObject.BindEvent(OnEnter_RetryButton, EUIEvent.PointerEnter);

        GetButton((int)Buttons.Home).gameObject.BindEvent(OnExit_HomeButton, EUIEvent.PointerExit);
        GetButton((int)Buttons.Retry).gameObject.BindEvent(OnExit_RetryButton, EUIEvent.PointerExit);

        _originScales.Add(GetButton((int)Buttons.Home).gameObject.transform.localScale);
        _originScales.Add(GetButton((int)Buttons.Retry).gameObject.transform.localScale);
        return true;
    }

    public void SetInfo()
    {
        Contexts.InGame.IsEnd = true;
        //GetText((int)Texts.Stage).text = $"Stage {Contexts.GameProfile.CurrentLevel}";
        GetText((int)Texts.Score).text = $"Score {Managers.Score.FinalScore}";
        GetText((int)Texts.Compensation).text = $"{Managers.Score.FinalGold}";
    }

    private void OnClick_HomeButton(PointerEventData eventData)
    {
        SceneMove(EScene.MainMenuScene);
    }

    private void OnClick_RetryButton(PointerEventData eventData)
    {
        SceneMove(EScene.InGameScene);
    }

    private void OnEnter_HomeButton(PointerEventData eventData)
    {
        OnEnterButton((int)Buttons.Home);
    }
    private void OnEnter_RetryButton(PointerEventData eventData)
    {
        OnEnterButton((int)Buttons.Retry);
    }
    private void OnExit_HomeButton(PointerEventData eventData)
    {
        OnExitButton((int)Buttons.Home);
    }
    private void OnExit_RetryButton(PointerEventData eventData)
    {
        OnExitButton((int)Buttons.Retry);
    }

    private void SceneMove(EScene scene)
    {
        Contexts.InGame.IsEnd = false;
        Managers.UI.ClosePopupUI(this);
        var loadingComplete = UI_LoadingPopup.Show();
        loadingComplete.Value = true;
        Managers.Scene.LoadScene(scene);
    }

    private void OnEnterButton(int index)
    {
        GameObject go = GetButton(index).gameObject;
        go.transform.DOKill();

        Vector3 targetScale = _originScales[index] * 0.9f;
        go.transform.DOScale(targetScale, 0.2f).SetUpdate(true).SetEase(Ease.OutQuad);
    }
    private void OnExitButton(int index) 
    {
        GameObject go = GetButton(index).gameObject;
        go.transform.DOKill();
        go.transform.DOScale(_originScales[index], 0.2f).SetUpdate(true).SetEase(Ease.OutQuad);
    }
}
