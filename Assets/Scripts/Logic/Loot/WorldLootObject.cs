using System;
using System.Threading.Tasks;
using Assets.HeroEditor.Common.Scripts.Common;
using DG.Tweening;
using UnityEngine;
using Utils;

namespace Logic.Loot
{
    [PrefabResource("Prefabs/vfx/Loot/DefaultLoot")]
    public class WorldLootObject : MonoBehaviour
    {
        private ItemStack items;

        private SpriteRenderer spriteRenderer;

        private static readonly PrefabPool<WorldLootObject> pool = new ();

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private async Task loadItem(ItemStack otherItem)
        {
            items = otherItem;
            var spritePath = items.item.WorldSprite ?? items.item.IconSprite;
            if (spritePath.IsEmpty()) return;
            var request = Resources.LoadAsync<Sprite>(spritePath);
            await YieldCoroutine.WaitForInstruction(request);

            if (request.isDone)
            {
                spriteRenderer.sprite = request.asset as Sprite;
            }
        }
        
        public void JumpOut(Vector3 position, Vector3 jumpDirection, float duration = 1.25f)
        {
            transform.position = position;
            transform.DOJump(position + jumpDirection, 0.3f, 1, 1f);
        }

        public static async Task<WorldLootObject> CreateAsync(ItemStack lootItems)
        {
            var lootObj = await pool.GetAsync();
            await lootObj.loadItem(lootItems);

            return lootObj;
        }

    }
}