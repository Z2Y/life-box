using Logic.Map;
using UnityEngine;

namespace Logic.Detector
{
    public class MapGateDetector : BaseDetector
    {
        public MapGateDetector() : base("MapGateDetector") {}

        protected override bool isTarget(Collider2D collision)
        {
            if (!collision.CompareTag("Gate"))
            {
                return false;
            }

            var gate = collision.GetComponent<MapGate>();
            return !ReferenceEquals(gate, null);
        }
    }
}