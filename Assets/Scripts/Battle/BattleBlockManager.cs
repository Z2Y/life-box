using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class BattleBlockManager : MonoBehaviour
{
    public static BattleBlockManager Instance { get; private set; }

    private List<BattlePositonBlock> battleBlocks = new List<BattlePositonBlock>();

    private List<Vector3Int> circleOffsets = new List<Vector3Int>() {
        new Vector3Int(-1, 1, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, -1, 0),
        new Vector3Int(-1, -0, 0)
    };

    public Grid grid;
    public Tilemap tilemap;
    public TileBase Walkable;
    public TileBase Dangerous;
    public TileBase Normal;

    private void Awake()
    {
        Instance = this;
        InitializeBlock();
    }

    public void InitializeBlock()
    {
        Vector3Int leftbottom = grid.WorldToCell(Camera.main.ViewportToWorldPoint(Vector3.zero));
        Vector3Int righttop = grid.WorldToCell(Camera.main.ViewportToWorldPoint(Vector3.one));
        Vector3Int lefttop = grid.WorldToCell(Camera.main.ViewportToWorldPoint(Vector3.up));
        Vector3Int rightbottom = grid.WorldToCell(Camera.main.ViewportToWorldPoint(Vector3.right));

        for (int i = leftbottom.x; i <= righttop.x; i++)
        {
            for (int j = rightbottom.y; j <= lefttop.y; j++)
            {
                var tilePos = new Vector3Int(i, j, 0);
                var tile = tilemap.GetTile(tilePos);
                if (tile == null) continue;
                battleBlocks.Add(new BattlePositonBlock(tilePos));
                tilemap.SetTile(tilePos, null);
            }
        }
    }


    public List<BattlePositonBlock> GetBattlePositonBlocks()
    {
        return battleBlocks;
    }

    public List<BattlePositonBlock> GetBlocksByRange(Vector3Int center, int range = 0, BlockRangeType type = BlockRangeType.Dot)
    {
        List<BattlePositonBlock> blocks = new List<BattlePositonBlock>();
        var block = battleBlocks.FirstOrDefault(b => b.Position == center);

        if (block == null) return blocks;

        if (range == 0 || type == BlockRangeType.Dot)
        {
            blocks.Add(block);
            return blocks;
        }

        List<Vector3Int> neighbors = block.GetNeighbors(range);
        foreach (var neighbor in neighbors)
        {
            block = battleBlocks.Find(b => b.Position == neighbor);
            if (block != null)
            {
                blocks.Add(block);
            }
        }
        return blocks;
    }

    public void HideAllBlocks()
    {
        Vector3Int[] positions = battleBlocks.Select(block => block.Position).ToArray();
        TileBase[] tiles = battleBlocks.Select<BattlePositonBlock, TileBase>(block => null).ToArray();
        tilemap.SetTiles(positions, tiles);
    }

    public void ShowBlocks(List<BattlePositonBlock> blocks, BattleBlockType type = BattleBlockType.Moveable)
    {
        Vector3Int[] positions = blocks.Select(block => block.Position).ToArray();
        TileBase[] tiles = blocks.Select(block =>
        {
            switch (type)
            {
                case BattleBlockType.Moveable:
                    return Walkable;
                case BattleBlockType.Danger:
                    return Dangerous;
                case BattleBlockType.Normal:
                    return Normal;
                default:
                    return null;
            }
        }).ToArray();
        tilemap.SetTiles(positions, tiles);
    }

    private void Update()
    {
        /*if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;
            Vector3Int tilePos = tilemap.WorldToCell(worldPos);

            UnityEngine.Debug.Log(tilePos);

            tilemap.SetTile(tilePos, Dangerous);
        }*/
    }
}