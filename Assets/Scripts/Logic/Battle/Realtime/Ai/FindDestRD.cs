using UnityEngine;

namespace Logic.Battle.Realtime.Ai
{
    
    // Find Random Destination
    public class FindDestRD
    {
        private readonly int maxGridDistance;
        private readonly Vector3 origin;

        public FindDestRD(int maxDistance, Vector3 origin)
        {
            maxGridDistance = maxDistance;
            this.origin = origin;
        }

        public Vector3 GetResult()
        {
            var direction = new Vector3Int(Random.Range(-1, 2), Random.Range(-1, 2), 0);
            return origin + direction * Random.Range(1, maxGridDistance / Mathf.Max(1, (int)direction.magnitude));
        }
    }
}