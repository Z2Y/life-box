using System;
using UnityEngine;

namespace Logic.Detector
{
    public class NPCDetector : BaseDetector, IDetector
    {
        public bool isTarget(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.GetMask("NPC"))
            {
                fireCallbackAsync(collision);
                return true;
            }

            return false;
        }

        private async void fireCallbackAsync(Collision collision)
        {
            if (onDetectCallback == null) return;
            await YieldCoroutine.WaitForInstruction(new WaitForEndOfFrame());
            onDetectCallback?.Invoke(collision);
        }

        public void onDetect(Action<Collision> callback)
        {
            onDetectCallback = callback;
        }
    }
    
    
}