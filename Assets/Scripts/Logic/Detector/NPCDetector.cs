using System;
using Controller;
using UnityEngine;
using UnityEngine.Events;

namespace Logic.Detector
{
    public class NPCDetector : BaseDetector
    {
        public NPCDetector() : base("NPCDetector") {}
        public NPCDetector(UnityAction<IDetector, Collider2D> callback) : base("NPCDetector")
        {
            onDetectCallback = callback;
        }

        protected override bool isTarget(Collider2D collision)
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
        public TalkableNPCDetector(UnityAction<IDetector, Collider2D> callback) : base("TalkableNPCDetector")
        {
            onDetectCallback = callback;
        }

        protected override bool isTarget(Collider2D collision)
        {
            var npcController = collision.gameObject.GetComponent<NPCController>();
            if (ReferenceEquals(npcController, null)) return false;
            return npcController.character.IsTalkable();
        }
    }


    public class ShopableNPCDetector : BaseDetector
    {
        public ShopableNPCDetector() : base("ShopableNPCDetector") {}
        public ShopableNPCDetector(UnityAction<IDetector, Collider2D> callback) : base("TalkableNPCDetector")
        {
            onDetectCallback = callback;
        }

        protected override bool isTarget(Collider2D collision)
        {
            var npcController = collision.gameObject.GetComponent<NPCController>();
            if (ReferenceEquals(npcController, null)) return false;
            return npcController.character.IsShopable();
        }        
    }
}