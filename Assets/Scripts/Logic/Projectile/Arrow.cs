using System.Collections;
using System.Threading.Tasks;
using Logic.Enemy;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Logic.Projectile
{
    [PrefabResource("Prefabs/Projectile/Arrow")]
    public class Arrow : MonoBehaviour
    {
        private static readonly PrefabPool<Arrow> Pool = new();
        
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

        public void OnDisable()
        {
            _hitArea.RemoveAllListener();
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

        public void Fire(Transform from, Sprite arrowSprite, Vector3 velocity, float maxAlive = 5f, UnityAction<Collider2D> onHit = null)
        {
            transform.SetParent(from);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.SetParent(null);
            SpRender.sprite = arrowSprite;
            Rigidbody.velocity = velocity;
            transform.right = velocity.normalized;
            _hitArea.AddListener(Bang);
            if (onHit != null)
            {
                _hitArea.AddListener(onHit);
            }
            
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