using System;
using UnityEngine;

namespace Controller
{
    public class NPCAnimationController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        
        private static readonly int Speed = Animator.StringToHash("speed");

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
        }

        public void SetSpeed(float speed)
        {
            animator.SetFloat(Speed, speed);
        }
    }
}