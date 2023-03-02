using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public enum BattleBlockType {
    Moveable,
    Danger,
    Normal,
}

public enum BlockRangeType {
    Dot,
    Cross,
    Circle,
}

public class BattlePositionBlock
{
    private List<Vector3Int> neighbors;

    private static List<Vector3Int> offset0 = new List<Vector3Int>() {
        new Vector3Int(-1, 1, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, -1, 0),
        new Vector3Int(-1, 0, 0)
    };

    private static List<Vector3Int> offset1 = new List<Vector3Int>() {
        new Vector3Int(0, 1, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(1, -1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, -0, 0)
    };
    public Vector3Int Position;

    public Vector3 WorldPosition {
        get {
            return BattleBlockManager.Instance.tilemap.CellToWorld(Position);
        }
    }

    public BattlePositionBlock(Vector3Int pos) {
        Position = pos;
    }

    public List<Vector3Int> GetNeighbors()
    {
        if (Math.Abs(Position.y) %2 == 0) {
            return offset0.Select((offset) => Position + offset).ToList();
        }
        return offset1.Select((offset) => Position + offset).ToList();
    }

    public int GetDistance(BattlePositionBlock other) {
        Vector3Int vec = (other.Position - Position);
        int maxDis = Math.Abs(vec.x) + Math.Abs(vec.y);
        int dis = int.MaxValue;
        GetNeighborDistance(maxDis).TryGetValue(other.Position, out dis);
        return dis;
    }

    public Dictionary<Vector3Int, int> GetNeighborDistance(int range)
    {
        Dictionary<Vector3Int, int> result = new Dictionary<Vector3Int, int>();
        if (range == 0) return result;
        Queue<BattlePositionBlock> que = new Queue<BattlePositionBlock>();
        Dictionary<Vector3Int, int> visit = new Dictionary<Vector3Int, int>();
        que.Enqueue(this);
        result.Add(this.Position, 0);
        while (que.Count > 0)
        {
            var block = que.Dequeue();
            int distance = result[block.Position];
            if (distance >= range) continue;
            var neighbors = block.GetNeighbors();
            foreach(var neighbor in neighbors) {
                if (!result.ContainsKey(neighbor)) {
                    result[neighbor] = distance + 1;
                    que.Enqueue(new BattlePositionBlock(neighbor));
                }
            }
        }
        visit.Remove(this.Position);
        return result;       
    }

    public List<Vector3Int> GetNeighbors(int range)
    {
        var neighborDistances = GetNeighborDistance(range);
        var neighbors = neighborDistances.Keys.ToList();
        neighbors.Sort((posA , posB) => neighborDistances[posA].CompareTo(neighborDistances[posB]));
        return neighbors;
    }
}