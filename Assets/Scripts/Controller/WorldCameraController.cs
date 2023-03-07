using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Controller
{
    public class WorldCameraController : MonoBehaviour
    {

        private GameObject _followGameObject;
        private bool isFollowing;
        
        public static WorldCameraController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public async Task FollowTo(GameObject other, bool moveSmoothly = true)
        {
            _followGameObject = other;

            if (other == null)
            {
                isFollowing = false;
                return;
            }
            
            if (moveSmoothly)
            {
                await YieldCoroutine.WaitForInstruction(transform.DOMove(tarGetPosition(), 0.5f).WaitForCompletion());
            }
            else
            {
                transform.position = other.transform.position;
            }

            isFollowing = true;
        }

        private Vector3 tarGetPosition()
        {
            var targetPosition = _followGameObject.transform.position;
            var self = transform;
            targetPosition.z = self.position.z;
            return targetPosition;
        }

        private void LateUpdate()
        {
            if (!isFollowing) return;
            transform.position = tarGetPosition();
        }
    }
}