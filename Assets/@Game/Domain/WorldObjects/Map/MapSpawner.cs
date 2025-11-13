using System.Collections.Generic;
using UnityEngine;
using UniRx;

/// <summary>
/// MapSpawner: blueprint 기반으로 동작
/// - MapPlanner로 blueprint를 생성(SetInfo에서)
/// - 내부 Queue는 _maxMapCapacity만큼 유지
/// - Map이 Despawn되면 blueprint 다음 인덱스 스폰
/// </summary>
public class MapSpawner : BaseObject
{
    private Queue<Map> _roadStorage;
    private GameObject _spawnParent;

    private const int MAP_SIZE = 100;
    private int _mapCount = 0;

    // planner / blueprint
    private MapPlanner _planner;
    private List<MapPlanner.PathNode> _blueprint = new List<MapPlanner.PathNode>();
    private int _blueprintIndex = 0;

    [SerializeField] private int _maxMapCapacity = 10; // inspector에서 조절 가능
    [SerializeField] private int _plannerGridW = 100;
    [SerializeField] private int _plannerGridH = 100;
    [SerializeField] private int _desiredBlueprintLength = 200;
    [SerializeField] private int _startDir = 0; // 0 = +Z

    private Vector3 _lastSpawnPos;
    private Quaternion _lastSpawnRot;
    private float _lastSpawnAngle = 0f;

    private Dictionary<Tile, MapData> _mapDataByTile = new Dictionary<Tile, MapData>();

    public override bool Init()
    {
        if (base.Init() == false) return false;

        _roadStorage = new Queue<Map>();
        _spawnParent = new GameObject("@Map");

        // MapData 매핑
        foreach (var md in Managers.Data.MapDatas.Values)
        {
            if (md.Direction == RoadDirection.none) _mapDataByTile[Tile.Straight] = md;
            else if (md.Direction == RoadDirection.Left) _mapDataByTile[Tile.Left] = md;
            else if (md.Direction == RoadDirection.Right) _mapDataByTile[Tile.Right] = md;
        }

        // 이벤트: 맵이 despawn되면 queue pop + 다음 blueprint 채우기
        Contexts.Map.OnDeSpawnRoad
            .Subscribe(_ =>
            {
                if (_roadStorage.Count > 0)
                {
                    _roadStorage.Dequeue();
                }
                SpawnUntilCapacity();
            })
            .AddTo(this);

        return true;
    }

    public override void SetInfo(int dataTemplate)
    {
        base.SetInfo(dataTemplate);
        _mapCount = Managers.Data.MapDatas.Count;

        _lastSpawnPos = new Vector3(0f, 0f, MAP_SIZE); // 기본 위치 (원한다면 변경)
        _lastSpawnRot = Quaternion.Euler(0f, 0f, 0f);
        _lastSpawnAngle = 0f;

        // Planner 생성 및 blueprint 제작
        _planner = new MapPlanner(_plannerGridW, _plannerGridH, MAP_SIZE);
        Vector2Int startCell = new Vector2Int(_plannerGridW / 2, _plannerGridH / 2);
        bool ok = _planner.GeneratePath(startCell, _startDir, _desiredBlueprintLength);

        if (!ok)
        {
            Debug.LogWarning("[MapSpawner] Planner failed to generate blueprint. Blueprint cleared.");
            _blueprint.Clear();
            _blueprintIndex = 0;
        }
        else
        {
            _blueprint = new List<MapPlanner.PathNode>(_planner.PathOrder);
            _blueprintIndex = 0;
            Debug.Log($"[MapSpawner] Blueprint generated with {_blueprint.Count} nodes.");
            // spawn up to capacity
            SpawnUntilCapacity();
        }

        // 기존 흐름과 호환되게 이벤트 트리거(원래 네 코드에서 했던 것 유지)
        Contexts.Map.OnSpawnRoad.OnNext(Unit.Default);
        Debug.Log("SetInfo MAP");
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
        if (index < 0 || index >= _blueprint.Count) return;

        var node = _blueprint[index];

        if (!_mapDataByTile.TryGetValue(node.tile, out MapData md))
        {
            Debug.LogError($"[MapSpawner] No MapData for Tile {node.tile}. Skipping node {index}.");
            return;
        }

        int outgoingDir = node.dir & 3;
        int prefabBaseFacing = Mathf.Clamp(md.BaseFacing, 0, 3);
        int deltaTurns = (outgoingDir - prefabBaseFacing + 4) & 3;
        float angle = deltaTurns * 90f;

        Vector3 spawnWorld = _planner.CellToWorld(node.cell);

        string roadName = md.RoadPrefab.name;
        Map m = Managers.Object.Spawn<Map>(roadName, spawnWorld, 0, md.DataTemplateId, _spawnParent.transform);
        if (m == null)
        {
            Debug.LogError($"[MapSpawner] Failed to spawn prefab {roadName}");
            return;
        }
        m.transform.SetParent(_spawnParent.transform, false);
        m.transform.rotation = Quaternion.Euler(0f, angle, 0f);

        _roadStorage.Enqueue(m);

        _lastSpawnPos = spawnWorld;
        _lastSpawnRot = Quaternion.Euler(0f, angle, 0f);
        _lastSpawnAngle = angle;

        Debug.Log($"[MapSpawner] Spawned blueprint node {index} prefab={roadName} cell={node.cell} dir={node.dir} angle={angle} queueCount={_roadStorage.Count}");
    }

    // 기존 RandomMap 유지(필요하면 사용)
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

    // 기존 위치/회전 업데이트 (보조용)
    private void GetNextSpawnPositionAndRotation(RoadDirection directionForThisMap)
    {
        float angle = _lastSpawnAngle;
        Vector3 currentPos = _lastSpawnPos;
        if (directionForThisMap == RoadDirection.Right) angle += 90f;
        else if (directionForThisMap == RoadDirection.Left) angle -= 90f;
        Vector3 newForward = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
        Vector3 nextPos = currentPos + newForward * MAP_SIZE;
        Quaternion nextRot = Quaternion.Euler(0f, angle, 0f);
        _lastSpawnPos = nextPos;
        _lastSpawnRot = nextRot;
        _lastSpawnAngle = Mathf.Repeat(angle, 360f);
    }
}
