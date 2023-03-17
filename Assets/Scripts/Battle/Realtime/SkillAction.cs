using System;
using System.Collections.Generic;
using System.Linq;
using Assets.HeroEditor.Common.Scripts.CharacterScripts;
using Battle.Realtime.Ai;
using Controller;
using HeroEditor.Common.Enums;
using Logic.Enemy;
using Model;
using UnityEngine;

namespace Battle.Realtime
{
    [Serializable]
    public class BattleSkillAction : ISkillAction {
        public Skill skill;
        public GameObject self;
        public AnimationClip skillAnimation;
        public List<GameObject> targets = new ();
        public string meleeSwordType;
        public BattleCostResult battleCostResult;
        public BattleEffectResult battleResult;

        private NPCAnimationController _controller;
        private CharacterState _originState;
        private int skillState = 0;
        private float prepareStartTime = 0;

        public void Init()
        {
            _controller = self.GetComponent<NPCAnimationController>();
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
            prepareStartTime = 0;
            battleCostResult = null;
        }

        // collect target info
        public void prepare()
        {
            _controller.GetReady();
            _originState = _controller.GetState();
            switch (skill.WeaponType)
            {
                case WeaponType.Bow:
                    skillState = 1;
                    _controller.BowCharge(1);
                    prepareStartTime = Time.time;
                    targets = GetEnemiesInSightRange(self.transform, skill.SelectRange, 120f).Take(1).ToList();
                    return;
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
            switch (skill.WeaponType)
            {
                case WeaponType.Bow:
                    var charged = Time.time - prepareStartTime > skillAnimation.length;
                    _controller.BowCharge(charged ? 2 : 3);
                    skillState = charged ? 2 : 0;
                    return;
                default:
                    skillState = 2;
                    return;
            }
        }

        public async void DoSkill()
        {
            var SkillEnv = new Dictionary<string, object>() { {"Skill" , this}};
            skillState = 3;
            try
            {
                battleResult = await skill.Effect.ExecuteExpressionAsync(SkillEnv) as BattleEffectResult;
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


        private List<GameObject> GetEnemiesInSightRange(Transform playerTransform, float sightRange, float sightAngle)
        {
            // 视野范围的一半的角度
            float halfSightAngle = sightAngle / 2f;
            // 角色的朝向
            Vector3 direction = playerTransform.right;
            // 扇形范围内的半径
            float radius = sightRange / 2f;
            // 扇形范围内的圆心位置
            Vector3 center = playerTransform.position + direction * radius;

            // 获取扇形范围内的所有物体
            Collider2D[] colliders = Physics2D.OverlapCapsuleAll(center, new Vector2(sightRange, radius), CapsuleDirection2D.Vertical, 0f);
            List<GameObject> enemies = new List<GameObject>();

            // 遍历所有物体，判断是否为敌人，并在扇形范围内
            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    Vector3 enemyDirection = collider.transform.position - playerTransform.position;
                    float angle = Vector3.Angle(direction, enemyDirection);
                    if (angle <= halfSightAngle && angle >= -halfSightAngle)
                    {
                        enemies.Add(collider.gameObject);
                    }
                }
            }

            enemies.Sort((a, b) => {
                var position = playerTransform.position;
                var distanceA = Vector3.Distance(a.transform.position, position);
                var distanceB = Vector3.Distance(b.transform.position, position);
                return distanceA.CompareTo(distanceB);
            });
            
            return enemies;
        }

    }
}