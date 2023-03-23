using System;
using UnityEngine;

namespace Logic.Map
{
    public class BattlePlaceController : MonoBehaviour
    {
        private BattleMapRandomGate[] gates;

        private void Awake()
        {
            gates = transform.Find("Gate").GetComponentsInChildren<BattleMapRandomGate>(true);
        }

        public void EnableAllGate()
        {
            foreach (var gate in gates)
            {
                enabled = true;
                gate.gameObject.SetActive(true);
            }
        }

        public void DisableAllGate()
        {
            foreach (var gate in gates)
            {
                enabled = false;
                gate.gameObject.SetActive(false);
            }
        }
    }
}