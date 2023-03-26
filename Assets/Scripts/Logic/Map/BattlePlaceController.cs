using System;
using System.Collections.Generic;
using System.Linq;
using Assets.HeroEditor.Common.Scripts.Common;
using Controller;
using Logic.Map.MapProcedure;
using Model;
using ModelContainer;
using UnityEngine;

namespace Logic.Map
{
    public class BattlePlaceController : MonoBehaviour
    {
        private BattleMapRandomGate[] gates;

        [SerializeField]
        private List<BattleMapProcedure> procedures;

        private PlaceController root;

        private BattleMapProcedure currentProcedure;

        [SerializeField]
        private bool beginOnStart;

        private void Awake()
        {
            gates = transform.Find("Gate").GetComponentsInChildren<BattleMapRandomGate>(true);
            root = GetComponent<PlaceController>();
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
            var events = PlaceTriggerContainer.Instance.GetPlaceTrigger(root.placeID).GetValidEvents();

            if (events.Count > 0)
            {
                procedures = events.Select((@event) =>
                {
                    var mapEvent = ScriptableObject.CreateInstance<BattleMapEvent>();
                    mapEvent.mapEvent = @event;
                    return (BattleMapProcedure)mapEvent;
                }).ToList();
                currentProcedure = procedures.First();
            }
            else
            {
                currentProcedure = procedures.Random();
            }
        }

        public void BeginProcedure()
        {
            DisableAllGate();
            Debug.Log($"Start Procedure {currentProcedure}");
            currentProcedure.StartProcedure(this);
            currentProcedure.OnProcedureFinish(() =>
            {
                Debug.Log("Procedure End");
                EnableAllGate();
            });
        }
    }
}