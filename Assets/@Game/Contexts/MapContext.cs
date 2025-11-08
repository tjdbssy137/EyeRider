using UnityEngine;
using UniRx;

public partial class MapContext
{
    public Subject<Unit> OnSpawnMap { get; private set; } = new Subject<Unit>();
    public Subject<Unit> OnDeSpawnMap { get; private set; } = new Subject<Unit>();
    public Subject<int> OnRotatingCamera { get; private set; } = new Subject<int>();
}