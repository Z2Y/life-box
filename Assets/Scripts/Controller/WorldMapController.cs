using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModelContainer;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Controller
{
    public class WorldMapController : MonoBehaviour
    {
        [SerializeField] private long mapID;
        
        [SerializeField] private GameObject placeRoot;

        [SerializeField] private Camera worldCamera;

        [SerializeField] private float zoom;

        private List<PlaceController> places;
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
            var worldRoot = GameObject.Find("WorldRoot");

            var maps = worldRoot.GetComponentsInChildren<WorldMapController>();

            var loaded = maps.FirstOrDefault((map) => map.mapID == mapID);

            if (loaded != null)
            {
                return loaded;
            }
            var loader = Resources.LoadAsync<GameObject>($"Maps/{mapID:00}/Map");

            while (!loader.isDone) {
                await YieldCoroutine.WaitForSeconds(0.01f);
            }
            
            Debug.Log(loader.asset);

            if (loader.asset == null)
            {
                return null;
            }

            var worldMap = Instantiate(loader.asset as GameObject, worldRoot.transform).GetComponent<WorldMapController>();
            worldMap.places = (await Task.WhenAll(PlaceCollection.Instance.Places.
                Where((place) => place.MapID == mapID).
                Select((place) => PlaceController.LoadPlaceAsync(place.ID)))).
                Where((p) => p != null).ToList();

            return worldMap;
        }

        
    }
}