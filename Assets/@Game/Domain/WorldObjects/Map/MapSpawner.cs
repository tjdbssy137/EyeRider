using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class MapSpawner : BaseObject
{
    private Queue<Map> _loadStorage;
    private int _lastSpawnPosX = 1;
    private int _lastSpawnPosZ = 0;
    private const int _maxMapCapacity = 10;
    private const int MAP_SIZE = 100;
    private int _mapCount = 0;
    private GameObject _spawnParent;
    private LoadDirection _lastDirection = LoadDirection.none;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _loadStorage = new Queue<Map>();
        _spawnParent = new GameObject("@Map");
        
        Contexts.Map.OnSpawnMap
            .Subscribe(_ =>
            {
                if(_loadStorage.Count <= _maxMapCapacity)
                {
                    RandomMap();                
                }
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
        Contexts.Map.OnSpawnMap.OnNext(Unit.Default);
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

        // mapData.Direction 은 '이 맵을 다음에 어떻게 놓을지'라는 의미로 사용한다고 가정
        Vector3 spawnPos = GetSpawnPositionForDirection(mapData.Direction, out int nextX, out int nextZ);

        string name = mapData.LoadPrefab.name;
        _lastDirection = mapData.Direction;

        Map m = Managers.Object.Spawn<Map>(name, spawnPos, 0, 0, _spawnParent.transform);
        m.transform.SetParent(_spawnParent.transform, false);
        _loadStorage.Enqueue(m);

        // 상태(인덱스)는 여기서 한 번만 업데이트
        _lastSpawnPosX = nextX;
        _lastSpawnPosZ = _lastSpawnPosZ + 1; // 항상 앞으로 한 칸 이동

        // 큐 용량 초과 시 가장 오래된 맵 제거
        if (_maxMapCapacity < _loadStorage.Count)
        {
            Map oldMap = _loadStorage.Dequeue();
            Managers.Object.Despawn(oldMap);
        }

        if (_loadStorage.Count < _maxMapCapacity)
        {
            Contexts.Map.OnSpawnMap.OnNext(Unit.Default);
        }
    }
    private Vector3 GetSpawnPositionForDirection(LoadDirection direction, out int nextX, out int nextZ)
    {
        // 현재 인덱스 사용
        int currentX = _lastSpawnPosX;
        int currentZ = _lastSpawnPosZ;

        // 방향에 따라 X 인덱스만 조정(Left: -1, Right: +1, none: same)
        if (direction == LoadDirection.Left) currentX -= 1;
        else if (direction == LoadDirection.Right) currentX += 1;
        // direction == none -> currentX 그대로

        // 항상 앞으로(다음 맵)는 Z + 1 이라 가정 (원하면 -1으로 바꿔도 됨)
        // 여기서는 spawn 위치를 '현재 인덱스' 기준으로 계산하고, 
        // next 인덱스를 반환해서 호출자가 상태로 반영하게 함
        nextX = currentX;
        nextZ = currentZ; // spawn 할 때는 아직 Z를 증가시키지 않고 위치 결정만 함

        return new Vector3(nextX * MAP_SIZE, 0, nextZ * MAP_SIZE);
    }

}
