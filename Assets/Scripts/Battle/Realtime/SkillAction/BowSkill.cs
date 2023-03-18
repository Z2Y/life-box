using System;
using System.Collections.Generic;
using System.Linq;
using Battle.Realtime.Ai;
using Controller;
using HeroEditor.Common.Enums;
using Logic.Enemy;
using Model;
using UnityEngine;
using Character = Assets.HeroEditor.Common.Scripts.CharacterScripts.Character;

namespace Battle.Realtime
{
    [Serializable]
    public class BowSkillAction : ISkillAction
    {
        public Skill skill;
        public GameObject self;
        public AnimationClip skillAnimation;
        public List<GameObject> targets = new ();

        public BattleCostResult battleCostResult;
        public BattleEffectResult battleResult;

        private NPCAnimationController _controller;
        private Character character;
        private Transform _arm;
        private Transform _weapon;
        
        private int skillState = 0;
        private float prepareStartTime = 0;

        public void Init()
        {
            _controller = self.GetComponent<NPCAnimationController>();
            character = self.GetComponent<Character>();
            _arm = _controller.GetLeftArm();
            _weapon = character.BowRenderers[3].transform;
        }

        public void Update()
        {
            if (_controller.IsReady() && character.WeaponType == WeaponType.Bow)
            {
                var targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RotateArm(_arm, _weapon,  targetPos, -60, 60);
                _controller.Turn(Mathf.Sign((targetPos - self.transform.position).x));
            }
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
            _controller.UseBow();
            _controller.GetReady();

            skillState = 1;
            _controller.BowCharge(1);
            prepareStartTime = Time.time;
            targets = GetEnemiesInSightRange(self.transform, skill.SelectRange, 120f).Take(1).ToList();
        }

        public void endPrepare()
        {
            var charged = Time.time - prepareStartTime > 0.05f;
            _controller.BowCharge(charged ? 2 : 3);
            skillState = charged ? 2 : 0;
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
            var halfSightAngle = sightAngle / 2f;
            // 角色的朝向
            var direction = playerTransform.right;
            // 扇形范围内的半径
            var radius = sightRange / 2f;
            // 扇形范围内的圆心位置
            var center = playerTransform.position + direction * radius;

            // 获取扇形范围内的所有物体
            var colliders = Physics2D.OverlapCapsuleAll(center, new Vector2(sightRange, radius), CapsuleDirection2D.Vertical, 0f);
            var enemies = new List<GameObject>();

            // 遍历所有物体，判断是否为敌人，并在扇形范围内
            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    var enemyDirection = collider.transform.position - playerTransform.position;
                    var angle = Vector3.Angle(direction, enemyDirection);
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
        
        private void RotateArm(Transform arm, Transform weapon, Vector2 target, float angleMin, float angleMax) // TODO: Very hard to understand logic.
        {
            target = arm.transform.InverseTransformPoint(target);
            
            var angleToTarget = Vector2.SignedAngle(Vector2.right, target);
            var angleToArm = Vector2.SignedAngle(weapon.right, arm.transform.right) * Math.Sign(weapon.lossyScale.x);
            var fix = weapon.InverseTransformPoint(arm.transform.position).y / target.magnitude;

            if (fix < -1) fix = -1;
            else if (fix > 1) fix = 1;

            var angleFix = Mathf.Asin(fix) * Mathf.Rad2Deg;
            var angle = angleToTarget + angleFix + arm.transform.localEulerAngles.z;

            angle = NormalizeAngle(angle);

            if (angle > angleMax)
            {
                angle = angleMax;
            }
            else if (angle < angleMin)
            {
                angle = angleMin;
            }

            if (float.IsNaN(angle))
            {
                Debug.LogWarning(angle);
            }

            arm.transform.localEulerAngles = new Vector3(0, 0, angle + angleToArm);
        }

        private static float NormalizeAngle(float angle)
        {
            while (angle > 180) angle -= 360;
            while (angle < -180) angle += 360;

            return angle;
        }

    }
}