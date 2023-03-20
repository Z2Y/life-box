using System;
using System.Linq;
using DG.Tweening;
using HeroEditor.Common.Enums;
using UnityEngine;
using CharacterScripts = Assets.HeroEditor.Common.Scripts.CharacterScripts;

namespace Controller
{
    public class NPCAnimationController : MonoBehaviour, IMoveAnimator, IAttackAnimator
    {
        private CharacterScripts.Character character;
        private CharacterTrail trial;
        private Animator animator;
        private Transform aim;
        
        private static readonly int Ready = Animator.StringToHash("Ready");
        private static readonly int Charge = Animator.StringToHash("Charge");
        
        
        public Vector3 Speed { get; private set; }
        public bool Attacking { get; private set;  }
        public bool Sliding { get; private set; }

        private void Awake()
        {
            trial = gameObject.AddComponent<CharacterTrail>();
        }

        private void Start()
        {
            if (character == null)
            {
                character = gameObject.GetComponent<CharacterScripts.Character>();
                animator = character.Animator;
            }
            animator.SetBool(Ready, true);
        }
        
        private void Update()
        {
            if (!ReferenceEquals(aim, null))
            {
                Turn((aim.position - transform.position).x);
            }
        }

        public CharacterScripts.CharacterState GetState()
        {
            return character.GetState();
        }

        public void SetState(CharacterScripts.CharacterState state)
        {
            character.SetState(state);
        }
        
        public void SetAim(Transform aimTrans)
        {
            aim = aimTrans;
        }

        private void SetAttacking(bool value)
        {
            Attacking = value;
        }

        public void SetSpeed(Vector3 speed)
        {

            if (speed.x != 0 && ReferenceEquals(aim, null))
            {
                if (character.WeaponType != WeaponType.Bow)
                {
                    Turn(speed.x);
                }
                Speed = speed;
            }
            else
            {
                Speed = new Vector3(Mathf.Sign(character.transform.localScale.x) * 0.0001f, speed.y, speed.z);
            }

            if (speed != Vector3.zero)
            {
                character.SetState(character.WeaponType != WeaponType.Bow ? CharacterScripts.CharacterState.Run : CharacterScripts.CharacterState.Walk);
            }
            else
            {
                character.SetState(CharacterScripts.CharacterState.Idle);
            }
        }

        public async void Slide(float duration)
        {
            if (Sliding) return;
            var originState = character.GetState();
            var originSpeed = Speed;
            Sliding = true;
            trial.enabled = true;
            await YieldCoroutine.WaitForSeconds(duration);
            Sliding = false;
            trial.enabled = false;
            character.SetState(originState);
        }

        public void SetBodyScale(Vector2 bodyScale)
        {
            character.BodyScale = bodyScale;
        }

        public async void AttackNormal()
        {
            character.Slash();
            SetAttacking(true);
            await YieldCoroutine.WaitForSeconds(0.25f);
            SetAttacking(false);
        }

        public void GetReady()
        {
            character.GetReady();
        }

        public bool IsReady()
        {
            return character.IsReady();
        }

        public void BowCharge(int chargeState)
        {
            animator.SetInteger(Charge, chargeState);
        }

        public void Turn(float direction)
        {
            var oScale = character.transform.localScale;
            character.transform.localScale = new Vector3(Mathf.Sign(direction) * Math.Abs(oScale.x), oScale.y, 1);
        }

        public void UseBow()
        {
            if (character.Bow.Count <= 0)
            {
                character.Equip(character.SpriteCollection.Bow.First(), EquipmentPart.Bow);
            }
            else
            {
                character.WeaponType = WeaponType.Bow;
                character.Initialize();
            }
        }

        public void UseSword()
        {
            character.WeaponType = WeaponType.Melee1H;
            character.Initialize();
        }

        public Transform GetMeleeArm()
        {
            return character.MeleeWeapon.transform;
        }

        public Transform GetLeftArm()
        {
            return character.GetComponent<CharacterScripts.CharacterBodySculptor>().ArmL;
        }

        public void onHit(GameObject from)
        {
            animator.Play("Hit");
        }
    }
}