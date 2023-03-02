using System;
using System.Collections.Generic;
using UnityEngine;

namespace Controller
{

    public class CollisionDetector : MonoBehaviour
    {
        private readonly HashSet<GameObject> collidingObjects = new ();

        private void OnCollisionEnter(Collision collision)
        {
            collidingObjects.Add(collision.gameObject);
        }

        private void OnCollisionStay(Collision collision)
        {
            collidingObjects.Add(collision.gameObject);
        }

        private void OnCollisionExit(Collision collision)
        {
            collidingObjects.Remove(collision.gameObject);
        }

        // You can then access the set of colliding objects
        public HashSet<GameObject> GetCollidingObjects()
        {
            return collidingObjects;
        }
    }

}