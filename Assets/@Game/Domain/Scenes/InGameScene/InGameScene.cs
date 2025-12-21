using System.Collections;
using UniRx;
using UniRx.Triggers;
using Unity.Cinemachine;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;
using static InGameContext;

public class InGameScene : BaseScene
{
    private Car _car;
    private CinemachineCamera _camera;
    public GameObject _spawnPoint;
    private MapSpawner _mapSpawner;
    private ObstacleSpawner _obstacleSpawner;

    private int _plannerGridW = 100;
    private int _plannerGridH = 100;
    private int _desiredBlueprintLength = 200;
    private int _startDir = 0; // 0 = +Z

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
        .SelectMany(_ =>
            this.UpdateAsObservable()
                .Where(__ => Contexts.InGame.IsPaused || Contexts.InGame.IsEnd)
                .Take(1))
                .Subscribe(_ =>
                {
                    //Debug.Log($"Game Start, Contexts.InGame.IsPaused : {Contexts.InGame.IsPaused}");
                    Managers.Sound.Play(Define.ESound.Bgm, 0.7f, 0.7f); // 눈 밖으로 나갈 수록 커지게?

                    Contexts.InGame.Metre = 0f;
                    Contexts.GameProfile.CurrentLevel = SecurePlayerPrefs.GetInt("Level", 1);
                    Managers.Difficulty.CurrentLevel(Contexts.GameProfile.CurrentLevel);
                    Contexts.InGame.IsPaused = false;
                    //Debug.Log($"Game Start, Contexts.InGame.IsPaused : {Contexts.InGame.IsPaused}");

                })
                .AddTo(_disposables);

        Contexts.InGame.OnEndGame
        .Subscribe(type =>
        {
            if(type == GameEndType.Lose)
            {
                Debug.Log("Game Over");
                Managers.Sound.Play(Define.ESound.Fail);

                Managers.Score.GetResult();
                Contexts.InGame.IsEnd = true;
                Contexts.GameProfile.Gold += Managers.Score.FinalGold;
                SecurePlayerPrefs.SetInt("Gold", Contexts.GameProfile.Gold);
                SecurePlayerPrefs.Save();
                UI_TryAgainPopup ui = Managers.UI.ShowPopupUI<UI_TryAgainPopup>();
                ui.SetInfo();
            }
            else if(type == GameEndType.Win)
            {
                Debug.Log("Level Clear");
                Managers.Sound.Play(Define.ESound.Success);
                Managers.Score.GetResult();
                Contexts.InGame.IsEnd = true;
                Contexts.GameProfile.CurrentLevel++;
                SecurePlayerPrefs.SetInt("Level", Contexts.GameProfile.CurrentLevel);
                SecurePlayerPrefs.Save();
                Contexts.GameProfile.Gold += Managers.Score.FinalGold;
                SecurePlayerPrefs.SetInt("Gold", Contexts.GameProfile.Gold);
                SecurePlayerPrefs.Save();
                UI_ResultPopup ui =  Managers.UI.ShowPopupUI<UI_ResultPopup>();
                ui.SetInfo();
            }
        }).AddTo(_disposables);

        Contexts.InGame.OnSpawnMissile.Subscribe(_ =>
        {
            int randX = Random.Range(-30, 30);
            int randZ = Random.Range(-30, 30);

            Vector3 spawnPos = Contexts.InGame.Car.transform.position + new Vector3(randX, 0, randZ);
            Missile missile = Managers.Object.Spawn<Missile>(spawnPos, 0, 0);
        }).AddTo(this);

        Contexts.Car.IsCritical
            .DistinctUntilChanged() // 값이 이전과 달라졌을 때만 통과
            .Where(isCritical => isCritical == true) // true가 된 순간만 필터링
            .Subscribe(_ =>
            {
                Managers.UI.ShowPopupUI<UI_RepairPopup>();
            }).AddTo(this);


        LoadResources();
        return true;
    }
    public override void Clear()
    {
        _disposables.Dispose();
    }
    public async void OnResourceLoaded()
    {
        Managers.Data.LoadAll();
        await SettingSceneObject();
    }

    public async Awaitable SettingSceneObject()
    {
        Contexts.InGame.IsPaused = true;
        Contexts.InGame.MaxLevel = Managers.Data.DifficultyDic.Count;

        Contexts.InGame.PanicPoint = 0;
        Contexts.Car.MaxCondition = 100;
        Contexts.Car.MaxFuel = 100;

        Contexts.InGame.SpawnPosition = _spawnPoint.transform.position;
        Contexts.Car.LastDistancePos = Contexts.InGame.SpawnPosition;
        Debug.Log($"[SettingSceneObject] LastDistancePos : {Contexts.Car.LastDistancePos}");

        _car = Managers.Object.Spawn<Car>(Contexts.InGame.SpawnPosition, 0, 0);

        GameObject mapSpawner = new GameObject("@MapSpawner");
        _mapSpawner = mapSpawner.GetOrAddComponent<MapSpawner>();
        _mapSpawner.OnSpawn();
        _mapSpawner.SetInfo(0);

        // Map Generate
        Contexts.InGame.MAP_SIZE = 100;
        Contexts.InGame.MapPlanner = new MapPlanner(_plannerGridW, _plannerGridH, Contexts.InGame.MAP_SIZE);
        Vector2Int startCell = new Vector2Int(_plannerGridW / 2, _plannerGridH / 2);

        bool ok = false;
        int totalTries = 0;

        while (!ok && totalTries < 300)
        {
            ok = Contexts.InGame.MapPlanner.GeneratePath(startCell, _startDir, _desiredBlueprintLength);
            totalTries++;

            // 10번 시도마다 프레임을 나누어 과부하 방지
            if (totalTries % 10 == 0)
            {
                await Awaitable.NextFrameAsync();
            }
        }

        if (!ok)
        {
            Debug.LogError("Failed to generate map path.");
            return;
        }

        // 성공 알림 및 이후 배치 진행
        Contexts.InGame.OnSuccessGeneratedMapPath.OnNext(true);

        GameObject obstacleSpawner = new GameObject("@ObstacleSpawner");
        _obstacleSpawner = obstacleSpawner.GetOrAddComponent<ObstacleSpawner>();
        _obstacleSpawner.OnSpawn();
        _obstacleSpawner.SetInfo(0);
        

        CameraSideAnchorController carSideClampAnchor = _car.transform.Find("CameraAnchor").GetComponent<CameraSideAnchorController>();
        _camera.Target.TrackingTarget = carSideClampAnchor.gameObject.transform;
        carSideClampAnchor.Init();
        carSideClampAnchor.OnSpawn();

        Managers.Object.Spawn<WaterdropSpawner>(Vector3.zero, 0, 0);

        // Game UI
        UI_InGameScene ui_InGameScene = Managers.UI.ShowSceneUI<UI_InGameScene>();
        ui_InGameScene.SetInfo();

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

    private void ResetGame()
    {
        

    }

}
