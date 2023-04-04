using System;
using UnityEngine;

namespace Controller
{
    public class AnimationController : MonoBehaviour, IMoveAnimator, IAttackAnimator
    {
        public Animator animator { get; private set; }
        public Vector3 Speed { get; private set; }
        public bool Attacking { get; private set;  }

        private Transform aim;

        private float defaultSign = 1f;
        private static readonly int Speed1 = Animator.StringToHash("Speed");

        private void Start()
        {
            animator = gameObject.GetComponentInChildren<Animator>();
            animator.Play("Idle");
            defaultSign = Mathf.Sign(transform.localScale.x);
        }

        private void OnDisable()
        {
            aim = null;
        }

        private void Update()
        {
            if (!ReferenceEquals(aim, null))
            {
                Turn((aim.position - transform.position).x);
            }
        }

        public void SetAttacking(bool value)
        {
            Attacking = value;
        }

        public void SetAim(Transform aimTrans)
        {
            aim = aimTrans;
        }

        public void SetSpeed(Vector3 speed)
        {
            // animator.SetFloat(Speed, speed);

            if (speed.x != 0 && ReferenceEquals(aim, null))
            {
                Turn(speed.x);
            }
            Speed = speed;

            if (Attacking)
            {
                return;
            }

            if (speed != Vector3.zero)
            {
                animator.SetFloat(Speed1, speed.magnitude);
            }
            else
            {
                animator.SetFloat(Speed1, 0f);
            }
        }

        public void Play(string state)
        {
            animator.Play(state);
        }
        

        public void AttackNormal()
        {
            animator.Play("Attack");
        }

        public void Turn(float direction)
        {
            var oScale = transform.localScale;
            transform.localScale = new Vector3(Mathf.Sign(direction) * defaultSign * Math.Abs(oScale.x), oScale.y, 1);
        }
    }

    public interface IMoveAnimator
    {
        public void SetSpeed(Vector3 speed);
        public void Turn(float direction);
        public void SetAim(Transform aim);
    }

    public interface IAttackAnimator
    {
        public void AttackNormal();
    }
}