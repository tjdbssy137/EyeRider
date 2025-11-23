using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

public class MapSpawner : BaseObject
{
    private Queue<Map> _roadStorage;
    private GameObject _spawnParent;

    private int _mapCount = 0;

    private List<MapPlanner.PathNode> _blueprint = new List<MapPlanner.PathNode>();
    private int _blueprintIndex = 0;

    [SerializeField] private int _maxMapCapacity = 10;

    private Vector3 _lastSpawnPos;
    private Quaternion _lastSpawnRot;
    private float _lastSpawnAngle = 0f;

    private Dictionary<Tile, MapData> _mapDataByTile = new Dictionary<Tile, MapData>();

    public override bool Init()
    {
        if (base.Init() == false) return false;

        _roadStorage = new Queue<Map>();
        _spawnParent = new GameObject("@Map");

        if (Managers.Data.MapDatas != null)
        {
            foreach (var md in Managers.Data.MapDatas.Values)
            {
                if (md.Direction == RoadDirection.none)
                    _mapDataByTile[Tile.Straight] = md;
                else if (md.Direction == RoadDirection.Left)
                    _mapDataByTile[Tile.Left] = md;
                else if (md.Direction == RoadDirection.Right)
                    _mapDataByTile[Tile.Right] = md;
            }
        }

        Contexts.Map.OnDeSpawnRoad
            .Subscribe(_ =>
            {
                if (0 < _roadStorage.Count)
                {
                    _roadStorage.Dequeue();
                }
                SpawnUntilCapacity();
            })
            .AddTo(_disposables);

        Contexts.InGame.OnSuccessGeneratedMapPath
            .Subscribe(result =>
            {
                if (!result)
                {
                    _blueprint.Clear();
                    _blueprintIndex = 0;
                }
                else
                {
                    _blueprint = new List<MapPlanner.PathNode>(Contexts.InGame.MapPlanner.PathOrder);
                    _blueprintIndex = 0;
                    SpawnUntilCapacity();
                }

                Contexts.Map.OnSpawnRoad.OnNext(Unit.Default);
            })
            .AddTo(_disposables);

        return true;
    }

    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
        _mapCount = Managers.Data.MapDatas.Count;

        _lastSpawnPos = new Vector3(0f, 0f, 0);
        _lastSpawnRot = Quaternion.Euler(0f, 0f, 0f);
        _lastSpawnAngle = 0f;
        Debug.Log("SetInfo MAP");
    }

    private void SpawnUntilCapacity()
    {
        while (_roadStorage.Count < _maxMapCapacity && _blueprintIndex < _blueprint.Count)
        {
            SpawnFromBlueprintAt(_blueprintIndex);
            _blueprintIndex++;
        }

        while (_roadStorage.Count < _maxMapCapacity)
        {
            RandomMap();
        }
    }

    private void SpawnFromBlueprintAt(int index)
    {
        if (index < 0 || _blueprint.Count <= index)
        {
            return;
        }

        var node = _blueprint[index];

        if (!_mapDataByTile.TryGetValue(node.tile, out MapData md))
        {
            Debug.LogError($"[MapSpawner] No MapData for Tile {node.tile}. Skipping node {index}.");
            return;
        }

        int enterDir = node.dir & 3;
        int outgoingDir = ApplyTileToDir(enterDir, node.tile);

        int prefabBaseFacing = Mathf.Clamp(md.BaseFacing, 0, 3);
        int deltaTurns = (outgoingDir - prefabBaseFacing + 4) & 3;
        float angle = deltaTurns * 90f;

        if (md.Direction == RoadDirection.Right) angle -= 180f;
        if (md.Direction == RoadDirection.Left) angle += 180f;
        angle = Mathf.Repeat(angle, 360f);

        Vector3 spawnWorld = Contexts.InGame.MapPlanner.CellToWorld(node.cell);

        string roadName = md.RoadPrefab.name;

        Map m = Managers.Object.Spawn<Map>(roadName, spawnWorld, 0, md.DataTemplateId, _spawnParent.transform);
        m.transform.SetParent(_spawnParent.transform, false);
        m.transform.rotation = Quaternion.Euler(0f, angle, 0f);
        m.SetDirection(outgoingDir);

        _roadStorage.Enqueue(m);

        _lastSpawnPos = spawnWorld;
        _lastSpawnRot = Quaternion.Euler(0f, angle, 0f);
        _lastSpawnAngle = angle;
    }

    private void RandomMap()
    {
        if (_mapCount == 0)
        {
            Debug.LogWarning("[MapSpawner] MapDatas is NULL");
            return;
        }

        int idx = Random.Range(0, _mapCount);
        var mapData = Managers.Data.MapDatas.Values.ElementAt(idx);
        if (mapData == null)
        {
            Debug.LogWarning("[MapSpawner] selected mapData is null");
            return;
        }

        int currentDir = Mathf.RoundToInt(_lastSpawnAngle / 90f) & 3;

        int outgoingDir = currentDir;
        if (mapData.Direction == RoadDirection.Left) outgoingDir = (currentDir + 3) & 3;
        else if (mapData.Direction == RoadDirection.Right) outgoingDir = (currentDir + 1) & 3;

        int prefabBaseFacing = Mathf.Clamp(mapData.BaseFacing, 0, 3);
        int deltaTurns = (outgoingDir - prefabBaseFacing + 4) & 3;
        float angle = deltaTurns * 90f;

        if (mapData.Direction == RoadDirection.Right) angle -= 180f;
        if (mapData.Direction == RoadDirection.Left) angle += 180f;
        angle = Mathf.Repeat(angle, 360f);

        Vector3 spawnWorld = _lastSpawnPos;

        string roadName = mapData.RoadPrefab.name;
        Map m = Managers.Object.Spawn<Map>(roadName, spawnWorld, 0, mapData.DataTemplateId, _spawnParent.transform);
        if (m == null)
        {
            Debug.LogError($"[MapSpawner] Failed to spawn map prefab '{roadName}'");
            return;
        }

        m.transform.SetParent(_spawnParent.transform, false);
        m.transform.rotation = Quaternion.Euler(0f, angle, 0f);

        // ★ 추가 — 이 맵에 방향 기록
        m.SetDirection(outgoingDir);

        _roadStorage.Enqueue(m);

        Vector3 newForward = DirIndexToVector(outgoingDir);
        Vector3 nextPos = spawnWorld + newForward * Contexts.InGame.MAP_SIZE;

        _lastSpawnPos = nextPos;
        _lastSpawnRot = Quaternion.Euler(0f, angle, 0f);
        _lastSpawnAngle = Mathf.Repeat(angle, 360f);
    }

    private int ApplyTileToDir(int dir, Tile tile)
    {
        dir &= 3;
        if (tile == Tile.Left) return (dir + 3) & 3;
        if (tile == Tile.Right) return (dir + 1) & 3;
        return dir;
    }

    private Vector3 DirIndexToVector(int dir)
    {
        switch (dir & 3)
        {
            case 0: return Vector3.forward;
            case 1: return Vector3.right;
            case 2: return Vector3.back;
            default: return Vector3.left;
        }
    }
}
