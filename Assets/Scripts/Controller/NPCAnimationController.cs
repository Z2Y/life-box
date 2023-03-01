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
        
        private static readonly int Speed = Animator.StringToHash("speed");
        private static readonly int Ready = Animator.StringToHash("Ready");

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

            if (speed.x != 0)
            {
                Turn(speed.x);
            }

            if (character.GetState() == CharacterScripts.CharacterState.Jump)
            {
                return;
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
            character.SetState(CharacterScripts.CharacterState.Jump);
            character.transform.DOJump(target, 1f, 1, 0.5f).OnComplete(() =>
            {
                character.SetState(CharacterScripts.CharacterState.Idle);
            });
        }

        public void AttackNormal()
        {
            character.Slash();
        }

        public void GetReady()
        {
            character.GetReady();
        }

        private void Turn(float direction)
        {
            character.transform.localScale = new Vector3(Mathf.Sign(direction), 1, 1);
        }
    }
}