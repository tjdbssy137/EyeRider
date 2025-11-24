using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

public class Map : BaseObject
{
    public BoxCollider _collider;
    private MapData _data;

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
    
    public override bool OnSpawn()
    {
        if (false == base.OnSpawn())
        {
            return false;
        }

        _collider.OnCollisionExitAsObservable()
            .Where(collision => collision.gameObject.CompareTag("Player"))
            .Subscribe(_ =>
            {
                //Debug.Log($"OnCollisionExitAsObservable map={this.name}, pos={transform.position}");
                
                Managers.Object.Despawn(this);
                Contexts.Map.OnDeSpawnRoad.OnNext(Unit.Default);
                
            })
            .AddTo(_disposables);

        _collider.OnCollisionEnterAsObservable()
            .Where(collision => collision.gameObject.CompareTag("Player"))
            .Subscribe(_ =>
            {
                //Debug.Log($"OnCollisionEnterAsObservable map={this.name}, pos={transform.position}");

                Contexts.InGame.CurrentMapXZ.OnNext(this.transform.position);

                Vector3 forward = DirIndexToVector(DirectionIndex);
                Vector3 right = new Vector3(forward.z, 0f, -forward.x);
                
                Contexts.InGame.WorldForwardDir.OnNext(forward);
                Contexts.InGame.WorldRightDir.OnNext(right);

                if (_data.Direction == RoadDirection.none)
                {
                    Contexts.InGame.OnExitCorner.OnNext(Unit.Default);
                }
                if (_data.Direction == RoadDirection.Right)
                {
                    Contexts.InGame.OnEnterCorner.OnNext(90);
                }
                if (_data.Direction == RoadDirection.Left)
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
}
