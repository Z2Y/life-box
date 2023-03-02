using System;
using Model;
using UnityEngine;

namespace Battle.Realtime
{
    public class BattleSkill : MonoBehaviour
    {

        [SerializeField] public KeyCode keycode;

        [SerializeField] public BattleSkillAction action;

        private void Awake()
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
                DoSkill();
            }
        }

        private void DoPrepareSkill()
        {
            if (!action.isIdle())
            {
                return;
            }

            action.prepare();
        }

        private void DoSkill()
        {
            if (action.isPreparing())
            {
                action.endPrepare();
            }
            if (!action.isReady())
            {
                return;
            }

            action.DoSkill();
        }

    }
}