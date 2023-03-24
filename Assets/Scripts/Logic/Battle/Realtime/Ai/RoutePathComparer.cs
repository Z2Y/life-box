using System.Collections.Generic;
using System.Diagnostics;

namespace Logic.Battle.Realtime.Ai
{
    public class RoutePathComparer : IComparer<RoutePath> {

        public int Compare(RoutePath a, RoutePath b) {
            Debug.Assert(a != null, nameof(a) + " != null");
            Debug.Assert(b != null, nameof(b) + " != null");
            if (a.fWeight == b.fWeight) {
                return a.GetHashCode() - b.GetHashCode();
            }
            return a.fWeight - b.fWeight;
        }

    }
}