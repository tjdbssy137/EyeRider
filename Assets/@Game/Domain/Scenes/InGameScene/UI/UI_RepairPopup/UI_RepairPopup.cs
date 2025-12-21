using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_RepairPopup : UI_Popup
{
    private enum Buttons
    {
        BuildButton,
    }
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButtons(typeof(Buttons));
        GetButton((int)Buttons.BuildButton).gameObject.BindEvent(OnClickBuildButton, Define.EUIEvent.Click);
        return true;
    }
    private void OnClickBuildButton(PointerEventData eventData)
    {
        if(Contexts.InGame.Car.Fuel < 30)
        {
            return;
        }
        Contexts.InGame.Car.RepairUsingFuel();
        if (Contexts.InGame.Car.Condition <= 50)
        {
            return;
        }
        Managers.UI.ClosePopupUI(this);
    }
}
