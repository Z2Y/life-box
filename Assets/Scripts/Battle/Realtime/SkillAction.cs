using System;
using System.Collections.Generic;
using Model;
using UnityEngine;

namespace Battle.Realtime
{
    [Serializable]
    public class BattleSkillAction {
        public Skill skill;
        public GameObject self;
        public List<GameObject> target;
        public BattleCostResult battleCostResult;

        public bool isExecutable()
        {
            return skill != null;
        }

        public void Reset() {
            target = null;
            battleCostResult = null;
        }

        // collect target info
        public void prepare()
        {
            
        }
    }
}