using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModelContainer;
using UnityEngine;

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
        private Bounds bounds;
        private GridLayout ground;

        private void Awake()
        {
            worldCamera = Camera.main;
            ground = placeRoot.GetComponentInChildren<GridLayout>();
        }

        private void LateUpdate()
        {
            followCamera();
        }
        

        private void followCamera()
        {
            // todo
            updateWorldBounds();
            updateWorldPlaces();
        }

        private void updateWorldBounds()
        {
            var position = ground.WorldToCell(worldCamera.ViewportToWorldPoint(Vector3.zero));
            var rightTop = ground.WorldToCell(worldCamera.ViewportToWorldPoint(Vector3.one));

            var size = new Vector3((int)((rightTop.x - position.x + 1) * zoom), (int)((rightTop.y - position.y + 1) * zoom), 1);
            bounds = new Bounds(position, size);
        }

        private List<long> getPlacesInBounds()
        {
            return places.Where((place) => bounds.Intersects(place.bounds))
                .Select((place) => place.placeID)
                .ToList();
        }


        private void updateWorldPlaces()
        {
            // todo
        }
        
        public async Task InitMapWithPosition(Vector3 worldPosition)
        {
            var position = ground.WorldToCell(worldPosition);

            bounds = new Bounds(position, bounds.size);

            var placeIDs = getPlacesInBounds();

            var loadTasks = placeIDs.Select(PlaceController.LoadPlaceAsync).ToList();

            var loadedPlaces = new List<PlaceController>();

            while (loadTasks.Any())
            {
                var completed = await Task.WhenAny(loadTasks);
                if (completed.Result != null)
                {
                    loadedPlaces.Add(completed.Result);
                }
                loadTasks.Remove(completed);
            }

            await Task.WhenAll(activePlaces.Where((place) => !placeIDs.Contains(place.Place.ID)).Select((place) => place.DeActivate()));
            
            activePlaces = loadedPlaces;
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