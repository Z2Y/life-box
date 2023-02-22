using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Controller
{
    public class WorldCameraController : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;

        private GameObject _followGameObject;
        private bool isFollowing;

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
                await YieldCoroutine.WaitForInstruction(worldCamera.gameObject.transform.DOMove(other.transform.position, 0.5f).WaitForCompletion());
            }
            else
            {
                worldCamera.gameObject.transform.position = other.transform.position;
            }

            isFollowing = true;
        }

        private void LateUpdate()
        {
            if (!isFollowing) return;
            worldCamera.gameObject.transform.position = _followGameObject.transform.position;
        }
    }
}