using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

public class MapSpawner : BaseObject
{
    private Queue<Map> _roadStorage;
    private GameObject _spawnParent;
    private List<MapPlanner.PathNode> _blueprint = new List<MapPlanner.PathNode>();
    private int _blueprintIndex = 0;
    [SerializeField] private int _maxMapCapacity = 10;

    private Dictionary<Tile, List<MapData>> _mapDataByTile = new Dictionary<Tile, List<MapData>>();
    private int _lastGasStationIndex = -20;

    public override bool Init()
    {
        if (base.Init() == false)
        {
            return false;
        }

        _roadStorage = new Queue<Map>();
        _spawnParent = new GameObject("@Map");

        if (Managers.Data.MapDatas != null)
        {
            foreach (var md in Managers.Data.MapDatas.Values)
            {
                Tile key = Tile.Straight;
                if (md.Direction == RoadDirection.Left)
                {
                    key = Tile.Left;
                }
                else if (md.Direction == RoadDirection.Right)
                {
                    key = Tile.Right;
                }

                if (!_mapDataByTile.ContainsKey(key))
                {
                    _mapDataByTile[key] = new List<MapData>();
                }
                _mapDataByTile[key].Add(md);
            }
        }

        Contexts.Map.OnDeSpawnRoad.Subscribe(_ => {
            if (0 < _roadStorage.Count)
            {
                _roadStorage.Dequeue();
            }
            SpawnUntilCapacity();
        }).AddTo(_disposables);

        Contexts.InGame.OnSuccessGeneratedMapPath.Subscribe(result => {
            if (result)
            {
                _blueprint = new List<MapPlanner.PathNode>(Contexts.InGame.MapPlanner.PathOrder);
                _blueprintIndex = 0;
                SpawnUntilCapacity();
                Contexts.Map.OnSpawnRoad.OnNext(Unit.Default);
            }
        }).AddTo(_disposables);

        return true;
    }

    private void SpawnUntilCapacity()
    {
        while (_roadStorage.Count < _maxMapCapacity && _blueprintIndex < _blueprint.Count)
        {
            SpawnFromBlueprintAt(_blueprintIndex);
            _blueprintIndex++;
        }
    }

    private void SpawnFromBlueprintAt(int index)
    {
        var node = _blueprint[index];
        if (!_mapDataByTile.TryGetValue(node.tile, out List<MapData> tempList))
        {
            return;
        }

        MapData md = GetSelectedMapData(tempList, index);

        int outgoingDir = node.dir & 3;
        int prefabBaseFacing = Mathf.Clamp(md.BaseFacing, 0, 3);
        int deltaTurns = (outgoingDir - prefabBaseFacing + 4) & 3;
        float angle = deltaTurns * 90f;

        if (md.Direction == RoadDirection.Right)
        {
            angle += 90f;
        }
        if (md.Direction == RoadDirection.Left)
        {
            angle -= 90f;
        }
        angle = Mathf.Repeat(angle, 360f);

        Vector3 spawnWorld = Contexts.InGame.MapPlanner.CellToWorld(node.cell);

        Map m = Managers.Object.Spawn<Map>(md.RoadPrefab.name, spawnWorld, 0, md.DataTemplateId, _spawnParent.transform);
        m.transform.rotation = Quaternion.Euler(0f, angle, 0f);
        m.SetDirection(outgoingDir);

        _roadStorage.Enqueue(m);
    }

    private MapData GetSelectedMapData(List<MapData> tempList, int index)
    {
        float fuelRatio = Contexts.InGame.Car.Fuel / Contexts.Car.MaxFuel;

        // 주유소 조건: 연료 부족 + 직선 타일 + 간격 유지
        if (fuelRatio < 0.5f && _blueprint[index].tile == Tile.Straight && 12 < (index - _lastGasStationIndex))
        {
            var gasStation = tempList.FirstOrDefault(m => m.RoadPrefab.name.Contains("Gas"));
            if (gasStation != null)
            {
                _lastGasStationIndex = index;
                return gasStation;
            }
        }

        // 일반 도로 랜덤 선택 (주유소 제외)
        var normalRoads = tempList.Where(m => !m.RoadPrefab.name.Contains("Gas")).ToList();
        return 0 < normalRoads.Count ? normalRoads[Random.Range(0, normalRoads.Count)] : tempList[Random.Range(0, tempList.Count)];
    }
}