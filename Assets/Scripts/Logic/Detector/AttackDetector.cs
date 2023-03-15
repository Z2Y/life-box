using Assets.HeroEditor.Common.Scripts.Common;
using Logic.Enemy;
using UnityEngine;

namespace Logic.Detector
{
    public class NormalAttackDetector : BaseDetector
    {
        private string targetTag;

        public NormalAttackDetector() : base("NormalAttackDetector") {}
        
        public NormalAttackDetector(string tag) : base("NormalAttackDetector")
        {
            targetTag = tag;
        }

        public void SetTag(string tag)
        {
            targetTag = tag;
        }

        protected override bool isTarget(Collider2D collision)
        {
            Debug.Log($"Check is Attack Target {collision.gameObject.name} {targetTag} {collision.CompareTag(targetTag)}");
            if (!targetTag.IsEmpty() &&  !collision.CompareTag(targetTag))
            {
                return false;
            }

            var hitResponder = collision.GetComponent<IHitResponder>();
            return !ReferenceEquals(hitResponder, null);
        }
    }
}