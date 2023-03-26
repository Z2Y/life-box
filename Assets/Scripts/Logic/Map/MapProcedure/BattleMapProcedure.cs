using System;
using Controller;
using UnityEngine;

namespace Logic.Map.MapProcedure
{
    public abstract class BattleMapProcedure : ScriptableObject
    {
        protected BattlePlaceController place;
        
        public abstract void StartProcedure(BattlePlaceController place);

        public abstract void OnProcedureFinish(Action onFinish);

        public abstract void TerminateProcedure();

    }
}