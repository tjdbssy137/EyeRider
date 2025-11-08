using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class MapSpawner : BaseObject
{
    private Queue<Map> _loadStorage;
    private int _lastSpawnPosX = 100;
    private int _lastSpawnPosZ = 0;
    private const int _maxMapCapacity = 10;
    private const int MAP_SIZE = 100;
    private int _mapCount = 0;
    private GameObject _spawnParent;
    private Subject<int> _onSpawnMap = new Subject<int>();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _loadStorage = new Queue<Map>();
        _spawnParent = new GameObject("@Map");
        
        _onSpawnMap
            .Where(x => x < MAP_SIZE)
            .Subscribe(_ =>
            {
                RandomMap();
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
        _onSpawnMap.OnNext(_loadStorage.Count);
        Debug.Log("SetInfo MAP");
    }

    private void RandomMap()
    {
        if (_mapCount == 0)
        {
            Debug.LogWarning("Map data is empty. Check Managers.Data.MapDatas.");
            return;
        }

        Vector3 spawnPos = new Vector3(MAP_SIZE * _lastSpawnPosX, 0, MAP_SIZE * _lastSpawnPosZ);
        int index = Random.Range(0, _mapCount);
        string name = Managers.Data.MapDatas[index].LoadPrefab.name;

        Map m = Managers.Object.Spawn<Map>(name, spawnPos, 0, 0, _spawnParent.transform);
        m.transform.SetParent(_spawnParent.transform, false);
        _loadStorage.Enqueue(m);

        // 큐 용량 초과 시 가장 오래된 맵 제거
        if (_maxMapCapacity < _loadStorage.Count)
        {
            Map oldMap = _loadStorage.Dequeue();
            Managers.Object.Despawn(oldMap);
        }

        // 다음 위치로 이동
        _lastSpawnPosZ++;
        if (3 <= _lastSpawnPosZ)
        {
            _lastSpawnPosX++;
        }

        if(_loadStorage.Count < _maxMapCapacity)
        {
            _onSpawnMap.OnNext(_loadStorage.Count);
        }
    }
}
