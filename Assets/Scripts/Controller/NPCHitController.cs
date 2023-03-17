using System;
using Assets.HeroEditor.Common.Scripts.CharacterScripts;
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
            var curState = npcController.Animator.GetState();
            if (curState is CharacterState.DeathF or CharacterState.DeathB)
            {
                return;
            }
            
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