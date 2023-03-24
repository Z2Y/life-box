using Logic.Detector;
using Logic.Map;
using UnityEngine;

namespace Controller
{
    public class PlaceInteractController : MonoBehaviour
    {
        private CollisionDetector collisionDetector;

        private readonly RouteResponderDetector _detector = new ();

        private PlaceController place;

        private void Awake()
        {
            collisionDetector = GetComponent<CollisionDetector>();
            place = GetComponentInParent<PlaceController>();
            
            _detector.onDetect(onDetect);
            _detector.onEndDetect(onEndDetect);
        }

        private void OnEnable()
        {
            collisionDetector.AddDetector(_detector);
        }
        
        private void OnDisable()
        {
            collisionDetector.RemoveDetector(_detector);
        }

        private void onEndDetect(IDetector detector, Collider2D collision)
        {
            // Debug.Log($"{collision.gameObject.name} Leave Place  {place.Place.MapID} {place.placeID}");
            collision.GetComponent<IRouteResponder>()?.OnLeave(place.Place.MapID, place.placeID);
        }

        private void onDetect(IDetector detector, Collider2D collision)
        {
            // Debug.Log($"{collision.gameObject.name} Enter Place {place.placeID}");
            collision.GetComponent<IRouteResponder>()?.OnEnter(place.Place.MapID, place.placeID);
        }
    }
}