using System;
using Controller;
using Logic.Detector;
using Logic.Detector.Scriptable;
using UnityEngine;

namespace Logic.Enemy
{
    public class HitArea : MonoBehaviour
    {
        private CollisionDetector collisionDetector;
        
        private ScriptableDetectorBase[] detectors;

        private void Awake()
        {
            collisionDetector = GetComponent<CollisionDetector>();
            foreach (var item in detectors)
            {
                var detector = item.GetDetector();
                detector.onDetect(onDetect);
                detector.onEndDetect(onEndDetect);
                collisionDetector.AddDetector(detector);
            }
        }
        
        private void onEndDetect(IDetector detector, Collider2D collision)
        {
        }

        private void onDetect(IDetector detector, Collider2D collision)
        {
            
        }
    }
}