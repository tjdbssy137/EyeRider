using UnityEngine;
using UniRx;

public partial class InGameContext
{
    public Subject<Unit> OnStartGame { get; private set; } = new Subject<Unit>();
    public Subject<Unit> OnEndGame { get; private set; } = new Subject<Unit>();
    public Subject<Unit> OnEyeExit { get; private set; } = new Subject<Unit>();

    public Subject<float> OnEnterCorner { get; private set; } = new Subject<float>();   

    public Car Car { get; set; }

    // Input
    public Subject<bool> OnWkey = new Subject<bool>();
    public Subject<bool> OnAKey = new Subject<bool>();
    public Subject<bool> OnSKey = new Subject<bool>();
    public Subject<bool> OnDKey = new Subject<bool>();

    public bool WKey { get; set; }
    public bool AKey { get; set; }
    public bool SKey { get; set; }
    public bool DKey { get; set; }
}