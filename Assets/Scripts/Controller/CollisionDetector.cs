using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Detector;
using Logic.Detector.Scriptable;
using NPBehave;
using UnityEngine;

namespace Controller
{

    public class CollisionDetector : MonoBehaviour
    {
        private readonly HashSet<Collider2D> collidingObjects = new ();

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
            collidingObjects.Add(collision.collider);
            foreach (var detector in collisionDetectors)
            {
                detector.Start(DetectPhase.Enter, blackboard, collision.collider);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            
            collidingObjects.Add(other);
            if (!enabled) return;

            // Debug.Log($"Trigger Enter {other.gameObject.name}");
            foreach (var detector in collisionDetectors)
            {
                detector.Start(DetectPhase.Enter, blackboard, other);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            collidingObjects.Remove(other);
            if (!enabled) return;
            // Debug.Log($"Trigger Leave {other.gameObject.name}");
            foreach (var detector in collisionDetectors)
            {
                detector.Start(DetectPhase.Exit, blackboard, other);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (!enabled) return;
            collidingObjects.Remove(collision.collider);
            // Debug.Log($"{collision.gameObject.name} leave");
            foreach (var detector in collisionDetectors)
            {
                detector.Start(DetectPhase.Exit, blackboard, collision.collider);
            }
        }

        private void OnEnable()
        {
            blackboard.UpdateClock(UnityContext.GetClock());
            var removed = collidingObjects.RemoveWhere((obj) => obj == null || obj.gameObject == null);
            
            // Debug.Log($"Remove {removed} invalid object");
            
            foreach (var collidingObject in collidingObjects)
            {
                foreach (var detector in collisionDetectors)
                {
                    detector.Start(DetectPhase.Enter, blackboard, collidingObject);
                }
            }
        }

        private void OnApplicationQuit()
        {
            collidingObjects.Clear();
        }

        private void OnDisable()
        {
            foreach (var collidingObject in collidingObjects)
            {
                foreach (var detector in collisionDetectors)
                {
                    detector.Start(DetectPhase.Exit, blackboard, collidingObject);
                }
            }
        }

        /*
            private void OnCollisionStay(Collision collision)
            {
                collidingObjects.Add(collision.gameObject);
            }*/

        // You can then access the set of colliding objects
        public HashSet<Collider2D> GetCollidingObjects()
        {
            return collidingObjects;
        }
    }

}