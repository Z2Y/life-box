using UnityEngine;

namespace Logic.Detector.Scriptable
{

    [CreateAssetMenu(menuName = "CollisionDetector/ScriptableAttackDetector", order = 4)]
    public class ScriptableAttackDetector : ScriptableDetector<NormalAttackDetector>
    {
        [SerializeField]
        public string enemyTag;
        
        public override IDetector GetDetector()
        {
            var detector = base.GetDetector() as NormalAttackDetector;
            detector?.SetTag(enemyTag);
            return detector;
        }
    }
}