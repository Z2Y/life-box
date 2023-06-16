using System;
using Controller;
using UnityEngine;

namespace Logic.Map
{
    [RequireComponent(typeof(Collider2D))]
    public class MapBlocker : MonoBehaviour
    {
        [SerializeField] private BoundsInt bound;

        private PlaceController place;

        private WorldMapController map;

        private void Awake()
        {
            place = GetComponentInParent<PlaceController>();
            map = GetComponentInParent<WorldMapController>();
        }

        private void OnEnable()
        {
            var gridBlock = new GridBlock()
            {
                instanceID = gameObject.GetInstanceID(),
                mapID = map.mapID,
                placeID = place.placeID
            };
            bound.position = map.Ground.WorldToCell(place.transform.position); // update bound position
            foreach (var pos in bound.allPositionsWithin)
            {
                map.Blocks.addBlock(pos, gridBlock);
            }
        }

        private void OnDisable()
        {
            foreach (var pos in bound.allPositionsWithin)
            {
                map.Blocks.removeBlock(pos, gameObject.GetInstanceID());
            }
        }
    }
}