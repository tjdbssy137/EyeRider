using UnityEngine;
using UniRx;

public partial class CarContext
{
    public Subject<float> OnCarMoving { get; private set; } = new Subject<float>();
    public Subject<Vector3> OnEyeMoving { get; private set; } = new Subject<Vector3>();

    public float Fuel = Contexts.InGame.Car.Fuel;
    public float Condition = Contexts.InGame.Car.Condition;
}