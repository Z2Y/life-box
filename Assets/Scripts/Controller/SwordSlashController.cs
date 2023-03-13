using System;
using DG.Tweening;
using UnityEngine;
using Utils;

namespace Controller
{
    [PrefabResourceWithArgs("Prefabs/vfx/sword/{0}")]
    public class SwordSlashController : MonoBehaviour, IOnPrefabLoaded<string>
    {
        private ParticleSystem particles;

        public static readonly PrefabPool<SwordSlashController, string> Pool = new();

        public string swordType;

        private void Awake()
        {
            particles = GetComponent<ParticleSystem>();
            
        }

        public async void Emit(Transform parent, Vector3 offset, Vector3 speed)
        {
            Turn(speed.x);
            particles.Emit(1);
            // offset.x *= Mathf.Sign(speed.x);
            var main = particles.main;
            var sign = Mathf.Sign(speed.x);
            var duration = main.startLifetime.constant + main.startDelay.constant;
            var originRotation = transform.rotation;
            transform.position = parent.position + offset;
            transform.rotation = Quaternion.AngleAxis(
                Vector3.SignedAngle(sign > 0 ? Vector3.right : Vector3.left , speed, Vector3.forward), Vector3.forward);
            
            await YieldCoroutine.WaitForInstruction(transform.DOMove(transform.position + (speed + 0.05f * speed.normalized)  * main.startLifetime.constant, duration).WaitForCompletion());
            transform.rotation = originRotation;
            Pool.Return(this, swordType);
        }
        
        private void Turn(float direction)
        {
            var oScale = transform.localScale;
            transform.localScale = new Vector3(Mathf.Sign(direction) * Math.Abs(oScale.x), oScale.y, 1);
            // transform.localPosition *= Mathf.Sign(direction);
        }

        public void OnLoaded(string arg)
        {
            swordType = arg;
        }
    }
}