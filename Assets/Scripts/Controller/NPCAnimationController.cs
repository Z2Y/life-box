using System;
using DG.Tweening;
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

        private void Start()
        {
            if (character == null)
            {
                character = gameObject.GetComponent<CharacterScripts.Character>();
                trial = gameObject.AddComponent<CharacterTrail>();
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

        public void Slide(Vector3 target)
        {
            // character.rigidbody2D.Do
            if (Sliding) return;
            var originState = character.GetState();
            var originSpeed = Speed;
            Sliding = true;
            trial.enabled = true;
            var tween = character.transform.DOMove(target, 0.4f).OnComplete(() =>
            {
                character.SetState(originState);
                Sliding = false;
                trial.enabled = false;
            });
            tween.OnUpdate(() =>
            {
                Speed = Vector3.Lerp(originSpeed, Vector3.zero, tween.ElapsedPercentage());
            });
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