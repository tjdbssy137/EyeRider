using Unity.Cinemachine;
using UnityEngine;
using System.Collections;
using UnityEditor.Rendering.LookDev;
using UniRx.Triggers;
using UniRx;

public class InGameScene : BaseScene
{
    private Car _car;
    private Eye _eye;
    private CinemachineCamera _camera;
    public GameObject _spawnPoint;
    private MapSpawner _mapSpawner;


    private int _plannerGridW = 100;
    private int _plannerGridH = 100;
    private int _desiredBlueprintLength = 200;
    private int _startDir = 0; // 0 = +Z

    private float _elapsedRunTime = 0f;

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
        this.InputSystem = new Input_InGameScene();
        this.InputSystem.Init();

        Contexts.InGame.OnStartGame
        .Take(1)
        .SelectMany(_ => this.UpdateAsObservable())
        .Subscribe(_ =>
        {
            UpdateRun();
        })
        .AddTo(_disposables);

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
        UI_InGameScene ui_InGameScene = Managers.UI.ShowSceneUI<UI_InGameScene>();
        ui_InGameScene.SetInfo();
        
        GameObject mapSpawner = new GameObject("@MapSpawner");
        _mapSpawner = mapSpawner.GetOrAddComponent<MapSpawner>();
        _mapSpawner.OnSpawn();
        _mapSpawner.SetInfo(0);
        
        _car = Managers.Object.Spawn<Car>(_spawnPoint.transform.position, 0, 0);
        Debug.Log($"_camera: {_camera}, _car: {_car}, _eye: {_eye}");
        Transform cameraTargetObject = _car.transform.Find("CameraTargetObject");
        _camera.Target.TrackingTarget = cameraTargetObject;

        // Map Generate
        Contexts.InGame.MAP_SIZE = 100;
        Contexts.InGame.MapPlanner = new MapPlanner(_plannerGridW, _plannerGridH, Contexts.InGame.MAP_SIZE);
        Vector2Int startCell = new Vector2Int(_plannerGridW / 2, _plannerGridH / 2);
        bool ok = Contexts.InGame.MapPlanner.GeneratePath(startCell, _startDir, _desiredBlueprintLength);
        Contexts.InGame.OnSuccessGeneratedMapPath.OnNext(ok);

        // GameStart Time Check
        Contexts.InGame.OnStartGame.OnNext(Unit.Default);
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

    private void UpdateRun()
    {
        if (Contexts.InGame.IsGameOver)
        {
            return;
        }

        if (Contexts.InGame.IsPaused)
        {
            return;
        }

        _elapsedRunTime += Time.unscaledDeltaTime;

        if (80f <= _elapsedRunTime)
        {
            Contexts.InGame.IsGameOver = true;
            //ShowGameOverUI();
        }
    }

}
