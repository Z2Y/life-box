using System.Collections.Generic;
using Controller;
using UnityEngine;

namespace Utils.Collision
{
    static class NearBy
    {
        public static IEnumerable<Collider2D> GetNearbyObjects(this GameObject target)
        {
            var collisionDetector = target.GetComponent<CollisionDetector>();
            return collisionDetector.GetCollidingObjects();
        }
    }
}