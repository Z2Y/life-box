using UnityEngine;

namespace Logic.Detector.Scriptable
{
    [CreateAssetMenu(menuName = "CollisionDetector/ScriptableNPCDetector", order = 1)]
    public class ScriptableNPCDetector : ScriptableDetector<NPCDetector> {}

    [CreateAssetMenu(menuName = "CollisionDetector/ScriptableNPCTalkableDetector", order = 2)]
    public class ScriptableNPCTalkableDetector : ScriptableDetector<TalkableNPCDetector> {}


}