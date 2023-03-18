using Battle.Realtime.Ai;
using Controller;
using Model;
using UnityEngine;

namespace Battle.Realtime
{
    public class SlideSkillAction : ISkillAction
    {
        public Skill skill;
        public GameObject self;
        private NPCAnimationController animator;
        
        public void Init()
        {
            animator = self.GetComponent<NPCAnimationController>();
        }
        public bool isIdle()
        {
            return !animator.Sliding && !animator.Attacking;
        }

        public bool isReady()
        {
            return isIdle();
        }

        public bool isPreparing()
        {
            return false;
        }

        public void DoSkill()
        {
            var speedX = Input.GetAxisRaw("Horizontal");
            var speedY = Input.GetAxisRaw("Vertical");
            var input = new Vector3(speedX, speedY, 0).normalized * 2f;

            if (input.magnitude < 0.0001f)
            {
                return;
            }
            
            animator.SetSpeed(input * (2f / 0.5f));
            animator.Slide(self.transform.position + input * 2f);
        }

        public void endPrepare()
        {
            // no need prepare
        }

        public void prepare()
        {
            // no need prepare
        }
    }
}