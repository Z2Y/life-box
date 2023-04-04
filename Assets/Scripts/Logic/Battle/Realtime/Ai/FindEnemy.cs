using Assets.HeroEditor.Common.Scripts.Common;
using StructLinq;
using UnityEngine;

namespace Logic.Battle.Realtime.Ai
{
    
    // Find Random Destination
    public class FindEnemy
    {
        private readonly float radius;
        private readonly int enemyLayer;
        private readonly string enemyTag;
        private readonly Collider2D[] results;

        public FindEnemy(float radius, int enemyLayer, string enemyTag = "")
        {
            this.radius = radius;
            this.enemyLayer = enemyLayer;
            this.enemyTag = enemyTag;
            results = new Collider2D[32];
        }

        public Collider2D GetResult(Vector3 position,  Collider2D previous)
        {
            var size = Physics2D.OverlapCircleNonAlloc(position, radius, results, enemyLayer);

            if (size == 0) return null;

            if (!ReferenceEquals(previous, null) && results.ToStructEnumerable().Take(size, x => x).Contains(previous, x => x))
            {
                return previous;
            }

            if (enemyTag.IsEmpty())
            {
                return results[Random.Range(0, size)];
            }

            return results.ToStructEnumerable().Where((collider) => collider.CompareTag(enemyTag), x => x).FirstOrDefault(x => x);
        }
    }
}