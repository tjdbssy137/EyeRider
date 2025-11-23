using UnityEngine;
using UniRx;
using System;

public partial class InGameContext
{
    public Subject<Unit> OnStartGame { get; private set; } = new Subject<Unit>();
    public Subject<Unit> OnEndGame { get; private set; } = new Subject<Unit>();

    // EYE
    public Subject<float> OnExitEye { get; private set; } = new Subject<float>();
    public Subject<Unit> OnEnterEye { get; private set; } = new Subject<Unit>();


    public Subject<float> OnEnterCorner { get; private set; } = new Subject<float>();   
    public Subject<Unit> OnExitCorner { get; private set; } = new Subject<Unit>();   

    public Car Car { get; set; }


    // Map Generate
    public Subject<bool> OnSuccessGeneratedMapPath = new Subject<bool>();
    public MapPlanner MapPlanner { get; set; }
    public int MAP_SIZE { get; set; }

    // Input
    public bool WKey { get; set; }
    public bool AKey { get; set; }
    public bool SKey { get; set; }
    public bool DKey { get; set; }

    // play Timer
    // private float _runStartTime = 0f;
    // public float RunStartTime
    // {
    //     get { return _runStartTime; }
    //     private set { _runStartTime = value; }
    // }
    // public void StartGame()
    // {
    //     _runStartTime = Time.unscaledTime;
    // }
    public bool IsGameOver { get; set; }
    public bool IsPaused { get; set; }
    public int MaxRunTime { get; set; }


    public Subject<Vector3> CurrentMapXZ {get; set;} = new Subject<Vector3>();
    public BehaviorSubject<Vector3> WorldForwardDir = new BehaviorSubject<Vector3>(Vector3.forward);
    public BehaviorSubject<Vector3> WorldRightDir = new BehaviorSubject<Vector3>(Vector3.right);



}