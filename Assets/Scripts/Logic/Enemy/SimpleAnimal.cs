using Battle.Realtime.Ai;
using Controller;
using DG.Tweening;
using Logic.Loot;
using ModelContainer;
using UnityEngine;
using Utils;

namespace Logic.Enemy
{
    [PrefabResourceWithArgs("Prefabs/Animal/{0}/Main")]
    public class SimpleAnimal : MonoBehaviour, IOnPrefabLoaded<string>, IHitResponder
    {
        public string animalType;
        public float animalHeight;
        private AnimationController animator;
        private SimpleAnimalBornPlace bornPlace;
        private SimpleAI ai;
        private PropertyValue hp;
        private SimpleAttackInfo info;

        public bool isDeath;

        private void Start()
        {
            ai = GetComponent<SimpleAI>();
            animator = GetComponent<AnimationController>();
            hp = new PropertyValue(SubPropertyType.HitPoint, 20,null);
            ai.StartAI();
        }

        public void SetBornPlace(SimpleAnimalBornPlace place)
        {
            bornPlace = place;
        }

        public void OnLoaded(string arg)
        {
            animalType = arg;
            gameObject.name = arg;
            isDeath = false;
            enabled = true;
            hp?.Change(20);
            ai?.StartAI();
            info?.UpdateHp(hp);
        }

        public async void onHit(GameObject from)
        {
            if (isDeath) return;
            var position = transform.position;
            var direction = (position - from.transform.position).normalized;
            animator.Play("Hurt");
            transform.DOMove(position + direction * 0.2f, 0.25f).SetDelay(0.15f);

            if (ReferenceEquals(info, null))
            {
                info = await SimpleAttackInfo.Show();
                bindAttackInfo();
            }
            hp.value -= 1;
            if (hp.value <= 0)
            {
                onDeath();
            }
            
            info.ShowAttackInfo("-1", direction);
            info.UpdateHp(hp);
        }

        private void bindAttackInfo()
        {
            Transform transform1;
            (transform1 = info.transform).SetParent(transform, false);
            transform1.localPosition = new Vector3(0, animalHeight, 0);
            //  info.transform.localScale = Vector3.one * info.transform.lossyScale.x;
        }

        private async void onDeath()
        {
            isDeath = true;
            animator.Play("Death");
            ai.StopAI();
            enabled = false;
            
            await YieldCoroutine.WaitForSeconds(0.7f);
            
            var lootItem = new ItemStack();
            lootItem.StoreItem(ItemCollection.Instance.GetItem(10016), 1);
            var lootObj = await WorldLootObject.CreateAsync(lootItem);
            lootObj.JumpOut(transform.position, UnityEngine.Random.insideUnitCircle);

            await YieldCoroutine.WaitForSeconds(0.5f);
            bornPlace.OnAnimalDeath(this);
            

        }
    }
}