using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

/// <summary>
/// MapSpawner (수정본) — RandomMap() 및 Blueprint 스폰에서
/// 프리팹 회전 계산 후 Left/Right 타일일 경우 -90도 보정 적용.
/// 프로젝트의 Managers, Map, MapData, RoadDirection, Contexts 등은 기존과 동일하다고 가정합니다.
/// </summary>
public class MapSpawner : BaseObject
{
    private Queue<Map> _roadStorage;
    private GameObject _spawnParent;

    private const int MAP_SIZE = 100;
    private int _mapCount = 0;

    private MapPlanner _planner;
    private List<MapPlanner.PathNode> _blueprint = new List<MapPlanner.PathNode>();
    private int _blueprintIndex = 0;

    [SerializeField] private int _maxMapCapacity = 10;
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

        if (Managers.Data?.MapDatas != null)
        {
            foreach (var md in Managers.Data.MapDatas.Values)
            {
                if (md.Direction == RoadDirection.none)
                {
                    _mapDataByTile[Tile.Straight] = md;
                }
                else if (md.Direction == RoadDirection.Left)
                {
                    _mapDataByTile[Tile.Left] = md;
                }
                else if (md.Direction == RoadDirection.Right)
                {
                    _mapDataByTile[Tile.Right] = md;
                }
            }
        }

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

        _lastSpawnPos = new Vector3(0f, 0f, MAP_SIZE);
        _lastSpawnRot = Quaternion.Euler(0f, 0f, 0f);
        _lastSpawnAngle = 0f;

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
            SpawnUntilCapacity();
        }

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

        while (_roadStorage.Count < _maxMapCapacity)
        {
            RandomMap();
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

        // ====== 여기서 Left/Right 타일이면 -90도 보정 적용 ======
        if (md.Direction == RoadDirection.Right)
        {
            angle -= 90f;
        }
        if (md.Direction == RoadDirection.Left)
        {
            angle += 90f;
        }
        angle = Mathf.Repeat(angle, 360f);
        // ====================================================

        Vector3 spawnWorld = _planner.CellToWorld(node.cell);

        string roadName = md.RoadPrefab.name;
        Map m = Managers.Object.Spawn<Map>(roadName, spawnWorld, 0, md.DataTemplateId, _spawnParent.transform);
        m.transform.SetParent(_spawnParent.transform, false);
        m.transform.rotation = Quaternion.Euler(0f, angle, 0f);

        _roadStorage.Enqueue(m);

        _lastSpawnPos = spawnWorld;
        _lastSpawnRot = Quaternion.Euler(0f, angle, 0f);
        _lastSpawnAngle = angle;

        Debug.Log($"[MapSpawner] Spawned blueprint node {index} prefab={roadName} cell={node.cell} dir={node.dir} angle={angle} queueCount={_roadStorage.Count}");
    }

    // ---------- 수정된 RandomMap (동일 보정 적용) ----------
    private void RandomMap()
    {
        if (_mapCount == 0)
        {
            Debug.LogWarning("[MapSpawner] _mapCount is 0 - no MapDatas available");
            return;
        }

        int idx = Random.Range(0, _mapCount);
        var mapData = Managers.Data.MapDatas.Values.ElementAt(idx);
        if (mapData == null)
        {
            Debug.LogWarning("[MapSpawner] selected mapData is null");
            return;
        }

        // 현재 바라보는 방향: 0=+Z, 1=+X, 2=-Z, 3=-X
        int currentDir = Mathf.RoundToInt(_lastSpawnAngle / 90f) & 3;

        int outgoingDir = currentDir;
        if (mapData.Direction == RoadDirection.Left) outgoingDir = (currentDir + 3) & 3;
        else if (mapData.Direction == RoadDirection.Right) outgoingDir = (currentDir + 1) & 3;

        int prefabBaseFacing = Mathf.Clamp(mapData.BaseFacing, 0, 3);
        int deltaTurns = (outgoingDir - prefabBaseFacing + 4) & 3;
        float angle = deltaTurns * 90f;

        // ====== Left/Right 보정: -90도 적용 ======
        if (mapData.Direction != RoadDirection.none)
        {
            angle -= 90f;
        }
        angle = Mathf.Repeat(angle, 360f);
        // =======================================

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

        _roadStorage.Enqueue(m);

        Vector3 newForward = DirIndexToVector(outgoingDir);
        Vector3 nextPos = spawnWorld + newForward * MAP_SIZE;
        Quaternion nextRot = Quaternion.Euler(0f, angle, 0f);

        _lastSpawnPos = nextPos;
        _lastSpawnRot = nextRot;
        _lastSpawnAngle = Mathf.Repeat(angle, 360f);

        Debug.Log($"[MapSpawner] Random spawned '{roadName}' idx:{idx} dir:{mapData.Direction} outgoing:{outgoingDir} baseFacing:{prefabBaseFacing} angle:{angle} nextPos:{_lastSpawnPos} queue:{_roadStorage.Count}");

        if (_roadStorage.Count < _maxMapCapacity)
        {
            Contexts.Map.OnSpawnRoad.OnNext(Unit.Default);
        }
    }

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

    // 유틸: 0:+Z, 1:+X, 2:-Z, 3:-X
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

    public void SpawnRandomOnce()
    {
        RandomMap();
    }

    public void SetStart(Vector3 startPos, float startAngle = 0f)
    {
        _lastSpawnPos = startPos;
        _lastSpawnAngle = Mathf.Repeat(startAngle, 360f);
        _lastSpawnRot = Quaternion.Euler(0f, _lastSpawnAngle, 0f);
    }
}
