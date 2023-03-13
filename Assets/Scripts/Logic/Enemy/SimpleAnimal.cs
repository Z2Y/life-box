using Battle.Realtime.Ai;
using Controller;
using DG.Tweening;
using UnityEngine;
using Utils;

namespace Logic.Enemy
{
    [PrefabResourceWithArgs("Prefabs/Animal/{0}/Main")]
    public class SimpleAnimal : MonoBehaviour, IOnPrefabLoaded<string>, IHitResponder
    {
        public string animalType;
        public float animalHeight = 0.4f;
        private AnimationController animator;
        private SimpleAnimalBornPlace bornPlace;
        private SimpleAI ai;
        private PropertyValue hp;
        private SimpleAttackInfo info;

        private bool isDeath;
        private void Awake()
        {
            ai = GetComponent<SimpleAI>();
            animator = GetComponent<AnimationController>();
            hp = new PropertyValue(SubPropertyType.HitPoint, 20,null);
        }

        private void Start()
        {
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
            hp.Change(20);
            info?.UpdateHp(hp);
        }

        public async void onHit(GameObject from)
        {
            if (isDeath) return;
            var position = transform.position;
            var direction = (position - from.transform.position).normalized;
            animator.Play("Hurt");
            transform.DOMove(position + direction * 0.25f, 0.25f).SetDelay(0.15f);

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
            info.transform.SetParent(transform, false);
            info.transform.localPosition = new Vector3(0, -animalHeight, 0);
           //  info.transform.localScale = Vector3.one * info.transform.lossyScale.x;
            info.transform.position = transform.position;
        }

        private async void onDeath()
        {
            isDeath = true;
            animator.Play("Death");
            await YieldCoroutine.WaitForSeconds(0.3f);
            bornPlace.OnAnimalDeath(this);
        }
    }
}