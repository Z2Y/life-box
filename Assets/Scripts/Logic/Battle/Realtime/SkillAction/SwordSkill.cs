using System;
using System.Collections.Generic;
using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using Controller;
using HeroEditor.Common.Enums;
using Logic.Enemy;
using Model;
using UnityEngine;

namespace Logic.Battle.Realtime.SkillAction
{
    [Serializable]
    public class SwordSkillAction : ISkillAction {
        public Skill skill;
        public GameObject self;
        public AnimationClip skillAnimation;
        public List<GameObject> targets = new ();
        public string meleeSwordType;
        public BattleCostResult battleCostResult;
        public BattleEffectResult battleResult;

        private NPCAnimationController _controller;
        private CharacterState _originState;
        private int skillState;

        public void Init()
        {
            _controller = self.GetComponent<NPCAnimationController>();
        }

        public void Update()
        {
        }

        public bool isReady()
        {
            return skill != null && skillState == 2;
        }

        public bool isIdle()
        {
            return skill != null && skillState == 0;
        }

        public bool isPreparing() => skillState == 1;

        public void Reset() {
            targets.Clear();
            skillState = 0;
            battleCostResult = null;
        }

        // collect target info
        public void prepare()
        {
            _controller.GetReady();
            _controller.UseSword();
            _originState = _controller.GetState();
            switch (skill.WeaponType)
            {
                case WeaponType.Melee1H:
                case WeaponType.Melee2H:
                case WeaponType.MeleePaired:
                    skillState = 1;
                    _controller.AttackNormal();
                    SwordSlashController.Pool.Get(meleeSwordType).
                        Emit(self.transform, _controller.GetMeleeArm().position - self.transform.position, _controller.Speed, new SwordHitConfig()
                        {
                           enemyTag = "Enemy",
                           startHitDelay = 0,
                           hitDuration = 0.3f,
                           onHit = OnHit
                        });
                    if (_originState == CharacterState.Run)
                    {
                        _controller.SetState(CharacterState.Idle);
                    }
                    return;
                default:
                    skillState = 1;
                    return;
            }
            
        }

        public void endPrepare()
        {
            skillState = 2;
        }

        public async void DoSkill()
        {
            // var SkillEnv = new Dictionary<string, object>() { {"Skill" , this}};
            skillState = 3;
            try
            {
                // battleResult = await skill.Effect.ExecuteExpressionAsync(SkillEnv) as BattleEffectResult;
                await YieldCoroutine.WaitForSeconds(skill.CoolDown);
                skillState = 0;
            }
            catch
            {
                skillState = 0;
            }
            
        }

        private void OnHit(IHitResponder responder, Collider2D collider)
        {
            responder.onHit(self);
        }

    }
}