using System;
using Controller;
using UnityEngine;

namespace Logic.Detector
{
    public class NPCDetector : BaseDetector
    {
        public NPCDetector() : base("NPCDetector") {}
        public NPCDetector(Action<IDetector, Collision2D> callback) : base("NPCDetector")
        {
            onDetectCallback = callback;
        }

        public override bool isTarget(Collision2D collision)
        {
            if (collision.gameObject.layer == LayerMask.GetMask("NPC"))
            {
                fireCallbackAsync(collision);
                return true;
            }

            return false;
        }
    }
    
    public class TalkableNPCDetector : BaseDetector
    {
        public TalkableNPCDetector() : base("TalkableNPCDetector") {}
        public TalkableNPCDetector(Action<IDetector, Collision2D> callback) : base("TalkableNPCDetector")
        {
            onDetectCallback = callback;
        }

        public override bool isTarget(Collision2D collision)
        {
            var npcController = collision.gameObject.GetComponent<NPCController>();
            if (ReferenceEquals(npcController, null)) return false;
            return npcController.character.IsTalkable();
        }
    }


    public class ShopableNPCDetector : BaseDetector
    {
        public ShopableNPCDetector() : base("ShopableNPCDetector") {}
        public ShopableNPCDetector(Action<IDetector, Collision2D> callback) : base("TalkableNPCDetector")
        {
            onDetectCallback = callback;
        }

        public override bool isTarget(Collision2D collision)
        {
            var npcController = collision.gameObject.GetComponent<NPCController>();
            if (ReferenceEquals(npcController, null)) return false;
            return npcController.character.IsShopable();
        }        
    }
}