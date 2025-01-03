using System.Collections;
using Core;
using UnityEngine;

namespace Entities.Enemy
{
    public class MinkController : MonoBehaviour
    {
        [SerializeField] private float changeDirectionInterval = 3f;
        public Transform player;
        public float attackRange = 2f;
        public float detectionRange { get; private set; } = 30f;
        public float accelerateSpeed;
        public float chaseSpeed;
        public float waitTime = 2f;
        public string currentAnimation = "";
        private Vector3 moveDirection;
        private float nextDirectionChangeTime;
        private Mink mink;
        private Animator animator;
        private Rigidbody rb;
        public enum EnemyState
        {
            Idle,
            Chase,
            Attack,
            RunAway
        }
        private EnemyState currentState = EnemyState.Idle;

        private void Awake()
        {
            mink = GetComponent<Mink>();
            player = GameObject.FindGameObjectWithTag("Player").transform;
            chaseSpeed = mink.MoveSpeed;
            animator = GetComponent<Animator>();
            // rb = GetComponent<Rigidbody>();
        }

        void Update()
        {
            Vector3 directionToPlayer = (player.position - mink.transform.position).normalized;
            Debug.DrawLine(mink.transform.position, mink.transform.position + directionToPlayer * 5, Color.red);
            Debug.DrawLine(Camera.main.transform.position, Camera.main.transform.position + Camera.main.transform.forward * 5, Color.green);
            //Debug.Log($"{distanceToPlayer} distance detween player!");

            switch (currentState)
            {
                case EnemyState.Idle:
                    HandleIdleState();
                    break;
                case EnemyState.Chase:
                    HandleChaseState();
                    break;
                case EnemyState.Attack:
                    HandleAttackState();
                    break;
                case EnemyState.RunAway:
                    HandleRunAwayState();
                    break;
            }
        }

        private void HandleIdleState()
        {
            //SetAnimatorState("Idle");
            mink.SetMoveSpeed(3f);
            RandomMove();
            DetectRangeChangeState();
        }

        private void HandleChaseState()
        {
            DetectRangeChangeState();
            float distanceToPlayer = Vector3.Distance(mink.transform.position, player.position);
            if (!IsCameraLookingAtMe())
            {
                //SetAnimatorState("Idle");
                accelerateSpeed = Mathf.Lerp(1, 10, Mathf.InverseLerp(detectionRange, attackRange, distanceToPlayer));
                float targetSpeed = chaseSpeed * (12 - accelerateSpeed);
                mink.SetMoveSpeed(Mathf.Max(targetSpeed, 1f));
                EnemyChasePlayer();
            }
        }

        private void HandleAttackState()
        {
            // SetAnimatorState("Attack");
            AttackPlayer();
            animator.CrossFade("MinkAttackAnimation", 0f);
            StartCoroutine(WaitForAnimation("Attack"),EnemyState.RunAway);
            Debug.Log($"Finish Attack Animation!");
            // currentState = EnemyState.RunAway;
        }

        private void HandleRunAwayState()
        {
            EnemyRunAwayPlayer();
            mink.transform.forward = -(player.position - mink.transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(mink.transform.position, player.position);
            if (distanceToPlayer >= detectionRange * 0.8f)
            {
                currentState = EnemyState.Idle;
            }
        }

        private void DetectRangeChangeState()
        {
            float distanceToPlayer = Vector3.Distance(mink.transform.position, player.position);
            if (distanceToPlayer <= attackRange)
            {
                currentState = EnemyState.Attack;
            }
            else if (distanceToPlayer <= detectionRange)
            {
                currentState = EnemyState.Chase;
            }
            else
            {
                currentState = EnemyState.Idle;
            }
        }

        private void AttackPlayer()
        {
            // Example: Apply damage to the player
            if (player.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(mink.Damage);
            }
        }
        private void EnemyChasePlayer()
        {
            Vector3 moveDirection = (player.position - mink.transform.position).normalized;
            MoveForward(moveDirection);
        }

        private void EnemyRunAwayPlayer()
        {
            Vector3 direction = (player.position - mink.transform.position).normalized;
            direction.y = 0;
            mink.transform.Translate(-direction * 10 * Time.deltaTime, Space.World);
            mink.transform.forward = -direction;
        }

        public void RandomMove()
        {  
            if (Time.time >= nextDirectionChangeTime)
            {
                // random angle in degrees, converted to radians
                float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                moveDirection = new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle)).normalized;
                nextDirectionChangeTime = Time.time + changeDirectionInterval;
            }
            // Ensure movement occurs
            if (moveDirection != Vector3.zero)
            {
                MoveForward(moveDirection);
            }
        }

        public void MoveForward(Vector3 direction)
        {
            direction.y = 0;
            mink.transform.Translate(direction * mink.MoveSpeed * Time.deltaTime, Space.World);
            mink.transform.forward = direction;
        }

        private bool IsCameraLookingAtMe()
        {
            Vector3 directionToMink = (mink.transform.position - Camera.main.transform.position).normalized;
            float dotProduct = Vector3.Dot(Camera.main.transform.forward, directionToMink);
            float distanceToCamera = Vector3.Distance(mink.transform.position, Camera.main.transform.position);
             if (distanceToCamera < 5f)
            {
                return false;
            }
            return dotProduct > 0.8f;
        }

        // private void SetAnimatorState(string animationName)
        // {
        //     animator.SetTrigger(animationName);
        // }

        private IEnumerator WaitForAnimation(string animationName, EnemyState changeState)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            while (stateInfo.IsName(animationName) && stateInfo.normalizedTime < 1f)
            {
                yield return null;
                stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            }
            currentState = changeState;
        }
    }
}