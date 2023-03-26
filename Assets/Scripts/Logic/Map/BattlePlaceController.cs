using System;
using Assets.HeroEditor.Common.Scripts.Common;
using Logic.Map.MapProcedure;
using UnityEngine;

namespace Logic.Map
{
    public class BattlePlaceController : MonoBehaviour
    {
        private BattleMapRandomGate[] gates;

        [SerializeField]
        private BattleMapProcedure[] procedures;

        private BattleMapProcedure currentProcedure;

        [SerializeField]
        private bool beginOnStart;

        private void Awake()
        {
            gates = transform.Find("Gate").GetComponentsInChildren<BattleMapRandomGate>(true);

        }

        private void OnDestroy()
        {
            if (currentProcedure != null)
            {
                currentProcedure.TerminateProcedure();
                currentProcedure = null;
            }
        }

        private void Start()
        {
            if (beginOnStart)
            {
                Prepare();
                BeginProcedure();
            }
        }

        public void EnableAllGate()
        {
            foreach (var gate in gates)
            {
                gate.gameObject.SetActive(true);
            }
        }

        public void DisableAllGate()
        {
            foreach (var gate in gates)
            {
                gate.gameObject.SetActive(false);
            }
        }

        public void Prepare()
        {
            currentProcedure = procedures.Random();
        }

        public void BeginProcedure()
        {
            DisableAllGate();
            currentProcedure.StartProcedure(this);
            currentProcedure.OnProcedureFinish(() =>
            {
                Debug.Log("Procedure End");
                EnableAllGate();
            });
        }
    }
}