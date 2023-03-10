using System;
using System.Collections.Generic;
using Logic.Detector;
using NPBehave;
using UnityEngine;

namespace Controller
{

    public class CollisionDetector : MonoBehaviour
    {
        private readonly HashSet<GameObject> collidingObjects = new ();

        private readonly List<IDetector> collisionDetectors = new();
        
        private readonly Blackboard blackboard = new (null, UnityContext.GetClock());

        public void AddDetector(IDetector detector)
        {
            collisionDetectors.Add(detector);
        }

        public void RemoveDetector(IDetector detector)
        {
            collisionDetectors.Remove(detector);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            collidingObjects.Add(collision.gameObject);
            Debug.Log($"{collision.gameObject.name} enter");
            foreach (var detector in collisionDetectors)
            {
                detector.Start(DetectPhase.Enter, blackboard, collision);
            }
        }
        
        /*
        private void OnCollisionStay(Collision collision)
        {
            collidingObjects.Add(collision.gameObject);
        }*/

        private void OnCollisionExit2D(Collision2D collision)
        {
            collidingObjects.Remove(collision.gameObject);
            Debug.Log($"{collision.gameObject.name} leave");
            foreach (var detector in collisionDetectors)
            {
                detector.Start(DetectPhase.Exit, blackboard, collision);
            }
        }

        // You can then access the set of colliding objects
        public HashSet<GameObject> GetCollidingObjects()
        {
            return collidingObjects;
        }
    }

}