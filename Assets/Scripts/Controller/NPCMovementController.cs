using System;
using System.Threading.Tasks;
using UnityEngine;
using Utils;

namespace Controller
{
    public class NPCMovementController : MonoBehaviour
    {
        [SerializeField] private Vector3 speed;
        [SerializeField] private NPCAnimationController animator;
        [SerializeField] private bool isPlayer;
        [SerializeField] private bool fromJoystick;
        private Rigidbody2D rgBody;

        private readonly NPCMoveTask moveTask = new ();

        private void Awake()
        {
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
            updatePositionBySpeed();
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

        public async Task MoveTo(Vector3 target, Vector3 moveSpeed, long expectTime)
        {
            moveTask.Cancel(); // cancel previous move task;

            try
            {
                await moveTask.DoMove(target, moveSpeed, expectTime);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Move To Canceled");
            }
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
        
        private TaskCompletionSource<bool> tcs;
        private Vector3 moveSpeed;

        private bool isComplete;
        public bool IsRunning => tcs != null && !isComplete;

        public Task DoMove(Vector3 target, Vector3 speed, long expectTime)
        {
            targetPos = target;
            startPos = rigidbody.position;
            startTime = TimeHelper.Now();
            isComplete = false;

            var distance = (target - startPos).magnitude;
            if (distance < 0.001f)
            {
                isComplete = true;
                return Task.CompletedTask; // No need movement
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

            tcs = new TaskCompletionSource<bool>();

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