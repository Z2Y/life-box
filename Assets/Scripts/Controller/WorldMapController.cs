using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModelContainer;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

namespace Controller
{
    [PrefabResourceWithArgs("Maps/{0:00}/Map")]
    public class WorldMapController : MonoBehaviour, IOnPrefabLoaded<long>
    {
        private static readonly Dictionary<long, WorldMapController> lookup = new();
        
        [SerializeField] private long mapID;
        
        [SerializeField] private GameObject placeRoot;

        [SerializeField] private Camera worldCamera;

        [SerializeField] private float zoom;

        private List<PlaceController> places = new();
        private List<PlaceController> activePlaces = new();
        private bool mapUpdating;
        private Bounds bounds;
        private GridLayout ground;

        private void Awake()
        {
            worldCamera = Camera.main;
            ground = placeRoot.GetComponentInChildren<GridLayout>();
        }

        private void Update()
        {
            updateMap();
        }


        private async void updateMap()
        {
            if (mapUpdating) return;
            updateWorldBounds();
            await updateWorldPlaces();
            await YieldCoroutine.WaitForSeconds(0.05f);
            mapUpdating = false;
        }

        private void updateWorldBounds()
        {
            var position = ground.WorldToCell(worldCamera.ViewportToWorldPoint(Vector3.zero));
            var rightTop = ground.WorldToCell(worldCamera.ViewportToWorldPoint(Vector3.one));

            var size = new Vector3((int)((rightTop.x - position.x + 1) * zoom), (int)((rightTop.y - position.y + 1) * zoom), 1);
            bounds = new Bounds(position, size);
        }

        public bool isGridPositionBlocked(Vector3Int pos)
        {
            // todo
            return true;
        }


        public GridLayout Ground => ground;

        private List<PlaceController> getPlacesInBounds()
        {
            return places.Where((place) => bounds.Intersects(place.bounds)).ToList();
        }


        private async Task updateWorldPlaces()
        {
            var placesInBounds = getPlacesInBounds();

            await Task.WhenAll(placesInBounds.Select((place) => place.Activate()));

            await Task.WhenAll(activePlaces.Where((place) => !placesInBounds.Contains(place)).Select((place) => place.DeActivate()));
        }
        
        public async Task InitMapWithPosition(Vector3 worldPosition)
        {
            var position = ground.WorldToCell(worldPosition);

            bounds = new Bounds(position, bounds.size);

            var placesInBounds = getPlacesInBounds();

            await Task.WhenAll(placesInBounds.Select((place) => place.Activate()));

            await Task.WhenAll(activePlaces.Where((place) => !placesInBounds.Contains(place)).Select((place) => place.DeActivate()));
            
            activePlaces = placesInBounds;
        }
        
        public static async Task<WorldMapController> LoadMapAsync(long mapID)
        {
            var worldMap = GetWorldMapController(mapID);
            if (worldMap != null)
            {
                return worldMap;
            }
            var worldRoot = GameObject.Find("WorldRoot");

            var maps = worldRoot.GetComponentsInChildren<WorldMapController>();

            var loaded = maps.FirstOrDefault((map) => map.mapID == mapID);

            if (loaded != null)
            {
                return loaded;
            }

            worldMap = await PrefabLoader<WorldMapController, long>.CreateAsync(mapID, worldRoot.transform);

            if (worldMap == null)
            {
                Debug.LogWarning($"Load Map {mapID} Failed, Resource Not Found.");
                return null;
            }
            
            worldMap.places = (await Task.WhenAll(PlaceCollection.Instance.Places.
                Where((place) => place.MapID == mapID).
                Select((place) => PlaceController.LoadPlaceAsync(place.ID)))).
                Where((p) => p != null).ToList();

            lookup.TryAdd(worldMap.mapID, worldMap);

            return worldMap;
        }

        public static WorldMapController GetWorldMapController(long mapId)
        {
            return lookup.TryGetValue(mapId, out var worldMap) ? worldMap : null;
        }
        
        public void OnLoaded(long placeID)
        {
            // todo
        }
        
    }
}