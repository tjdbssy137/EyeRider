using UnityEngine;
using UniRx;

public partial class InGameContext
{
    public Subject<Unit> OnStartGame { get; private set; } = new Subject<Unit>();
    public Subject<Unit> OnEndGame { get; private set; } = new Subject<Unit>();
    public Subject<Unit> OnEyeExit { get; private set; } = new Subject<Unit>();

    public Subject<float> OnEnterCorner { get; private set; } = new Subject<float>();   

    public Car Car { get; set; }
}