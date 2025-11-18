using System;
using System.Collections.Generic;
using UnityEngine;

public enum Tile : byte
{
    Empty = 0,
    Straight = 1,
    Left = 2,
    Right = 3
}

public class MapPlanner
{
    private Tile[,] _grid;
    private int _width;
    private int _height;
    private int _cellSize;
    private Vector2Int _origin;
    public struct PathNode { public Vector2Int cell; public Tile tile; public int dir; }
    public List<PathNode> PathOrder { get; private set; } = new List<PathNode>();
    private System.Random _rng;

    // 디버그 카운터
    public int DebugTryCount { get; private set; }
    public int DebugBacktrackCount { get; private set; }

    public MapPlanner(int width = 100, int height = 100, int cellSize = 100, int? seed = null)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentException("Invalid map size");
        }
        _width = width;
        _height = height;
        _cellSize = cellSize;
        _grid = new Tile[_width, _height];
        _origin = new Vector2Int(_width / 2, _height / 2);
        _rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
    }

    public Vector3 CellToWorld(Vector2Int cell)
    {
        int worldX = (cell.x - _origin.x) * _cellSize;
        int worldZ = (cell.y - _origin.y) * _cellSize;
        return new Vector3(worldX, 0f, worldZ);
    }

    public bool InBounds(Vector2Int c) => c.x >= 0 && c.y >= 0 && c.x < _width && c.y < _height;
    public Tile GetTile(Vector2Int c) => InBounds(c) ? _grid[c.x, c.y] : Tile.Empty;
    private void SetTile(Vector2Int c, Tile t) { if (InBounds(c)) _grid[c.x, c.y] = t; }

    private Vector2Int DirToOffset(int dir)
    {
        switch (dir & 3)
        {
            case 0: return new Vector2Int(0, 1);
            case 1: return new Vector2Int(1, 0);
            case 2: return new Vector2Int(0, -1);
            default: return new Vector2Int(-1, 0);
        }
    }

    private int ApplyTileToDir(int dir, Tile tile)
    {
        if (tile == Tile.Left) return (dir + 3) & 3;
        if (tile == Tile.Right) return (dir + 1) & 3;
        return dir & 3;
    }

    public void DumpGridSummary()
    {
        int used = 0;
        for (int x = 0; x < _width; x++)
            for (int y = 0; y < _height; y++)
                if (_grid[x, y] != Tile.Empty) used++;

        Debug.Log($"[MapPlanner] Grid {_width}x{_height}, UsedTiles={used}, PathNodes={PathOrder.Count}, Tries={DebugTryCount}, Backtracks={DebugBacktrackCount}");
        if (_width <= 50 && _height <= 50)
        {
            string s = "";
            for (int y = _height - 1; y >= 0; y--)
            {
                for (int x = 0; x < _width; x++)
                {
                    s += (_grid[x, y] == Tile.Empty) ? "." : (_grid[x, y] == Tile.Straight ? "S" : (_grid[x, y] == Tile.Left ? "L" : "R"));
                }
                s += "\n";
            }
            Debug.Log("[MapPlanner] Grid dump:\n" + s);
        }
    }

    public Tile[,] GetGrid() => _grid;

    public bool GeneratePath(Vector2Int startCell, int startDir, int length, int maxBacktrackSteps = 100000)
    {
        int maxRetries = 6;
        int attemptLength = length;
        int baseSeed = Environment.TickCount;
        for (int retry = 0; retry < maxRetries; retry++)
        {
            int seed = baseSeed + retry * 10007;
            _rng = new System.Random(seed);

            Debug.Log($"[MapPlanner] GeneratePath attempt {retry + 1}/{maxRetries}, seed={seed}, targetLength={attemptLength}");

            bool ok = GeneratePathOnce(startCell, startDir, attemptLength, maxBacktrackSteps);
            if (ok) return true;

            // Reduce the target length slightly and retry
            attemptLength = Mathf.Max(4, (int)(attemptLength * 0.88f));
            Debug.LogWarning($"[MapPlanner] Retry: reducing targetLength -> {attemptLength}");
        }

        Debug.LogWarning("[MapPlanner] All retries failed in GeneratePath.");
        DumpGridSummary();
        return false;
    }

    
    private bool GeneratePathOnce(Vector2Int startCell, int startDir, int length, int maxBacktrackSteps)
    {
        PathOrder.Clear();
        Array.Clear(_grid, 0, _grid.Length);
        DebugTryCount = 0;
        DebugBacktrackCount = 0;

        if (!InBounds(startCell))
        {
            Debug.LogError("[MapPlanner] StartCell out of bounds: " + startCell);
            return false;
        }

        // 직진 가중치 많이 준 선택지 (코너 빈도는 여기서 조절)
        Tile[] weightedChoices = new Tile[]
        {
            Tile.Straight, Tile.Straight, Tile.Straight, Tile.Straight,
            Tile.Straight, Tile.Straight, Tile.Straight, Tile.Straight,
            Tile.Straight, Tile.Straight,     // 10개 = 직진
            Tile.Left,                        // 1개 = 왼쪽
            Tile.Right                        // 1개 = 오른쪽
        };

        Vector2Int curCell = startCell;
        int curDir = startDir & 3;

        int steps = 0;

        while (steps < length)
        {
            DebugTryCount++;

            // 이미 타일이 있는 셀이라면 여기에 또 놓을 수 없음 → 이번 시도 실패
            if (_grid[curCell.x, curCell.y] != Tile.Empty)
            {
                Debug.LogWarning("[MapPlanner] Current cell already used, dead end.");
                DumpGridSummary();
                return false;
            }

            // 선택지 복사 + 셔플
            Tile[] choices = (Tile[])weightedChoices.Clone();
            for (int i = 0; i < choices.Length; i++)
            {
                int j = _rng.Next(i, choices.Length);
                (choices[i], choices[j]) = (choices[j], choices[i]);
            }

            bool moved = false;

            for (int i = 0; i < choices.Length; i++)
            {
                Tile choice = choices[i];

                // 1) 현재 방향(curDir)과 타일(choice)을 기준으로 출구 방향 계산
                int exitDir = ApplyTileToDir(curDir, choice);

                // 2) 그 방향으로 한 칸 나간 다음 셀
                Vector2Int nextCell = curCell + DirToOffset(exitDir);

                // 3) 경계 밖이거나, 이미 사용된 셀로 이동하려 하면 스킵
                if (!InBounds(nextCell)) continue;
                if (_grid[nextCell.x, nextCell.y] != Tile.Empty) continue;

                // 4) ✅ "현재 셀(curCell)" 에 타일을 찍는다
                SetTile(curCell, choice);

                // dir = 이 타일을 지나고 난 "출구 방향"
                PathOrder.Add(new PathNode
                {
                    cell = curCell,
                    tile = choice,
                    dir  = exitDir & 3
                });

                steps++;

                // 5) 다음 루프는 nextCell 에서 시작
                curCell = nextCell;
                curDir  = exitDir & 3;

                moved = true;
                break;
            }

            if (!moved)
            {
                Debug.LogWarning("[MapPlanner] Dead end: no valid moves from " + curCell);
                DumpGridSummary();
                return false;
            }
        }

        Debug.Log($"[MapPlanner] GeneratePathOnce succeeded. Nodes={PathOrder.Count}, Tries={DebugTryCount}, Backtracks={DebugBacktrackCount}");
        DumpGridSummary();
        return true;
    }

}