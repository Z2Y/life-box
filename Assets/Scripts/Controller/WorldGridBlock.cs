using System.Collections.Generic;
using UnityEngine;

namespace Controller
{
    public class WorldGridBlock
    {
        private readonly Dictionary<Vector3Int, GridBlock> blocks = new ();

        public bool isBlocked(Vector3Int pos)
        {
            return blocks.ContainsKey(pos);
        }

        public bool addBlock(Vector3Int pos, GridBlock block)
        {
            return blocks.TryAdd(pos, block);
        }

        public bool removeBlock(Vector3Int pos, int instanceID)
        {
            if (!isBlocked(pos))
            {
                return false;
            }
            return blocks[pos].instanceID == instanceID && blocks.Remove(pos);
        }
    }

    public struct GridBlock
    {
        public int instanceID;
        public long mapID;
        public long placeID;
    } 
}