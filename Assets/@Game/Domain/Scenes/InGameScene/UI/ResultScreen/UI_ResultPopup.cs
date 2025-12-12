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
        for(int i = 0; i < Managers.Score.Star; i++)
        {
            GetImage((int)Images.Star1 + i).sprite = _starOn;
        }
    }

    private void OnClick_HomeButton(PointerEventData eventData)
    {
        Contexts.InGame.IsEnd = false;
        Managers.UI.ClosePopupUI(this);
        var loadingComplete = UI_LoadingPopup.Show();

        loadingComplete.Value = true;
        Managers.Scene.LoadScene(EScene.MainMenuScene);
    }

    private void OnClick_NextButton(PointerEventData eventData)
    {
        Contexts.InGame.IsEnd = false;
        Managers.UI.ClosePopupUI(this);
        var loadingComplete = UI_LoadingPopup.Show();
        loadingComplete.Value = true;
        Managers.Scene.LoadScene(EScene.InGameScene);
    }
}
