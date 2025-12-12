using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

public class Map : BaseObject
{
    public BoxCollider _collider;
    private MapData _data;
    private Car _car;

    // ★ 추가 — 이 맵이 가진 방향값 (0=+Z,1=+X,2=-Z,3=-X)
    public int DirectionIndex { get; private set; }
    public void SetDirection(int dir)
    {
        DirectionIndex = dir & 3;
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        if (_collider == null)
        {
            Debug.Log("_collider is NULL");
        }
        return true;
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override bool OnSpawn()
    {
        if (false == base.OnSpawn())
        {
            return false;
        }

        //_collider.OnCollisionExitAsObservable()
        //    .Where(collision => collision.gameObject.CompareTag("Player"))
        //    .Subscribe(_ =>
        //    {                
        //        Managers.Resource.Destroy(this.gameObject);
        //        Contexts.Map.OnDeSpawnRoad.OnNext(Unit.Default);
        //    })
        //    .AddTo(_disposables);

        this.UpdateAsObservable()
            .Where(_ => this.gameObject.activeSelf)
           .Subscribe(_ => CheckDistance())
           .AddTo(_disposables);

        _collider.OnCollisionEnterAsObservable()
            .Where(collision => collision.gameObject.CompareTag("Player"))
            .Subscribe(_ =>
            {
                //Debug.Log($"OnCollisionEnterAsObservable map={this.name}, pos={transform.position}");
                _car = Contexts.InGame.Car;

                Contexts.InGame.CurrentMapXZ.OnNext(this.transform.position);

                Vector3 forward = DirIndexToVector(DirectionIndex);
                Vector3 right = new Vector3(forward.z, 0f, -forward.x);

                Contexts.InGame.WorldForwardDir.OnNext(forward);
                Contexts.InGame.WorldRightDir.OnNext(right);

                if (_data.Direction == RoadDirection.None)
                {
                    Contexts.InGame.OnExitCorner.OnNext(Unit.Default);
                }
                else if (_data.Direction == RoadDirection.Right)
                {
                    Contexts.InGame.OnEnterCorner.OnNext(90);
                }
                else if (_data.Direction == RoadDirection.Left)
                {
                    Contexts.InGame.OnEnterCorner.OnNext(-90);
                }

            })
            .AddTo(_disposables);

        return true;
    }

    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
        _data = Managers.Data.MapDatas.GetValueOrDefault(dataTemplate);
        Contexts.InGame.OnSpawnMap.OnNext(this.transform);

    }
 
    private Vector3 DirIndexToVector(int dir)
    {
        switch (dir & 3)
        {
            case 0: return Vector3.forward; // +Z
            case 1: return Vector3.right; // +X
            case 2: return Vector3.back; // -Z
            default: return Vector3.left; // -X
        }
    }

    private void CheckDistance()
    {
        if (_car == null)
        {
            return;
        }

        Vector3 toRoad = transform.position - _car.transform.position;

        bool isBehind = Vector3.Dot(_car.transform.forward, toRoad) < 0;

        float dist = toRoad.magnitude;

        if (isBehind && 100f <= dist)
        {
            Managers.Resource.Destroy(this.gameObject);
            Contexts.Map.OnDeSpawnRoad.OnNext(Unit.Default);
        }
    }
}
