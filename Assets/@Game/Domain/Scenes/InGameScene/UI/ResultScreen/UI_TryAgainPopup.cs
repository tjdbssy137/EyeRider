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
        Time.timeScale = 0;

        return true;
    }

    public void SetInfo()
    {
        //GetText((int)Texts.Stage).text = $"Stage {Contexts.GameProfile.CurrentLevel}";
        GetText((int)Texts.Score).text = $"Score {Contexts.InGame.GameScore}";
        GetText((int)Texts.Compensation).text = $"{100}";
    }

    private void OnClick_HomeButton(PointerEventData eventData)
    {
        Time.timeScale = 1;
        Managers.UI.ClosePopupUI(this);
        var loadingComplete = UI_LoadingPopup.Show();

        loadingComplete.Value = true;
        Managers.Scene.LoadScene(EScene.MainMenuScene);
    }

    private void OnClick_RetryButton(PointerEventData eventData)
    {
        Time.timeScale = 1;
        Managers.UI.ClosePopupUI(this);
        var loadingComplete = UI_LoadingPopup.Show();
        loadingComplete.Value = true;
        Managers.Scene.LoadScene(EScene.InGameScene);
    }
}
