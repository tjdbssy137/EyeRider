using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class MapSpawner : BaseObject
{
    private Queue<Map> _roadStorage;
    private int _lastSpawnPosX = 0;
    private int _lastSpawnPosZ = 1;
    private float _lastSpawnAngle = 0;
    private const int _maxMapCapacity = 10;
    private const int MAP_SIZE = 100;
    private int _mapCount = 0;
    private GameObject _spawnParent;

    private Vector3 _lastSpawnPos;
    private Quaternion _lastSpawnRot;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _roadStorage = new Queue<Map>();
        _spawnParent = new GameObject("@Map");

        Contexts.Map.OnSpawnRoad
            .Subscribe(_ =>
            {
                if (_roadStorage.Count <= _maxMapCapacity)
                {
                    RandomMap();
                }
            })
            .AddTo(this);

        Contexts.Map.OnDeSpawnRoad
            .Subscribe(_ =>
            {
                _roadStorage.Dequeue();
            })
            .AddTo(this);
        return true;
    }

    public override bool OnSpawn()
    {
        if (base.OnSpawn() == false)
            return false;

        return true;
    }

    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
        _mapCount = Managers.Data.MapDatas.Count;

        _lastSpawnPos = new Vector3(_lastSpawnPosX * MAP_SIZE, 0f, _lastSpawnPosZ * MAP_SIZE);
        _lastSpawnRot = Quaternion.Euler(0f, _lastSpawnAngle, 0f);
        Contexts.Map.OnSpawnRoad.OnNext(Unit.Default);
        Debug.Log("SetInfo MAP");
    }

    private void RandomMap()
    {
        if (_mapCount == 0)
        {
            Debug.LogWarning("Map data is empty. Check Managers.Data.MapDatas.");
            return;
        }

        int index = Random.Range(0, _mapCount);
        var mapData = Managers.Data.MapDatas[index];
        string roadName = mapData.RoadPrefab.name;
        Map m = Managers.Object.Spawn<Map>(roadName, _lastSpawnPos, 0, mapData.DataTemplateId, _spawnParent.transform);

        m.transform.SetParent(_spawnParent.transform, false);
        m.transform.rotation = _lastSpawnRot;

        _roadStorage.Enqueue(m);
        GetNextSpawnPositionAndRotation(mapData.Direction);
        if (_roadStorage.Count < _maxMapCapacity)
        {
            Contexts.Map.OnSpawnRoad.OnNext(Unit.Default);
        }
    }

    private void GetNextSpawnPositionAndRotation(RoadDirection directionForThisMap)
    {
        // 현재 각도(도)와 위치를 사용
        float angle = _lastSpawnAngle;
        Vector3 currentPos = _lastSpawnPos;

        // 현재 forward (월드 기준)
        Vector3 currentForward = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

        if (directionForThisMap == RoadDirection.Right)
        {
            // 오른쪽으로 꺾음: 각도를 +90으로 변경한 후, 그 방향으로 전진
            angle += 90f;
            // normalize angle to [-180,180] or [0,360] if you like
        }
        else if (directionForThisMap == RoadDirection.Left)
        {
            // 왼쪽으로 꺾음: 각도를 -90으로 변경한 후, 그 방향으로 전진
            angle -= 90f;
        }
        else // none == straight
        {
            // 각도 유지 (straight)
        }

        // 새 forward (각도를 바꾼 뒤의 forward) — 중요한 부분: 코너 프리팹이 새 방향을 향하게 하려면
        Vector3 newForward = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

        // 다음 위치: 전진 벡터 방향으로 한 칸(MAP_SIZE)
        Vector3 nextPos = currentPos + newForward * MAP_SIZE;
        Quaternion nextRot = Quaternion.Euler(0f, angle, 0f);

        // 업데이트
        _lastSpawnPos = nextPos;
        _lastSpawnRot = nextRot;
        _lastSpawnAngle = Mathf.Repeat(angle, 360f);

        // none인 경우는 변경하지 않음(직진 유지)
    }


}
