using System;
using System.Collections.Generic;
using Cathei.LinqGen;
using Controller;
using Logic.Map.MapProcedure;
using Logic.Message;
using ModelContainer;
using UniTaskPubSub;
using UnityEngine;
using Utils;

namespace Logic.Map
{
    public class BattlePlaceController : MonoBehaviour
    {
        private BattleMapRandomGate[] gates;

        [SerializeField]
        private List<BattleMapProcedure> procedures;
        
        [SerializeField]
        private bool beginOnStart;

        [SerializeField] 
        private string endRule;

        private PlaceController root;

        private BattleMapProcedure currentProcedure;

        [SerializeField] public int battleDepth;

        private int currentProcedureIndex;

        private bool completeOnProcedureEnd;

        private Action onComplete;

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

        private async void OnCompleteBattle()
        {
            var battleMsg = SimplePoolManager.Get<BattleComplete>();

            try
            {
                battleMsg.placeID = root.placeID;
                battleMsg.mapID = root.Place.MapID;
                Debug.Log($"Dispatch BattleComplete {battleMsg}");
                await AsyncMessageBus.Default.PublishAsync(battleMsg);
            }
            finally
            {
                battleMsg.Dispose();
            }
        }

        public void Prepare(int depth = 0)
        {
            var eventProcedures = PlaceTriggerContainer.GetPlaceTrigger(root.placeID)?.GetValidEvents().Gen().Select((@event) =>
            {
                var mapEvent = ScriptableObject.CreateInstance<BattleMapEvent>();
                mapEvent.mapEvent = @event;
                return (BattleMapProcedure)mapEvent;
            }).ToArray();

            if (eventProcedures is { Length: > 0 })
            {
                procedures = new List<BattleMapProcedure>(eventProcedures);
            }
            
            currentProcedure = procedures[currentProcedureIndex];
            battleDepth = depth;
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
                var result = endRule.ExecuteExpression();
                Debug.Log($"is Battle End {result}");
                if (result is true)
                {
                    var endProcedure = ScriptableObject.CreateInstance<BattleMapFinish>();
                    endProcedure.StartProcedure(this);
                    endProcedure.OnProcedureFinish(OnCompleteBattle);
                }
                else
                {
                    EnableAllGate();
                }
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