using System.Linq;
using Assets.HeroEditor.Common.Scripts.Common;
using JetBrains.Annotations;
using UnityEngine;

namespace Logic.Battle.Realtime.Ai
{
    
    // Find Random Destination
    public class FindEnemy
    {
        private readonly float radius;
        private readonly int enemyLayer;

        public FindEnemy(float radius, int enemyLayer)
        {
            this.radius = radius;
            this.enemyLayer = enemyLayer;
        }

        public Collider2D GetResult(Vector3 position, [CanBeNull] Collider2D previous)
        {
            var targets = Physics2D.OverlapCircleAll(position, radius, enemyLayer);

            if (!ReferenceEquals(previous, null) && targets.Contains(previous))
            {
                return previous;
            }

            return targets.Random();

        }
    }
}