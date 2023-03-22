using UnityEngine;

namespace Logic.Detector.Scriptable
{
    [CreateAssetMenu(menuName = "CollisionDetector/ScriptableWallDetector", order = 5)]
    public class ScriptableWallDetector : ScriptableDetector<WallDetector> {}
}