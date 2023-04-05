using System.Collections.Generic;
using System.Linq;
using Cathei.LinqGen;
using Cysharp.Threading.Tasks;
using Logic.Map;
using Model;
using UnityEngine;
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

        
        private MapEdgeFog edgeFog;
        private bool mapUpdating;
        private Bounds visibleBounds;
        private GridLayout ground;

        public List<PlaceController> Places { get; private set; } = new ();
        public List<PlaceController> ActivePlaces { get; private set; } = new ();
        

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

            if (worldBounds.size != Vector3.zero)
            {
                edgeFog.UpdateFogBounds(visibleBounds, worldBounds);
            }
        }

        private void updateWorldBounds()
        {
            var temp = new Bounds();
            var isFirst = true;
            foreach (var place in ActivePlaces)
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
            WorldCameraController.Instance.UpdateWorldBound(worldBounds.center, worldBounds.size - visibleBounds.size);
        }

        public bool isGridPositionBlocked(Vector3Int pos)
        {
            // todo
            return false;
        }


        public GridLayout Ground => ground;

        private bool isPlaceVisible(PlaceController place)
        {
            Debug.Log($"is Place visible");
            return visibleBounds.Intersects(place.bounds);
        }

        private bool isPlaceActive(PlaceController place)
        {
            return place.gameObject.activeInHierarchy;
        }


        private async UniTask updateWorldPlaces()
        {
            await UniTask.WhenAll(Places.Gen().Select((place) => isPlaceVisible(place) ? place.Activate() : place.DeActivate())
                .AsEnumerable());

            ActivePlaces.Clear();
            ActivePlaces.AddRange(Places.Gen().Where(isPlaceActive).AsEnumerable());
            updateWorldBounds();
        }
        
        public async UniTask InitMapWithPosition(Vector3 worldPosition)
        {
            mapUpdating = true;
            transform.position = worldPosition;

            visibleBounds = new Bounds(worldPosition, visibleBounds.size);

            await updateWorldPlaces();
            mapUpdating = false;
        }

        public void ActivatePlace(PlaceController target)
        {
            if (ActivePlaces.Gen().Any((p) => p.placeID == target.placeID))
            {
                return;
            }
            target.updateBounds();
            ActivePlaces.Add(target);
            updateWorldBounds();
        }

        public void DeActivatePlace(PlaceController target)
        {
            ActivePlaces.Remove(target);
            updateWorldBounds();
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
        
        public static async UniTask<WorldMapController> LoadMapAsync(long mapID)
        {
            var worldMap = GetWorldMapController(mapID);
            if (worldMap != null)
            {
                return worldMap;
            }
            var worldRoot = GameObject.Find("WorldRoot");

            var maps = worldRoot.GetComponentsInChildren<WorldMapController>();

            var loaded = maps.Gen().Where((map) => map.mapID == mapID).FirstOrDefault();

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
            
            lookup[worldMap.mapID] = worldMap;

            var placeRoot = worldMap.transform.Find("PlaceRoot");
            
            worldMap.Places = (await UniTask.WhenAll(RealmDBController.Db.All<Place>().
                Where((place) => place.MapID == mapID).ToList().
                Select((place) => PlaceController.LoadPlaceAsync(place.ID, placeRoot)))).
                Where(LinqHelper.IsNotNull).ToList();

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