using System;
using System.Collections.Generic;
using UnityEngine;

namespace Controller
{
    [RequireComponent(typeof(Collider2D))]
    public class FadingObject : InstanceTracker<FadingObject>
    {
        public GameObject target;

        public float alpha = 1, velocity, targetAlpha = 1;

        internal SpriteRenderer[] renderers;

        private void Awake()
        {
            if (target == null)
            {
                target = gameObject;
            }

            renderers = GetComponentsInChildren<SpriteRenderer>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            targetAlpha = 0.5f;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            targetAlpha = 1f;
        }
    }
}