using System;
using System.Collections;
using DG.Tweening;
using Logic.Detector;
using Logic.Enemy;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Controller
{
    [PrefabResourceWithArgs("Prefabs/vfx/sword/{0}")]
    public class SwordSlashController : MonoBehaviour, IOnPrefabLoaded<string>
    {
        private ParticleSystem particles;
        private CollisionDetector collisionDetector;
        private NormalAttackDetector normalAttackDetector;

        public static readonly PrefabPool<SwordSlashController, string> Pool = new();
        private UnityAction<IHitResponder, Collider2D> hitCallback;
        private Transform following;
        private Vector3 followingOffset;

        public string swordType;

        private void Awake()
        {
            particles = GetComponent<ParticleSystem>();
            collisionDetector = gameObject.AddComponent<CollisionDetector>();
            collisionDetector.enabled = false;
            normalAttackDetector = new NormalAttackDetector();
            collisionDetector.AddDetector(normalAttackDetector);
        }

        private void Update()
        {
            if (!ReferenceEquals(following, null))
            {
                followParent();
            }
        }

        private void prepareCollision(string enemyTag, UnityAction<IHitResponder, Collider2D> callback)
        {
            collisionDetector.enabled = true;
            hitCallback = callback;
            normalAttackDetector.SetTag(enemyTag);
            normalAttackDetector.onDetect(onHit);
        }

        private void endCollision()
        {
            collisionDetector.enabled = false;
            hitCallback = null;
            normalAttackDetector.offDetect(onHit);
        }

        private void onHit(IDetector _, Collider2D target)
        {
            hitCallback?.Invoke(target.GetComponent<IHitResponder>(), target);
        }

        public async void Emit(Transform parent, Vector3 offset, Vector3 speed, SwordHitConfig hitConfig)
        {
            Turn(speed.x);
            particles.Emit(1);
            var main = particles.main;
            var sign = Mathf.Sign(speed.x);
            offset.x *= -1;
            var duration = main.startLifetime.constant;
            // var originRotation = transform.rotation;
            // var originParent = transform.parent;
            following = parent;
            followingOffset = offset;
            
            transform.rotation = Quaternion.AngleAxis(
                Vector3.SignedAngle(sign > 0 ? Vector3.right : Vector3.left , speed, Vector3.forward), Vector3.forward);

            // await YieldCoroutine.WaitForSeconds(Mathf.Max(main.startDelay.constant, hitConfig.startHitDelay));
            prepareCollision(hitConfig.enemyTag, hitConfig.onHit);
            await YieldCoroutine.WaitForSeconds(hitConfig.hitDuration);
            endCollision();
            await YieldCoroutine.WaitForSeconds(duration);
            // transform.rotation = originRotation;
            following = null;
            Pool.Return(this, swordType);
        }

        private void followParent()
        {
            transform.position = following.position + followingOffset;
        }

        private void Turn(float direction)
        {
            var oScale = transform.localScale;
            transform.localScale = new Vector3(Mathf.Sign(direction) * Math.Abs(oScale.x), oScale.y, 1);
        }

        public void OnLoaded(string arg)
        {
            swordType = arg;
        }
    }

    public struct SwordHitConfig
    {
        public string enemyTag;
        public float startHitDelay;
        public float hitDuration;
        public UnityAction<IHitResponder, Collider2D> onHit;
    }
}