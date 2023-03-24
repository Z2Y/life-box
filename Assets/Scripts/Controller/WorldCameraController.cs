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

        private const float maxMoveSpeed = 0.2f;

        private void Awake()
        {
            Instance = this;
        }

        public void JumpToFollowPos()
        {
            if (!isFollowing) return;
            transform.position = tarGetPosition();
        }

        public bool isNearFollowTarget()
        {
            var targetPos = tarGetPosition();
            var offset = targetPos - transform.position;
            return offset.magnitude < maxMoveSpeed;
        }

        public async Task FollowTo(GameObject other, bool moveSmoothly = true, float duration = 0.5f)
        {
            _followGameObject = other;
            isFollowing = false;
            
            if (other == null)
            {
                return;
            }
            
            if (moveSmoothly)
            {
                var tween = transform.DOMove(tarGetPosition(), duration);

                await YieldCoroutine.WaitForInstruction(tween.WaitForCompletion());
            }
            else
            {
                transform.position = tarGetPosition();
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

        private void FixedUpdate()
        {
            if (!isFollowing) return;
            var targetPos = tarGetPosition();
            var offset = targetPos - transform.position;

            if (offset.magnitude >= maxMoveSpeed)
            {
                transform.position += offset.normalized * maxMoveSpeed;
            }
            else
            {
                transform.position = targetPos;
            }
        }
    }
}