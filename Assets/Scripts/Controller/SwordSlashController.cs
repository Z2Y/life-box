using System;
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
            transform.SetParent(parent);
            transform.localPosition = offset;
           
            var liftTime = particles.main.startLifetime;
            await YieldCoroutine.WaitForSeconds(liftTime.constant);
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