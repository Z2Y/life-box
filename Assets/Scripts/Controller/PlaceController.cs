using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model;
using ModelContainer;
using UnityEngine;

namespace Controller
{
    public class PlaceController : MonoBehaviour
    {
        private static readonly Dictionary<long, PlaceController> lookup = new();
        
        [SerializeField] private long placeID;

        [SerializeField] private Vector3 offset;

        [SerializeField] private Bounds bounds;

        [SerializeField] public List<long> nearbyPlaceIDs;

        private Place place;

        private void Awake()
        {
            place = PlaceCollection.Instance.GetPlace(placeID);
            lookup.Add(placeID, this);
        }

        private void OnDestroy()
        {
            lookup.Remove(placeID);
        }

        public void Activate()
        {
            
        }

        public static async Task<PlaceController> LoadPlaceAsync(long placeID)
        {
            var place = GetPlaceController(placeID);
            if (place != null)
            {
                return place;
            }
            var loader = Resources.LoadAsync<GameObject>($"Places/{placeID}");
            while (!loader.isDone) {
                await YieldCoroutine.WaitForSeconds(0.01f);
            }

            if (loader.asset == null)
            {
                return null;
            }

            var obj = Instantiate(loader.asset as GameObject, GameObject.Find("PlaceRoot").transform);
            return obj.GetComponent<PlaceController>();
        }

        private static PlaceController GetPlaceController(long placeID)
        {
            return lookup.TryGetValue(placeID, out var placeController) ? placeController : null;
        }
    }
}