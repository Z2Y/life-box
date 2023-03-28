using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Logic.Map;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Controller
{
    public class NPCMovementController : MonoBehaviour, IRouteResponder
    {
        [SerializeField] private Vector3 speed;
        [SerializeField] private NPCAnimationController animator;
        [SerializeField] private bool isPlayer;
        [SerializeField] private bool fromJoystick;
        private Rigidbody2D rgBody;

        private NPCController mainController;
        private readonly NPCMoveTask moveTask = new ();
        private UnityAction<long> _onEnterPlace;
        private UnityAction<long> _onLeavePlace;

        private void Awake()
        {
            mainController = GetComponent<NPCController>();
            animator = GetComponent<NPCAnimationController>();
            rgBody = GetComponent<Rigidbody2D>();
            
            moveTask.rigidbody = rgBody;
            moveTask.animator = animator;
        }

        private void Update()
        {
            if (isPlayer)
            {
                updateSpeedFromUserInput();
            }
            else
            {
                moveTask.Update();
            }
        }

        private void FixedUpdate()
        {
            if (isPlayer)
            {
                updatePositionBySpeed();
            }
        }

        public void SetAsPlayer(bool player = true)
        {
            isPlayer = player;
        }

        public void SetSpeed(Vector3 value)
        {
            speed = value;
        }

        private void updatePositionBySpeed()
        {
            if (speed.magnitude <= 0.001f)
            {
                rgBody.velocity = Vector2.zero;
                return;
            }

            // var npcTransform = transform;
            // npcTransform.position += speed * Time.deltaTime;
            rgBody.velocity = speed;
        }
        
        public void AddEnterPlaceListener(UnityAction<long> onEnter)
        {
            
            _onEnterPlace += onEnter;
        }

        public void OffEnterPlaceListener(UnityAction<long> onEnter)
        {
            _onEnterPlace -= onEnter;
        }
        
        public void AddLeavePlaceListener(UnityAction<long> onLeave)
        {
            _onLeavePlace += onLeave;
        }

        public void OffLeavePlaceListener(UnityAction<long> onLeave)
        {
            _onLeavePlace -= onLeave;
        }

        private void updateSpeedFromUserInput()
        {
            if (animator.Sliding) return;
            
            var speedX = Input.GetAxisRaw("Horizontal");
            var speedY = Input.GetAxisRaw("Vertical");
            var input = new Vector3(speedX, speedY, 0).normalized * 2f;

            if (input != speed && Vector3.Distance(input, speed) > 0.001f)
            {
                speed.Set(input.x, input.y, input.z);
                speed *= 2;
                animator.SetSpeed(speed);
            }
        }

        public async UniTask MoveTo(Vector3 target, Vector3 moveSpeed, long expectTime)
        {
            moveTask.Cancel(); // cancel previous move task;
            
            await moveTask.DoMove(target, moveSpeed, expectTime);
        }

        public void OnEnter(long mapID, long placeID)
        {
            _onEnterPlace?.Invoke(placeID);
        }

        public void OnLeave(long mapID, long placeID)
        {
            _onLeavePlace?.Invoke(placeID);
        }
    }

    public class NPCMoveTask
    {
        public Rigidbody2D rigidbody;
        public IMoveAnimator animator;

        private Vector3 targetPos;
        private Vector3 startPos;
        private long startTime;
        private long moveTime;
        
        private UniTaskCompletionSource<bool> tcs;
        private Vector3 moveSpeed;

        private bool isComplete;
        public bool IsRunning => tcs != null && !isComplete;

        public UniTask DoMove(Vector3 target, Vector3 speed, long expectTime)
        {
            targetPos = target;
            startPos = rigidbody.position;
            startTime = TimeHelper.Now();
            isComplete = false;

            var distance = (target - startPos).magnitude;
            if (distance < 0.001f)
            {
                isComplete = true;
                return UniTask.CompletedTask; // No need movement
            }

            moveSpeed = (targetPos - startPos).normalized * speed.magnitude;

            moveTime = (long)(distance / speed.magnitude * 1000);
            
            if (!ReferenceEquals(animator, null))
            {
                animator.SetSpeed(moveSpeed);
            }

            if (expectTime > 0 && moveTime > expectTime)
            {
                moveTime = expectTime;
            }

            tcs = new UniTaskCompletionSource<bool>();

            return tcs.Task;
        }

        public void Update()
        {
            if (isComplete || moveSpeed.magnitude <= 0.001f)
            {
                return;
            }

            if (TimeHelper.Now() - startTime >= moveTime)
            {
                Complete();
                return;
            }

            rigidbody.velocity = moveSpeed;
        }

        private void Complete()
        {
            tcs?.TrySetResult(true);
            if (!ReferenceEquals(animator, null))
            {
                animator.SetSpeed(Vector3.zero);
            }

            rigidbody.velocity = Vector2.zero;
            isComplete = true;
        }

        public void Cancel()
        {
            tcs?.TrySetCanceled();
            if (!ReferenceEquals(animator, null))
            {
                animator.SetSpeed(Vector3.zero);
            }
            rigidbody.velocity = Vector2.zero;
            isComplete = true;
        }
    }
}