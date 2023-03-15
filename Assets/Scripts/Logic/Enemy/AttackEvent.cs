using UnityEngine;
using UnityEngine.Serialization;

namespace Logic.Enemy
{
    public class AttackEvent : MonoBehaviour
    {
        public HitArea[] areas;

        public void onBeginHitDetect()
        {
            foreach (var hitArea in areas)
            {
                hitArea.enabled = true;
            }
        }

        public void onEndHitDetect()
        {
            foreach (var hitArea in areas)
            {
                hitArea.enabled = false;
            }
        }
    }
}