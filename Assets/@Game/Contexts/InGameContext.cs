using UnityEngine;
using UniRx;

public partial class InGameContext
{
    public Subject<Unit> OnStartGame { get; private set; } = new Subject<Unit>();
    public Subject<Unit> OnEndGame { get; private set; } = new Subject<Unit>();
    public Subject<Unit> OnEyeExit { get; private set; } = new Subject<Unit>();

    public Subject<float> OnEnterCorner { get; private set; } = new Subject<float>();   

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


}