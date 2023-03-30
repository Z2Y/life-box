using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Utils;

namespace Logic.Loot
{
    [PrefabResource("Prefabs/vfx/Loot/DefaultLoot")]
    public class WorldLootObject : MonoBehaviour, ILootItem
    {
        private ItemStack items;

        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private bool isAutoLoot;

        public static readonly PrefabPool<WorldLootObject> Pool = new ();

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private async UniTask loadItem(ItemStack otherItem)
        {
            items = otherItem;
            var spritePath = items.item.WorldSprite ?? items.item.IconSprite;
            if (string.IsNullOrEmpty(spritePath)) return;
            var request = Resources.LoadAsync<Sprite>(spritePath);
            await request;

            if (request.isDone)
            {
                spriteRenderer.sprite = request.asset as Sprite;
            }
        }
        
        public void JumpOut(Vector3 position, Vector3 jumpDirection, float duration = 1f)
        {
            spriteRenderer.DOFade(1, 0.5f);
            transform.position = position;
            transform.DOJump(position + jumpDirection, 0.3f, 1, duration);
        }

        public static async UniTask<WorldLootObject> CreateAsync(ItemStack lootItems)
        {
            var lootObj = await Pool.GetAsync();
            await lootObj.loadItem(lootItems);

            return lootObj;
        }

        public bool IsAutoLoot()
        {
            return isAutoLoot;
        }

        public ItemStack GetLootItemStack()
        {
            return items;
        }

        public void OnLoot(GameObject by)
        {
            transform.DOMove(by.transform.position, 0.5f);
            spriteRenderer.DOFade(0, 0.5f).OnComplete(() =>
            {
                Pool.Return(this);
            });
        }
    }
}