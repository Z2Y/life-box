using Controller;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Logic.Map
{
    public class BattleMapRandomGate : MonoBehaviour, IMapGate
    {
        [SerializeField] public long[] toPlaceIDs;

        [SerializeField] private string rule;

        [SerializeField] private ConnectDirection direction;
        private long fromMapID => fromPlace.Place.MapID;
        private WorldMapController map => WorldMapController.GetWorldMapController(fromMapID);
        
        public PlaceController fromPlace;
        private PlaceController nextPlace;

        private long GetNextPlaceID()
        {
            return toPlaceIDs[Random.Range(0, toPlaceIDs.Length)];
        }

        private async UniTask Jump()
        {
            var nextPlaceID = GetNextPlaceID();
            nextPlace = map.Places.Find((place) => place.placeID == nextPlaceID);
            

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

            await nextPlace.Activate();
            map.ActivatePlace(nextPlace);
            fromPlace.GetComponent<BattlePlaceController>().DisableAllGate();

            var mainCharacter = LifeEngine.Instance.MainCharacter;
            mainCharacter.Movement.AddLeavePlaceListener(OnLeavePlace);
            
        }

        private async void OnLeavePlace(long leavePlaceID)
        {
            if (leavePlaceID != fromPlace.placeID)
            {
                return;
            }
            var mainCharacter = LifeEngine.Instance.MainCharacter;
            mainCharacter.Movement.OffLeavePlaceListener(OnLeavePlace);

            map.DeActivatePlace(fromPlace);

            // await WorldCameraController.Instance.FollowTo(mainCharacter.gameObject, true, 1f);
            while (!WorldCameraController.Instance.isNearFollowTarget())
            {
                await YieldCoroutine.WaitForSeconds(0.125f);
            }
            await fromPlace.DeActivate();
            var battleController = nextPlace.GetComponent<BattlePlaceController>();
            battleController.Prepare();
            battleController.BeginProcedure();
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
            if (!Interactive()) return;
            Jump().Coroutine();
        }

        public bool Interactive()
        {
            return enabled;
        }
    }
}