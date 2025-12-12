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
        Contexts.InGame.IsEnd = false;
        Managers.UI.ClosePopupUI(this);
        var loadingComplete = UI_LoadingPopup.Show();

        loadingComplete.Value = true;
        Managers.Scene.LoadScene(EScene.MainMenuScene);
    }

    private void OnClick_RetryButton(PointerEventData eventData)
    {
        Contexts.InGame.IsEnd = false;
        Managers.UI.ClosePopupUI(this);
        var loadingComplete = UI_LoadingPopup.Show();
        loadingComplete.Value = true;
        Managers.Scene.LoadScene(EScene.InGameScene);
    }
}
