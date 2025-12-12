using UnityEngine;
using UniRx;
using System;

public partial class InGameContext
{
    public Subject<Unit> OnStartGame { get; private set; } = new Subject<Unit>();
    public enum GameEndType
    {
        None,
        Win,
        Lose
    }
    public Subject<GameEndType> OnEndGame { get; private set; } = new Subject<GameEndType>();

    // Game State
    public bool IsEnd { get; set; }
    public bool IsPaused { get; set; }

    // EYE
    public Subject<float> OnExitEye { get; private set; } = new Subject<float>();
    public Subject<Unit> OnEnterEye { get; private set; } = new Subject<Unit>();

    // CORNER
    public Subject<float> OnEnterCorner { get; private set; } = new Subject<float>();   
    public Subject<Unit> OnExitCorner { get; private set; } = new Subject<Unit>();   

    public Car Car { get; set; }

    public int MaxLevel { get; set; }
    public Vector3 SpawnPosition { get; set; }

    // Map Generate
    public Subject<bool> OnSuccessGeneratedMapPath = new Subject<bool>();
    public MapPlanner MapPlanner { get; set; }
    public int MAP_SIZE { get; set; }
    public Subject<Transform> OnSpawnMap {get; set;} = new Subject<Transform>();

    // Input
    public bool WKey { get; set; }
    public bool AKey { get; set; }
    public bool SKey { get; set; }
    public bool DKey { get; set; }



    public Subject<Vector3> CurrentMapXZ {get; set;} = new Subject<Vector3>();
    public BehaviorSubject<Vector3> WorldForwardDir = new BehaviorSubject<Vector3>(Vector3.forward);
    public BehaviorSubject<Vector3> WorldRightDir = new BehaviorSubject<Vector3>(Vector3.right);

    public  Subject<float> OnCollisionObstacle { get; private set; } = new Subject<float>();
    public int IsCollisionObstacle { get; set; }


    // Game Score
    public float PanicPoint { get; set; }
    public float Metre { get; set; }

}