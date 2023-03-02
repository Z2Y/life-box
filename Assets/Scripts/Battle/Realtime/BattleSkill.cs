using System;
using Model;
using UnityEngine;

namespace Battle.Realtime
{
    public class BattleSkill : MonoBehaviour
    {
        [SerializeField] public Skill skill;

        [SerializeField] public KeyCode keycode;

        [SerializeField] public BattleSkillAction action;

        private float lastAttackTime = -1;

        private float attackInterval = 0.25f;

        private void Update()
        {
            if (Input.GetKeyDown(keycode))
            {
                DoSkill();
            }
        }

        private void DoSkill()
        {
            if (Time.time - lastAttackTime < attackInterval)
            {
                return;
            }

            if (skill.SkillType == SkillType.Attack)
            {
                
            }

            lastAttackTime = Time.time;
        }

    }
}