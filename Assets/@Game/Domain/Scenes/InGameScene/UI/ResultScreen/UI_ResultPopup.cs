using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_ResultPopup : UI_Popup
{
    private enum Buttons
    {
        Home,
        Next
    }
    private enum Images
    {
        Star1, //0
        Star2,//1
        Star3, //2
    }
    private enum Texts
    {
        //Stage,
        Score,
        Compensation
    }

    public Sprite _starOn;
    public Sprite _starOff;
    private List<Vector3> _originScales = new List<Vector3>();

    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }
        BindButtons(typeof(Buttons));
        BindImages(typeof(Images));
        BindTexts(typeof(Texts));

        GetButton((int)Buttons.Home).gameObject.BindEvent(OnClick_HomeButton, EUIEvent.Click);
        GetButton((int)Buttons.Next).gameObject.BindEvent(OnClick_NextButton, EUIEvent.Click);

        GetButton((int)Buttons.Home).gameObject.BindEvent(OnEnter_HomeButton, EUIEvent.PointerEnter);
        GetButton((int)Buttons.Next).gameObject.BindEvent(OnEnter_NextButton, EUIEvent.PointerEnter);

        GetButton((int)Buttons.Home).gameObject.BindEvent(OnExit_HomeButton, EUIEvent.PointerExit);
        GetButton((int)Buttons.Next).gameObject.BindEvent(OnExit_NextButton, EUIEvent.PointerExit);

        _originScales.Add(GetButton((int)Buttons.Home).gameObject.transform.localScale);
        _originScales.Add(GetButton((int)Buttons.Next).gameObject.transform.localScale);

        for (int i = 0; i < 3; i++)
        {
            GetImage((int)Images.Star1 + i).sprite = _starOff;
        }

        return true;
    }

    public void SetInfo()
    {
        Contexts.InGame.IsEnd = true;
        //GetText((int)Texts.Stage).text = $"Stage {Contexts.GameProfile.CurrentLevel}";
        GetText((int)Texts.Score).text = $"Score {Managers.Score.FinalScore}";
        GetText((int)Texts.Compensation).text = $"{Managers.Score.FinalGold}";
        for (int i = 0; i < Managers.Score.Star; i++)
        {
            GetImage((int)Images.Star1 + i).sprite = _starOn;
        }
    }

    private void OnClick_HomeButton(PointerEventData eventData)
    {
        SceneMove(EScene.MainMenuScene);
    }

    private void OnClick_NextButton(PointerEventData eventData)
    {
        SceneMove(EScene.InGameScene);
    }
    private void OnEnter_HomeButton(PointerEventData eventData)
    {
        OnEnterButton((int)Buttons.Home);
    }
    private void OnEnter_NextButton(PointerEventData eventData)
    {
        OnEnterButton((int)Buttons.Next);
    }
    private void OnExit_HomeButton(PointerEventData eventData)
    {
        OnExitButton((int)Buttons.Home);
    }
    private void OnExit_NextButton(PointerEventData eventData)
    {
        OnExitButton((int)Buttons.Next);
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
