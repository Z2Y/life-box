using System;
using Battle.Realtime.Ai;
using UnityEngine;

namespace Battle.Realtime
{
    public class BattleSkill : MonoBehaviour
    {

        [SerializeField] public KeyCode keycode;

        public ISkillAction action;

        private void Start()
        {
            action.Init();
        }

        private void Update()
        {
            if (Input.GetKeyDown(keycode))
            {
                DoPrepareSkill();
            }



            if (Input.GetKeyUp(keycode))
            {
                DoEndPrepareSkill();
            }
        }

        private void LateUpdate()
        {
            action.Update();
        }

        private void DoPrepareSkill()
        {
            if (!action.isIdle())
            {
                return;
            }

            if (!action.isReady())
            {
                action.prepare();
            }
            else
            {
                action.DoSkill();
            }
        }

        private void DoEndPrepareSkill()
        {
            if (!action.isPreparing())
            {
                return;
            }
            action.endPrepare();
            
            if (!action.isReady())
            {
                return;
            }
            
            action.DoSkill();
        }

    }
}