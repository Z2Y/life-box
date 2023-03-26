using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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

        private Tilemap[] tilemaps;

        public Place Place => PlaceCollection.Instance.GetPlace(placeID);

        private void Awake()
        {
            tilemaps = GetComponentsInChildren<Tilemap>();
        }

        public void UpdateBoundsFormEditor()
        {
            tilemaps = GetComponentsInChildren<Tilemap>();
            updateBounds();
        }

        public void updateBounds()
        {
            var isFirst = true;
            foreach (var tilemap in tilemaps)
            {
                var cellBounds = tilemap.cellBounds;
                var cellSize = tilemap.CellToWorld(new Vector3Int(1, 1, 0)) - tilemap.CellToWorld(new Vector3Int(0, 0, 0));
                var tileBounds = new Bounds(transform.position, new Vector3(cellSize.x * cellBounds.size.x, cellSize.y * cellBounds.size.y, 0));
                if (isFirst)
                {
                    bounds = tileBounds;
                    isFirst = false;
                }
                else
                {
                    bounds.Encapsulate(tileBounds);
                }
            }
        }

        private void OnDestroy()
        {
            lookup.Remove(placeID);
        }

        public async UniTask Activate()
        {
            // updateBounds();
            gameObject.SetActive(true);
        }

        public async UniTask DeActivate()
        {
            // throw new NotImplementedException();
            await YieldCoroutine.WaitForInstruction(new WaitForEndOfFrame());
            gameObject.SetActive(false);
        }

        public static async UniTask UnloadPlaceAsync(long placeID)
        {
            var place = GetPlaceController(placeID);
            if (place != null)
            {
                await place.DeActivate();
                Destroy(place.gameObject);
            }
        }

        public static async UniTask<PlaceController> LoadPlaceAsync(long placeID, Transform parent = null)
        {
            var place = GetPlaceController(placeID);
            if (place != null)
            {
                return place;
            }

            place = await PrefabLoader<PlaceController, long>.CreateAsync(placeID, parent);

            if (place != null)
            {
                lookup[placeID] = place;
                place.OnLoaded(placeID);
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
            this.placeID = placeID;
            transform.localPosition = offset;
            updateBounds();
        }
    }
}