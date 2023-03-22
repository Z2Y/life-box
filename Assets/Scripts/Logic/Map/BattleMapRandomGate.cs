using System;
using System.Linq;
using System.Threading.Tasks;
using Controller;
using ModelContainer;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Logic.Map
{
    public class BattleMapRandomGate : MonoBehaviour, IMapGate
    {
        [SerializeField] public long fromPlaceID;

        [SerializeField] public long[] toPlaceIDs;

        [SerializeField] private string rule;

        [SerializeField] private ConnectDirection direction;
        private long fromMapID => PlaceCollection.Instance.GetPlace(fromPlaceID).MapID;
        private WorldMapController map => WorldMapController.GetWorldMapController(fromMapID);
        
        public PlaceController fromPlace;

        private long GetNextPlaceID()
        {
            return toPlaceIDs[Random.Range(0, toPlaceIDs.Length)];
        }

        private async Task Jump()
        {
            var nextPlaceID = GetNextPlaceID();
            Debug.Log($"{nextPlaceID} {map}");
            var nextPlace = map.Places.Find((place) => place.placeID == nextPlaceID);

            if (nextPlace == null) return;

            switch (direction)
            {
                case ConnectDirection.UP:
                    nextPlace.transform.position = fromPlace.transform.position + new Vector3(0, fromPlace.bounds.size.y, 0);
                    break;
                case ConnectDirection.Bottom:
                    nextPlace.transform.position = fromPlace.transform.position + new Vector3(0, -fromPlace.bounds.size.y, 0);
                    break;
                case ConnectDirection.Left:
                    nextPlace.transform.position = fromPlace.transform.position + new Vector3(-fromPlace.bounds.size.x, 0 , 0);
                    break;
                case ConnectDirection.Right:
                    nextPlace.transform.position = fromPlace.transform.position + new Vector3(fromPlace.bounds.size.x, 0, 0);
                    break;
            }

            await map.ActivatePlace(nextPlace);
            // todo Move Camera to target place.
        }

        private enum ConnectDirection
        {
            UP = 0,
            Left = 2,
            Right = 3,
            Bottom = 4
        }

        public void OnEnter()
        {
            Debug.Log($"Enter Place {fromPlaceID}");
            Jump().Coroutine();
        }
    }
}