using System;
using System.Collections.Generic;
using Logic.Detector;
using UnityEngine;

namespace Controller
{

    public class CollisionDetector : MonoBehaviour
    {
        private readonly HashSet<GameObject> collidingObjects = new ();

        private readonly List<IDetector> collisionDetectors = new();

        public void AddDetector(IDetector detector)
        {
            collisionDetectors.Add(detector);
        }

        public void RemoveDetector(IDetector detector)
        {
            collisionDetectors.Remove(detector);
        }

        private void OnCollisionEnter(Collision collision)
        {
            collidingObjects.Add(collision.gameObject);
            foreach (var detector in collisionDetectors)
            {
                detector.Start(DetectPhase.Enter, collision);
            }
        }
        
        /*
        private void OnCollisionStay(Collision collision)
        {
            collidingObjects.Add(collision.gameObject);
        }*/

        private void OnCollisionExit(Collision collision)
        {
            collidingObjects.Remove(collision.gameObject);
            foreach (var detector in collisionDetectors)
            {
                detector.Start(DetectPhase.Exit, collision);
            }
        }

        // You can then access the set of colliding objects
        public HashSet<GameObject> GetCollidingObjects()
        {
            return collidingObjects;
        }
    }

}