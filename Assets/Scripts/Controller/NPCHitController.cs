using System;
using Logic.Enemy;
using UnityEngine;

namespace Controller
{
    public class NPCHitController : MonoBehaviour, IHitResponder
    {
        private NPCController npcController;

        private void Awake()
        {
            npcController = gameObject.GetComponent<NPCController>();
        }

        public void onHit(GameObject from)
        {
            npcController.Animator.onHit(from);
            var hp = npcController.Property.GetProperty(SubPropertyType.HitPoint);
            
            hp.value -= 1;
            if (hp.value <= 0)
            {
                npcController.onDeath();
            }
        }
    }
}