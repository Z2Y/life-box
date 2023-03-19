using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Logic.Enemy;
using UnityEngine;
using Utils;

namespace Logic.Projectile
{
    [PrefabResource("Prefabs/Projectile/Arrow")]
    public class Arrow : MonoBehaviour
    {
        public static readonly PrefabPool<Arrow> Pool = new();
        
        public SpriteRenderer SpRender;
        public GameObject Trail;
        public GameObject Impact;
        public Rigidbody2D Rigidbody;

        private HitArea _hitArea;

        public void Awake()
        {
            _hitArea = GetComponent<HitArea>();
            SpRender = GetComponent<SpriteRenderer>();
        }

        public void OnEnable()
        {
            _hitArea.AddListener(Bang);
        }

        public void OnDisable()
        {
            _hitArea.RemoveListener(Bang);
            foreach (var tr in Trail.GetComponents<TrailRenderer>())
            {
                tr.Clear();
            }
            StopAllCoroutines();
        }

        public void SetEnemyTag(string tagName)
        {
            _hitArea.SetEnemyTag(tagName);
        }

        public async void Fire(Transform from, Sprite arrowSprite, Vector3 velocity, float maxAlive = 5f)
        {
            gameObject.SetActive(true);
            transform.SetParent(from);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.SetParent(null);
            SpRender.sprite = arrowSprite;
            Rigidbody.velocity = velocity;
            transform.right = velocity.normalized;
            StartCoroutine(autoRecycle(maxAlive));
        }

        private IEnumerator autoRecycle(float maxAlive)
        {
            yield return new WaitForSeconds(maxAlive);
            Pool.Return(this);
        }

        private void Bang(Collider2D other)
        {
            Pool.Return(this);
        }

        public static async Task<Arrow> CreateAsync()
        {
            var arrow = await Pool.GetAsync();

            return arrow;
        }
    }
}