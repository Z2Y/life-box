using System;
using UnityEngine;

namespace Logic.Enemy
{
    public class AttackEvent : MonoBehaviour
    {
        public HitArea[] areas;

        private void Awake()
        {
            foreach (var area in areas)
            {
                area.AddListener(onHit);
            }
        }
        
        public void onBeginHitDetect()
        {
            foreach (var hitArea in areas)
            {
                hitArea.enabled = true;
            }
        }

        public void onEndHitDetect()
        {
            foreach (var hitArea in areas)
            {
                hitArea.enabled = false;
            }
        }

        private void onHit(Collider2D collision)
        {
            var hitResponder = collision.GetComponent<IHitResponder>();
            hitResponder?.onHit(gameObject);
        }
    }
}