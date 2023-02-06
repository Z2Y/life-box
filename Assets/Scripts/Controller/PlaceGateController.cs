using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Controller
{
    public class PlaceGateController : MonoBehaviour
    {
        [SerializeField] private long fromPlaceID;

        [SerializeField] private long toPlaceID;

        [SerializeField] private string rule;

        public async Task Jump()
        {
            var nextPlace = await PlaceController.LoadPlaceAsync(toPlaceID);
            if (nextPlace == null) return;

            var targetGate = nextPlace.GetComponents<PlaceGateController>()?.FirstOrDefault((gate) => gate.fromPlaceID == toPlaceID);
            if (targetGate == null) return;

            nextPlace.Activate();
            // todo Move Camera to target place.
        }
    }
}