using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model;
using ModelContainer;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

namespace Controller
{
    [PrefabResourceWithArgs("Places/{0}")]
    public class PlaceController : MonoBehaviour, IOnPrefabLoaded<long>
    {
        private static readonly Dictionary<long, PlaceController> lookup = new();

        [SerializeField] public long placeID;

        [SerializeField] private Vector3 offset;

        [SerializeField] public Bounds bounds;

        [SerializeField] public List<long> nearbyPlaceIDs;

        private Tilemap[] tilemaps;

        public Place Place { get; private set; }

        private void Awake()
        {
            Place = PlaceCollection.Instance.GetPlace(placeID);
            tilemaps = GetComponentsInChildren<Tilemap>();
            updateBounds();
        }

        public void updateBounds()
        {
            bounds = new Bounds();
            foreach (var tilemap in tilemaps)
            {
                var cellBounds = tilemap.cellBounds;
                var cellSize = tilemap.CellToWorld(new Vector3Int(1, 1, 0)) - tilemap.CellToWorld(new Vector3Int(0, 0, 0));
                var tileBounds = new Bounds(tilemap.CellToWorld(cellBounds.position), new Vector3(cellSize.x * cellBounds.size.x, cellSize.y * cellBounds.size.y, 0));
                
                bounds.Encapsulate(tileBounds);
            }
        }

        private void OnDestroy()
        {
            lookup.Remove(placeID);
        }

        public async Task Activate()
        {
            // throw new NotImplementedException();
        }

        public async Task DeActivate()
        {
            // throw new NotImplementedException();
        }

        public static async Task UnloadPlaceAsync(long placeID)
        {
            var place = GetPlaceController(placeID);
            if (place != null)
            {
                await place.DeActivate();
                Destroy(place.gameObject);
            }
        }

        public static async Task<PlaceController> LoadPlaceAsync(long placeID, Transform parent = null)
        {
            var place = GetPlaceController(placeID);
            if (place != null)
            {
                return place;
            }

            place = await PrefabLoader<PlaceController, long>.CreateAsync(placeID,
                parent);

            if (place != null)
            {
                lookup.TryAdd(placeID, place);
            }

            return place;
        }

        public static PlaceController GetPlaceController(long placeID)
        {
            return lookup.TryGetValue(placeID, out var placeController) ? placeController : null;
        }

        public void OnLoaded(long placeID)
        {
            // todo
        }
    }
}