using UnityEngine;
using UniRx;

public partial class MapContext
{
    public Subject<Map> OnSpawnMap { get; private set; } = new Subject<Map>();
    public Subject<int> OnRotatingCamera { get; private set; } = new Subject<int>();
}