using UnityEngine;

public class MainMenuScene : BaseScene
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
        Contexts.GameProfile.CurrentLevel = SecurePlayerPrefs.GetInt("Level", 1);
        if(Contexts.GameProfile.CurrentLevel <= 1)
        {
            Contexts.GameProfile.NextLevel = Contexts.GameProfile.CurrentLevel;
        }
        else
        {
            Contexts.GameProfile.NextLevel = Contexts.GameProfile.CurrentLevel + 1;
        }
        Managers.UI.ShowSceneUI<UI_MainMenuScene>();
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
