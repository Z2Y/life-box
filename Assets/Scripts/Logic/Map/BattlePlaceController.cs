using System;
using System.Collections.Generic;
using System.Linq;
using Assets.HeroEditor.Common.Scripts.Common;
using Controller;
using Cysharp.Threading.Tasks;
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

        private int currentProcedureIndex;

        [SerializeField]
        private bool beginOnStart;

        private void Awake()
        {
            gates = transform.Find("Gate").GetComponentsInChildren<BattleMapRandomGate>(true);
            root = GetComponent<PlaceController>();
            currentProcedureIndex = 0;
        }

        private void OnDestroy()
        {
            if (currentProcedure != null)
            {
                currentProcedure.TerminateProcedure();
                currentProcedure = null;
            }
        }

        private async void Start()
        {
            if (beginOnStart)
            {
                await YieldCoroutine.WaitForSeconds(0.125f);
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
            var events = PlaceTriggerContainer.GetPlaceTrigger(root.placeID)?.GetValidEvents();

            if (events is { Count: > 0 })
            {
                procedures = events.Select((@event) =>
                {
                    var mapEvent = ScriptableObject.CreateInstance<BattleMapEvent>();
                    mapEvent.mapEvent = @event;
                    return (BattleMapProcedure)mapEvent;
                }).ToList();
            }
            
            currentProcedure = procedures[currentProcedureIndex];
        }

        private void NextProcedure()
        {
            if (currentProcedureIndex + 1 < procedures.Count)
            {
                currentProcedureIndex += 1;
                currentProcedure = procedures[currentProcedureIndex];
                currentProcedure.StartProcedure(this);
            }
            else
            {
                Debug.Log("End Procedures");
                EnableAllGate();
            }
        }

        public void BeginProcedure()
        {
            DisableAllGate();
            Debug.Log($"Start Procedure {currentProcedure}");
            currentProcedure.StartProcedure(this);
            currentProcedure.OnProcedureFinish(NextProcedure);
        }
    }
}