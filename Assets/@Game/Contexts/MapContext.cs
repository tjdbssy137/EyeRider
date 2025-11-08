using UnityEngine;
using UniRx;

public partial class MapContext
{
    public Subject<Unit> OnSpawnRoad { get; private set; } = new Subject<Unit>();
    public Subject<Unit> OnDeSpawnRoad { get; private set; } = new Subject<Unit>();
    public Subject<int> OnRotatingCamera { get; private set; } = new Subject<int>();
}