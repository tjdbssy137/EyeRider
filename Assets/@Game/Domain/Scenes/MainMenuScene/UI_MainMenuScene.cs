using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class UI_MainMenuScene : UI_Scene
{
    private enum Images
    {
        GameStart_Button,
    }
    public override bool Init()
    {
        if (base.Init() == false)
			return false;

        BindImages(typeof(Images));
        GetImage((int)Images.GameStart_Button).gameObject.BindEvent(OnClick_GameStartButton, EUIEvent.Click);
        LoadResources();
		return true;
    }

    private void OnClick_GameStartButton(PointerEventData eventData)
    {
        Managers.Scene.LoadScene(EScene.InGameScene);
    }
     public async void OnResourceLoaded()
    {
        Managers.Data.LoadAll();
    }
    void LoadResources()
    {
        Managers.Resource.LoadAllAsync<Object>("PreLoad", async (key, count, totalCount) =>
        {
            Debug.Log($"{key} {count}/{totalCount}");

            if (count == totalCount)
            {
                await Awaitable.MainThreadAsync(); // 메인 스레드 보장
                OnResourceLoaded();
            }
        });
    }

}