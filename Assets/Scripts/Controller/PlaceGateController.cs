using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Controller
{
    public class PlaceGateController : MonoBehaviour
    {
        [SerializeField] public long fromPlaceID;

        [SerializeField] public long toPlaceID;

        [SerializeField] private string rule;
        

        // private Collider2D collider2D;

        private void Awake()
        {
            // collider2D = GetComponentInChildren<Collider2D>();
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            var npc = col.gameObject.GetComponentInChildren<NPCController>();
            if (npc != null) // npc entering
            {
                
            }
        }

        public async Task Jump()
        {
            var nextPlace = await PlaceController.LoadPlaceAsync(toPlaceID);
            if (nextPlace == null) return;

            var targetGate = nextPlace.GetComponents<PlaceGateController>()?.FirstOrDefault((gate) => gate.fromPlaceID == toPlaceID);
            if (targetGate == null) return;

            await nextPlace.Activate();
            // todo Move Camera to target place.
        }
    }
}