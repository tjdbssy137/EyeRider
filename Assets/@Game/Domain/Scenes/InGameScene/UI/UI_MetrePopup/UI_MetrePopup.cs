using UniRx;
using UnityEngine;

public class UI_MetrePopup : UI_Popup
{
    public enum Texts
    {
        Metre,
    }
    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }
        BindTexts(typeof(Texts));

        Observable.Timer(System.TimeSpan.FromSeconds(1.5f))
            .Subscribe(_ => Managers.UI.ClosePopupUI(this))
            .AddTo(this);
        return true;
    }
    public void SetInfo()
    {
        GetText((int)Texts.Metre).text = $"{Contexts.InGame.Metre} M";
    }

}
