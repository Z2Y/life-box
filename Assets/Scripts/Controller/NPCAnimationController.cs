using System;
using DG.Tweening;
using UnityEngine;
using CharacterScripts = Assets.HeroEditor.Common.Scripts.CharacterScripts;

namespace Controller
{
    public class NPCAnimationController : MonoBehaviour, IMoveAnimator, IAttackAnimator
    {
        private CharacterScripts.Character character;
        private Animator animator;
        private Transform aim;
        
        private static readonly int Ready = Animator.StringToHash("Ready");
        private static readonly int Charge = Animator.StringToHash("Charge");

        public Vector3 Speed { get; private set; }
        public bool Attacking { get; private set;  }

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

        public void SetAttacking(bool value)
        {
            Attacking = value;
        }

        public void SetSpeed(Vector3 speed)
        {
            // animator.SetFloat(Speed, speed);
            
            if (character.GetState() == CharacterScripts.CharacterState.Jump)
            {
                return;
            }

            if (speed.x != 0 && ReferenceEquals(aim, null))
            {
                Turn(speed.x);
                Speed = speed;
            }
            else
            {
                Speed = new Vector3(Mathf.Sign(character.transform.localScale.x) * 0.0001f, speed.y, speed.z);
            }

            if (Attacking)
            {
                return;
            }

            if (speed != Vector3.zero)
            {
                character.SetState(CharacterScripts.CharacterState.Run);
            }
            else
            {
                character.SetState(CharacterScripts.CharacterState.Idle);
            }
        }

        public void Jump(Vector3 target)
        {
            // character.rigidbody2D.Do
            if (character.GetState() == CharacterScripts.CharacterState.Jump) return;
            character.SetState(CharacterScripts.CharacterState.Jump);
            character.transform.DOJump(target, 1f, 1, 0.5f).OnComplete(() =>
            {
                character.SetState(CharacterScripts.CharacterState.Idle);
            });
        }

        public void SetBodyScale(Vector2 bodyScale)
        {
            character.BodyScale = bodyScale;
        }

        public void AttackNormal()
        {
            character.Slash();
        }

        public void GetReady()
        {
            character.GetReady();
        }

        public void BowCharge(int chargeState)
        {
            animator.SetInteger(Charge, 1);
        }

        public void Turn(float direction)
        {
            var oScale = character.transform.localScale;
            character.transform.localScale = new Vector3(Mathf.Sign(direction) * Math.Abs(oScale.x), oScale.y, 1);
        }

        public Transform GetMeleeArm()
        {
            return character.MeleeWeapon.transform;
        }
    }
}