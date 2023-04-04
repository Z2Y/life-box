using Assets.HeroEditor.Common.Scripts.Common;
using Logic.Loot;
using UnityEngine;

namespace Logic.Detector
{
    public class LootItemDetector : BaseDetector
    {
        private string targetTag;

        public LootItemDetector() : base("LootItemDetector") {}

        protected override bool isTarget(Collider2D collision)
        {
            if (!targetTag.IsEmpty() && !collision.CompareTag(targetTag))
            {
                return false;
            }

            var lootItem = collision.GetComponent<ILootItem>();
            return !ReferenceEquals(lootItem, null);
        }
    }
}