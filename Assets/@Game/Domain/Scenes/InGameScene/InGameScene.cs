using Unity.Cinemachine;
using UnityEngine;

public class InGameScene : BaseScene
{

    private Car _car;
    private Eye _eye;
    private CinemachineCamera _camera;
    public GameObject _spawnPoint;
    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        _camera = Object.FindFirstObjectByType<CinemachineCamera>();
        LoadResources();
        return true;
    }

    public async void OnResourceLoaded()
    {
        Managers.Data.LoadAll();
        SettingSceneObject();
    }
    public void SettingSceneObject()
    {
        _car = Managers.Object.Spawn<Car>(_spawnPoint.transform.position, 0, 0);
        _eye = Managers.Object.Spawn<Eye>(_spawnPoint.transform.position, 0, 0);
        // 처음에 태풍의 눈이 앞으로 더 나아가고(연출) 게임 시작.
        _camera.Target.TrackingTarget = _car.gameObject.transform;
    }

    void LoadResources()
    {
        // LoadResource
        Managers.Resource.LoadAllAsync<Object>("PreLoad", async (key, count, totalCount) =>
        {
            Debug.Log($"{key} {count}/{totalCount}");

            if (count == totalCount)
            {
                OnResourceLoaded();
                await Awaitable.MainThreadAsync();
            }
        });
    }
}