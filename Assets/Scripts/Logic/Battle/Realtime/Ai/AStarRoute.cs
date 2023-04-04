using System;
using System.Collections.Generic;
using Controller;
using UnityEngine;
using Utils;

namespace Logic.Battle.Realtime.Ai
{
    public class AstarRoute : PoolObject, IDisposable {
        private readonly SortedSet<RoutePath> openList = new (new RoutePathComparer());
        private readonly HashSet<RoutePath> used = new();
        private readonly List<Vector3Int> nearbyPositions = new (8);
        private readonly Dictionary<Vector3Int, RoutePath> opened = new ();
        private readonly Dictionary<Vector3Int, bool> closed = new ();
        private readonly Dictionary<Vector3Int, bool> blocked = new ();

        public RoutePath FindPath(WorldMapController map, Vector3Int source, Vector3Int target, int maxWalk = 100) {
            var sourcePath = SimplePoolManager.Get<RoutePath>();
            sourcePath.Point = source;
            sourcePath.UpdateWeight(target);
            openList.Add(sourcePath);
            used.Add(sourcePath);

            if (isBlocked(map, target) || isBlocked(map, source)) {
                return null;
            }

            while (openList.Count > 0 && maxWalk > 0) {
                var current = openList.Min;
                openList.Remove(current);
                maxWalk--;

                if (current.Point == target) {
                    // sw.Stop();
                    // UnityEngine.Debug.Log(string.Format("Find in {0} step {1} ms", 100 - maxWalk, (sw.ElapsedTicks / 10000.0f)));
                    return prepareResultPath(current);
                }
                opened.Remove(current.Point);
                closed[current.Point] = true;

                var adjacent = adjacentPoints(map, current.Point);
                foreach (var p in adjacent)
                {
                    if (isClosed(p)) {
                        continue;
                    }
                    var newPath = SimplePoolManager.Get<RoutePath>();
                    newPath.Point = p;
                    newPath.SetParent(current);
                    newPath.UpdateWeight(target);
                    used.Add(newPath);

                    opened.TryGetValue(p, out var oldPath);

                    if (oldPath == null || newPath.fWeight < oldPath.fWeight) {
                        opened[p] = newPath;
                        openList.Add(newPath);
                    }
                }
            }

            Dispose();
            return null;
        }

        private RoutePath prepareResultPath(RoutePath target)
        {
            var path = target.Reverse();
            var pt = path;
            while (pt != null) // remove all path in used
            {
                used.Remove(pt);
                pt = pt.ParentRoute;
            }
            Dispose();
            return path;
        }

        private bool isClosed(Vector3Int pos) {
            return closed.ContainsKey(pos);
        }

        private bool isBlocked(WorldMapController map, Vector3Int pos) {
            if (blocked.ContainsKey(pos)) {
                return blocked[pos];
            }
            
            blocked[pos] = map.isGridPositionBlocked(pos);

            return blocked[pos];
        }

        private List<Vector3Int> adjacentPoints(WorldMapController map, Vector3Int pos) {
            nearbyPositions.Clear();
            for (var x = pos.x - 1; x <= pos.x + 1; x++) {
                for (var y = pos.y - 1; y <= pos.y + 1; y++) {
                    var next = new Vector3Int(x, y, 0);
                    if (!isBlocked(map, next)) {
                        nearbyPositions.Add(next);
                    }
                }
            }
            return nearbyPositions;
        }

        public override void Dispose()
        {
            foreach (var path in used)
            {
                path.Dispose();
            }
            openList.Clear();
            opened.Clear();
            closed.Clear();
            blocked.Clear();
            used.Clear();
            base.Dispose();
        }
    }
}