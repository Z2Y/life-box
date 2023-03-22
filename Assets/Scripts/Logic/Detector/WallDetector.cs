using UnityEngine;

namespace Logic.Detector
{
    public class WallDetector : BaseDetector
    {
        public WallDetector() : base("WallDetector")
        {
        }

        protected override bool isTarget(Collider2D collision)
        {
            return collision.CompareTag("Wall");
        }
    }
}