using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logic.Map;
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
        
        [SerializeField] public long mapID;
        
        [SerializeField] private GameObject placeRoot;

        [SerializeField] private Camera worldCamera;

        [SerializeField] private float zoom;
        
        [SerializeField] private Bounds worldBounds;

        [SerializeField] private EdgeCollider2D wall;

        private List<PlaceController> activePlaces = new();
        private MapEdgeFog edgeFog;
        private bool mapUpdating;
        private Bounds visibleBounds;
        private GridLayout ground;

        public List<PlaceController> Places { get; private set; } = new();

        private void Awake()
        {
            worldCamera = Camera.main;
            ground = placeRoot.GetComponentInChildren<GridLayout>();
            edgeFog = wall.transform.GetComponent<MapEdgeFog>();
            updateVisibleBounds();
        }

        private void Update()
        {
            updateMap();
        }


        private async void updateMap()
        {
            if (mapUpdating) return;
            mapUpdating = true;
            updateVisibleBounds();
            // await updateWorldPlaces();
            await YieldCoroutine.WaitForSeconds(0.125f);
            mapUpdating = false;
        }

        private void updateVisibleBounds()
        {
            var position = worldCamera.ViewportToWorldPoint(Vector3.zero);
            var rightTop = worldCamera.ViewportToWorldPoint(Vector3.one);

            var size = new Vector3((rightTop.x - position.x), (rightTop.y - position.y), 0);
            visibleBounds = new Bounds((Vector2)worldCamera.ViewportToWorldPoint(Vector3.one / 2), size);
            
            edgeFog.UpdateFogBounds(visibleBounds, worldBounds);
        }

        private void updateWorldBounds()
        {
            var temp = new Bounds();
            var isFirst = true;
            foreach (var place in activePlaces)
            {
                if (isFirst)
                {
                    temp = place.bounds;
                    isFirst = false;
                }
                else
                {
                    temp.Encapsulate(place.bounds);  
                }
            }

            worldBounds = temp;
            wall.points = new Vector2[]
            {
                worldBounds.min, 
                worldBounds.min + new Vector3(worldBounds.size.x, 0, 0), 
                worldBounds.max,
                worldBounds.max - new Vector3(worldBounds.size.x, 0, 0),
                worldBounds.min, 
            };
        }

        public bool isGridPositionBlocked(Vector3Int pos)
        {
            // todo
            return false;
        }


        public GridLayout Ground => ground;

        private List<PlaceController> getPlacesInBounds()
        {
            return Places.Where((place) => visibleBounds.Intersects(place.bounds)).ToList();
        }


        private async Task updateWorldPlaces()
        {
            var placesInBounds = getPlacesInBounds();

            if (placesInBounds.Count == activePlaces.Count &&
                placesInBounds.All((place) => activePlaces.Contains(place)))
            {
                return;
            }

            await Task.WhenAll(placesInBounds.Select((place) => place.Activate()));

            await Task.WhenAll((activePlaces.Count > 0 ? activePlaces : Places).Where((place) => !placesInBounds.Contains(place)).Select((place) => place.DeActivate()));

            activePlaces = placesInBounds;
            updateWorldBounds();
        }
        
        public async Task InitMapWithPosition(Vector3 worldPosition)
        {
            mapUpdating = true;
            transform.position = worldPosition;

            visibleBounds = new Bounds(worldPosition, visibleBounds.size);

            await updateWorldPlaces();
            WorldCameraController.Instance.UpdateWorldBound(worldBounds.center, worldBounds.size - visibleBounds.size);
            mapUpdating = false;
        }

        public static void UnloadMap(long mapID)
        {
            var worldMap = GetWorldMapController(mapID);
            if (worldMap != null)
            {
                lookup.Remove(mapID);
                Destroy(worldMap.gameObject);
            }
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

            var placeRoot = worldMap.transform.Find("PlaceRoot");
            
            worldMap.Places = (await Task.WhenAll(PlaceCollection.Instance.Places.
                Where((place) => place.MapID == mapID).
                Select((place) => PlaceController.LoadPlaceAsync(place.ID, placeRoot)))).
                Where((p) => p != null).ToList();
            
            lookup[worldMap.mapID] = worldMap;

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