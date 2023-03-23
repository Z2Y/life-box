using Logic.Map;
using UnityEngine;

namespace Logic.Detector
{
    public class RouteResponderDetector : BaseDetector
    {
        public RouteResponderDetector() : base("RouteResponderDetector")
        {
        }

        protected override bool isTarget(Collider2D collision)
        {
            var responder = collision.GetComponent<IRouteResponder>();
            return !ReferenceEquals(responder, null);
        }
    }
}