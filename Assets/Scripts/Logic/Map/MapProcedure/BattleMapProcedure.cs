using System;
using UnityEngine;

namespace Logic.Map.MapProcedure
{
    public abstract class BattleMapProcedure : ScriptableObject
    {
        protected BattlePlaceController place;
        
        public abstract void StartProcedure(BattlePlaceController from);

        public abstract void OnProcedureFinish(Action onFinish);

        public abstract void TerminateProcedure();

    }
}