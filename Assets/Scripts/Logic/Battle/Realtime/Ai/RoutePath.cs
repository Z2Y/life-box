using System;
using UnityEngine;
using Utils;

namespace Logic.Battle.Realtime.Ai
{
    public class RoutePath : PoolObject, IDisposable {
        public Vector3Int Point;
        public RoutePath ParentRoute { get; private set; }

        private int gWeight;
        private int hWeight;
        public int fWeight;

        public void UpdateWeight(Vector3Int target)
        {
            hWeight = (Mathf.Abs(target.x - Point.x) + Mathf.Abs(target.y - Point.y)) * 10;
            if (ParentRoute != null)
            {
                var deltaX = Mathf.Abs(ParentRoute.Point.x - Point.x) * 10;
                var deltaY = Mathf.Abs(ParentRoute.Point.y - Point.y) * 10;
                gWeight = ParentRoute.gWeight + (int)Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
            }
            fWeight = hWeight + gWeight;
        }

        public void SetParent(RoutePath parent)
        {
            ParentRoute = parent;
        }

        public RoutePath Reverse() {
            if (ParentRoute == null) {
                return this;
            }
            var p = ParentRoute.Reverse();
            ParentRoute.ParentRoute = this;
            ParentRoute = null;
            return p;
        }

        public override void Dispose()
        {
            ParentRoute?.Dispose();
            hWeight = 0;
            fWeight = 0;
            gWeight = 0;
            ParentRoute = null;
            base.Dispose();
        }
    }
}