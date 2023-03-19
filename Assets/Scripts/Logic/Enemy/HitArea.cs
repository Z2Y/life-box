using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
using Logic.Detector;
using Logic.Detector.Scriptable;
using UnityEngine;
using UnityEngine.Events;

namespace Logic.Enemy
{
    public class HitArea : MonoBehaviour
    {
        private CollisionDetector collisionDetector;
        
        public ScriptableDetectorBase[] detectors;

        public UnityAction<Collider2D> _onDetect;

        private List<IDetector> _detectors = new();

        private void Awake()
        {
            collisionDetector = GetComponent<CollisionDetector>();
            foreach (var item in detectors)
            {
                var detector = item.GetDetector();
                _detectors.Add(detector);
                detector.onDetect(onDetect);
                detector.onEndDetect(onEndDetect);
                collisionDetector.AddDetector(detector);
            }
        }

        public void SetEnemyTag(String tagName)
        {
            foreach (var d in _detectors.OfType<NormalAttackDetector>())
            {
                d.SetTag(tagName);
            }
        }

        public void AddListener(UnityAction<Collider2D> onDetect)
        {
            _onDetect += onDetect;
        }

        public void RemoveListener(UnityAction<Collider2D> onDetect)
        {
            _onDetect -= onDetect;
        }

        private void OnEnable()
        {
            collisionDetector.enabled = true;
        }

        private void OnDisable()
        {
            collisionDetector.enabled = false;
        }

        private void onEndDetect(IDetector detector, Collider2D collision)
        {
        }

        private void onDetect(IDetector detector, Collider2D collision)
        {
            var hitResponder = collision.GetComponent<IHitResponder>();
            hitResponder.onHit(gameObject);
            _onDetect?.Invoke(collision);
        }
    }
}