using UnityEngine;

namespace Logic.Detector
{
    public class NPCDetector : IDetector
    {
        public bool isTarget(Collision collision)
        {
            return collision.gameObject.layer == LayerMask.GetMask("NPC");
        }
    }
    
    
}