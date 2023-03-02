using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
using UnityEngine;

namespace Utils.Collision
{
    static class NearBy
    {
        public static IEnumerable<T> GetNearbyObjects<T>(this GameObject target)  where T : MonoBehaviour
        {
            var collisionDetector = target.GetComponent<CollisionDetector>();
            return collisionDetector.GetCollidingObjects().Select((obj) => obj.GetComponent<T>()).Where((obj) => !ReferenceEquals(obj, null));
        }
    }
}