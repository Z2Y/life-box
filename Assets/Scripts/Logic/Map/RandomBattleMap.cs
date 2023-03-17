using System;
using System.Linq;
using System.Threading.Tasks;
using Controller;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Logic.Map
{
    public class BattleMapRandomRoad : MonoBehaviour
    {
        [SerializeField] public long fromPlaceID;

        [SerializeField] public long[] toPlaceIDs;

        [SerializeField] private string rule;

        private void Awake()
        {
            // collider2D = GetComponentInChildren<Collider2D>();
        }

        private long GetNextPlaceID()
        {
            return toPlaceIDs[Random.Range(0, toPlaceIDs.Length)];
        }

        public async Task Jump()
        {
            var nextPlaceID = GetNextPlaceID();
            var nextPlace = await PlaceController.LoadPlaceAsync(nextPlaceID);
            if (nextPlace == null) return;

            await nextPlace.Activate();
            // todo Move Camera to target place.
        }
    }
}