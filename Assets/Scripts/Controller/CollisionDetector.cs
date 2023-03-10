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
        
        private Blackboard blackboard;

        private void Awake()
        {
            blackboard = new Blackboard(null, UnityContext.GetClock());
        }

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
            if (!enabled) return;
            collidingObjects.Add(collision.gameObject);
            foreach (var detector in collisionDetectors)
            {
                detector.Start(DetectPhase.Enter, blackboard, collision.collider);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!enabled) return;
            collidingObjects.Add(other.gameObject);

            Debug.Log($"Trigger Enter {other.gameObject.name}");
            foreach (var detector in collisionDetectors)
            {
                detector.Start(DetectPhase.Enter, blackboard, other);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!enabled) return;
            collidingObjects.Remove(other.gameObject);
            Debug.Log($"Trigger Leave {other.gameObject.name}");
            foreach (var detector in collisionDetectors)
            {
                detector.Start(DetectPhase.Exit, blackboard, other);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (!enabled) return;
            collidingObjects.Remove(collision.gameObject);
            Debug.Log($"{collision.gameObject.name} leave");
            foreach (var detector in collisionDetectors)
            {
                detector.Start(DetectPhase.Exit, blackboard, collision.collider);
            }
        }
        
        /*
        private void OnCollisionStay(Collision collision)
        {
            collidingObjects.Add(collision.gameObject);
        }*/

        // You can then access the set of colliding objects
        public HashSet<GameObject> GetCollidingObjects()
        {
            return collidingObjects;
        }
    }

}