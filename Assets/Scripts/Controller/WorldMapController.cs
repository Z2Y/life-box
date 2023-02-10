using System;
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
        
        [SerializeField] private GameObject PlaceRoot;

        [SerializeField] private Camera worldCamera;

        [SerializeField] private float zoom;

        
        private List<PlaceController> activePlaces = new();
        private BoundsInt bounds;
        private GridLayout ground;

        private void Awake()
        {
            ground = PlaceRoot.GetComponentInChildren<GridLayout>();
            // places = PlaceCollection.Instance.Places.Where((place) => place.MapID == mapID).ToList();
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

            var size = new Vector3Int((int)((rightTop.x - position.x + 1) * zoom), (int)((rightTop.y - position.y + 1) * zoom), 1);
            bounds = new BoundsInt(position, size);
        }

        private List<long> getPlacesInBounds()
        {
            throw new NotImplementedException();
        }


        private void updateWorldPlaces()
        {
            // todo
        }
        
        public async Task InitMapWithPosition(Vector3 worldPosition)
        {
            var position = ground.WorldToCell(worldPosition);

            bounds = new BoundsInt(position, bounds.size);

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
            var loader = Resources.LoadAsync<GameObject>($"Maps/{mapID}");
            while (!loader.isDone) {
                await YieldCoroutine.WaitForSeconds(0.01f);
            }

            if (loader.asset == null)
            {
                return null;
            }

            var obj = Instantiate(loader.asset as GameObject, worldRoot.transform);
            return obj.GetComponent<WorldMapController>();
        }

        
    }
}