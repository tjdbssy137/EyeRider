// MapPlanner.cs (디버그 강화)
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
        if (width <= 0 || height <= 0) throw new ArgumentException("Invalid map size");
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

    // 디버그 요약 덤프: 작은 그리드는 전체 출력, 큰 그리드는 통계만 출력
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

    // GeneratePath with detailed debug
    // Replace the existing GeneratePath with this improved version
public bool GeneratePath(Vector2Int startCell, int startDir, int length, int maxBacktrackSteps = 100000)
{
    // Outer retry strategy: try multiple seeds/length reductions if initial generation fails.
    int maxRetries = 5;
    int attemptLength = length;
    int baseSeed = Environment.TickCount;
    for (int retry = 0; retry < maxRetries; retry++)
    {
        int seed = baseSeed + retry * 10007;
        _rng = new System.Random(seed);

        Debug.Log($"[MapPlanner] GeneratePath attempt {retry+1}/{maxRetries}, seed={seed}, targetLength={attemptLength}");

        bool ok = GeneratePathOnce(startCell, startDir, attemptLength, maxBacktrackSteps);
        if (ok) return true;

        // Reduce the target length slightly and retry, to avoid dead ends.
        attemptLength = Mathf.Max(4, (int)(attemptLength * 0.9f));
        Debug.LogWarning($"[MapPlanner] Retry: reducing targetLength -> {attemptLength}");
    }

    Debug.LogWarning("[MapPlanner] All retries failed in GeneratePath.");
    DumpGridSummary();
    return false;
}

    // helper method - single attempt using weighted choices (more straights)
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

        // Weighted choices: Straight more likely to avoid self-blocking
        Tile[] weightedChoices = new Tile[] {
        Tile.Straight, Tile.Straight, Tile.Straight, // 3x straight
        Tile.Left, Tile.Left,                         // 2x left
        Tile.Right                                     // 1x right
        // => P(straight)=3/6=50%, left=2/6~33%, right=1/6~17% (adjust as needed)
    };

        Stack<(Vector2Int cell, int dir, Tile[] choices, int choiceIndex)> stack =
            new Stack<(Vector2Int, int, Tile[], int)>();

        Vector2Int curCell = startCell;
        int curDir = startDir & 3;

        SetTile(curCell, Tile.Straight);
        PathOrder.Add(new PathNode { cell = curCell, tile = Tile.Straight, dir = curDir });

        int steps = 1;
        int backtrackCounter = 0;

        while (steps < length)
        {
            DebugTryCount++;

            // create a shuffled weighted choices array copy for this decision point
            Tile[] choices = (Tile[])weightedChoices.Clone();
            // Fisher-Yates shuffle
            for (int i = 0; i < choices.Length; i++)
            {
                int j = _rng.Next(i, choices.Length);
                Tile tmp = choices[i]; choices[i] = choices[j]; choices[j] = tmp;
            }

            bool moved = false;
            for (int i = 0; i < choices.Length; i++)
            {
                Tile choice = choices[i];
                int nextDir = ApplyTileToDir(curDir, choice);
                Vector2Int nextCell = curCell + DirToOffset(nextDir);

                // quick debug log for first few tries only to avoid flooding
                if (DebugTryCount < 50 || (DebugTryCount % 10000) == 0)
                {
                    Debug.Log($"[MapPlanner] Try#{DebugTryCount} choice {choice} -> nextCell {nextCell} inBounds={InBounds(nextCell)} occupied={(InBounds(nextCell) ? (_grid[nextCell.x, nextCell.y] != Tile.Empty).ToString() : "out")}");
                }

                if (!InBounds(nextCell)) continue;
                if (_grid[nextCell.x, nextCell.y] != Tile.Empty) continue;

                // Accept this choice
                stack.Push((curCell, curDir, choices, i));
                curCell = nextCell;
                curDir = nextDir;
                SetTile(curCell, choice);
                PathOrder.Add(new PathNode { cell = curCell, tile = choice, dir = curDir });
                steps++;
                moved = true;
                break;
            }

            if (!moved)
            {
                DebugBacktrackCount++;
                if (stack.Count == 0)
                {
                    Debug.LogWarning("[MapPlanner] No moves available and stack empty -> fail to generate path (single attempt).");
                    DumpGridSummary();
                    return false;
                }

                var top = stack.Pop();
                var last = PathOrder[PathOrder.Count - 1];
                SetTile(last.cell, Tile.Empty);
                PathOrder.RemoveAt(PathOrder.Count - 1);
                steps--;
                curCell = top.cell;
                curDir = top.dir;

                backtrackCounter++;
                if (backtrackCounter > maxBacktrackSteps)
                {
                    Debug.LogWarning($"[MapPlanner] Exceeded maxBacktrackSteps ({maxBacktrackSteps}) -> abort attempt.");
                    DumpGridSummary();
                    return false;
                }
            }
        }

        Debug.Log($"[MapPlanner] GeneratePathOnce succeeded. Nodes={PathOrder.Count}, Tries={DebugTryCount}, Backtracks={DebugBacktrackCount}");
        DumpGridSummary();
        return true;
    }

}
