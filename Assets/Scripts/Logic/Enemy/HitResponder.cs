using UnityEngine;

namespace Logic.Enemy
{
    public interface IHitResponder
    {
        public void onHit(GameObject from);
    }
}