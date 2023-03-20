using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Controller
{
    public class WorldCameraController : MonoBehaviour
    {

        private GameObject _followGameObject;
        
        [SerializeField]
        private bool isFollowing;
        
        [SerializeField]
        private Bounds worldBounds = new (Vector3.zero, Vector3.positiveInfinity);
        
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
            var cameraPosition = transform.position;

            targetPosition.x = Mathf.Clamp(targetPosition.x, worldBounds.min.x, worldBounds.max.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, worldBounds.min.y, worldBounds.max.y);
            targetPosition.z = cameraPosition.z;
            return targetPosition;
        }

        public void UpdateWorldBound(Vector3 center, Vector3 size)
        {
            worldBounds = new Bounds(center, size);
        }

        private void LateUpdate()
        {
            if (!isFollowing) return;
            transform.position = tarGetPosition();
        }
    }
}