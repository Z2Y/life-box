using System;
using DG.Tweening;
using UnityEngine;
using CharacterScripts = Assets.HeroEditor.Common.Scripts.CharacterScripts;

namespace Controller
{
    public class NPCAnimationController : MonoBehaviour
    {
        private CharacterScripts.Character character;
        private Animator animator;
        
        private static readonly int Ready = Animator.StringToHash("Ready");
        private static readonly int Charge = Animator.StringToHash("Charge");

        public Vector3 Speed { get; private set; }

        private void Start()
        {
            if (character == null)
            {
                character = gameObject.GetComponent<CharacterScripts.Character>();
                animator = character.Animator;
            }
            animator.SetBool(Ready, true);
        }

        public void SetSpeed(Vector3 speed)
        {
            // animator.SetFloat(Speed, speed);
            
            if (character.GetState() == CharacterScripts.CharacterState.Jump)
            {
                return;
            }
            
            if (speed.x != 0)
            {
                Turn(speed.x);
                Speed = speed;
            }

            if (speed != Vector3.zero)
            {
                character.SetState(CharacterScripts.CharacterState.Walk);
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

        private void Turn(float direction)
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