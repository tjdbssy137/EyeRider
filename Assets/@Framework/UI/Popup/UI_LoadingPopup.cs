using System;
using System.Collections;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using static Define;

public class UI_LoadingPopup : UI_Popup
{
    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        return true;
    }

    public static ReactiveProperty<bool> Show()
    {
        UI_LoadingPopup indicator = Managers.UI.ShowPopupUI<UI_LoadingPopup>();

        ReactiveProperty<bool> condition = new ReactiveProperty<bool>(false);
        condition.DistinctUntilChanged(c => c == true)
            .Where(c => c)
            .Subscribe(_ =>
            {
                Managers.UI.ClosePopupUI(indicator);
            });

        return condition;
    }


    public static async Awaitable<ReactiveProperty<bool>> Show(Func<Task> action, EThread threadType = EThread.Main)
    {
        await Awaitable.MainThreadAsync();
        UI_LoadingPopup indicator = Managers.UI.ShowPopupUI<UI_LoadingPopup>();
        

        ReactiveProperty<bool> condition = new ReactiveProperty<bool>(false);
        condition.DistinctUntilChanged(c => c == true)
            .Where(c => c)
            .Subscribe(_ =>
            {
                Managers.UI.ClosePopupUI(indicator);
            });

        if(threadType == EThread.Background)
            await Awaitable.BackgroundThreadAsync();
        else
            await Awaitable.MainThreadAsync();

        await action.Invoke();

        await Awaitable.MainThreadAsync();
        condition.Value = true;

        return condition;
    }
}
