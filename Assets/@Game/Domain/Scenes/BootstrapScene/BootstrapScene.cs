using UnityEngine;
using static Define;

public class BootstrapScene : BaseScene
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        LoadResources();
        return true;
    }

    public async void OnResourceLoaded()
    {
        Managers.Data.LoadAll();
        Managers.UI.ShowSceneUI<UI_BootstrapScene>();
        Managers.Scene.LoadScene(EScene.InGameScene);
    }

    void LoadResources()
    {
        if (Managers.Resource.IsPreloadDone)
        {
            OnResourceLoaded();
            return;
        }
        
        Managers.Resource.LoadAllAsync<Object>("PreLoad", async (key, count, totalCount) =>
        {
            Debug.Log($"{key} {count}/{totalCount}");

            if (count == totalCount)
            {
                await Awaitable.MainThreadAsync(); // 메인 스레드 보장
                Managers.Resource.MarkPreloadDone();
                OnResourceLoaded();
            }
        });
    }
}

