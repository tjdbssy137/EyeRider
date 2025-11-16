using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class InGameScene : BaseScene
{
    private Car _car;
    private Eye _eye;
    private CameraFollowPoint _cameraFollowPoint;
    private CinemachineCamera _camera;
    public GameObject _spawnPoint;
    private MapSpawner _mapSpawner;
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        // 카메라 먼저 찾기
        _camera = Object.FindFirstObjectByType<CinemachineCamera>();
        if (_camera == null)
        {
            Debug.LogError("Camera is NULL");
        }
        
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
        GameObject mapSpawner = new GameObject("@MapSpawner");
        _mapSpawner = mapSpawner.GetOrAddComponent<MapSpawner>();
        _mapSpawner.OnSpawn();
        _mapSpawner.SetInfo(0);
        
        _eye = Managers.Object.Spawn<Eye>(_spawnPoint.transform.position, 0, 0);
        _car = Managers.Object.Spawn<Car>(_spawnPoint.transform.position, 0, 0);
        Debug.Log($"_camera: {_camera}, _car: {_car}, _eye: {_eye}");
        // _cameraFollowPoint = Managers.Object.Spawn<CameraFollowPoint>(_spawnPoint.transform.position, 0, 0);
        // _cameraFollowPoint.SettingCar(_car);
        _camera.Target.TrackingTarget = _car.transform;
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
