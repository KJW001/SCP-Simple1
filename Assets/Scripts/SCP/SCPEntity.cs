using UnityEngine;
using UnityEngine.AI;
using SCPGame.Core;
using SCPGame.Player;

namespace SCPGame.SCP
{
    // ─────────────────────────────────────────────────────────────
    // 모든 SCP의 "기반(베이스)" 클래스입니다.
    // NavMeshAgent 로 길을 찾아 움직이고, 유한 상태 기계(FSM)로
    // 상태(순찰/추격/공격/탐색)를 전환합니다.
    //
    // 각 개체의 특수 행동은 이 클래스를 상속받아 필요한 함수만
    // override(재정의) 하면 됩니다. (예: Scp096Entity, Scp173Entity)
    //
    // [준비물]
    //  - 오브젝트에 NavMeshAgent 필요
    //  - 씬 바닥에 NavMesh 를 Bake 해두어야 이동합니다
    //  - IDamageable 구현: 플레이어가 무기로 SCP에 피해를 줄 수 있음
    // ─────────────────────────────────────────────────────────────
    [RequireComponent(typeof(NavMeshAgent))]
    public class SCPEntity : MonoBehaviour, IDamageable
    {
        // FSM 상태 정의
        public enum State
        {
            Idle,    // 대기
            Patrol,  // 순찰(배회)
            Chase,   // 추격
            Attack,  // 공격
            Search   // 놓친 위치 탐색
        }

        [Header("이 개체의 데이터")]
        public SCPData data;

        [Header("순찰 지점 (선택)")]
        [Tooltip("비워두면 시작 위치 주변을 랜덤 배회")]
        public Transform[] patrolPoints;

        // ── 보호(protected) 필드: 자식 클래스에서 접근 가능 ──
        protected NavMeshAgent agent;     // 길찾기 이동 담당
        protected Transform player;       // 추적 대상(플레이어)
        protected PlayerHealth playerHealth;
        protected State currentState = State.Idle;

        protected float currentHealth;    // 현재 체력
        protected float lastAttackTime;   // 마지막 공격 시각
        protected Vector3 lastKnownPos;   // 플레이어를 마지막으로 본 위치
        protected int patrolIndex;        // 현재 순찰 지점 번호
        protected Vector3 spawnPoint;     // 스폰 위치(랜덤 배회 기준)
        protected SCPAnimatorDriver animDriver;    // 애니메이션 구동기(있으면 사용)


        // IDamageable: 체력이 0 이하로 설정된 데이터는 '무적'으로 본다
        public bool IsAlive => data == null || data.maxHealth <= 0f || currentHealth > 0f;

        protected virtual void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            spawnPoint = transform.position;
            // 주의: 이 시점에 data 는 아직 null 일 수 있습니다.
            //       (스포너가 컴포넌트를 붙인 뒤 data 를 넣기 때문)
            //       → data 에 의존하는 초기화는 Start 에서 합니다.
        }

        protected virtual void Start()
        {
            // 데이터 기반 초기 세팅 (data 가 채워진 뒤 실행됨)
            if (data != null)
            {
                currentHealth = data.maxHealth;
                agent.speed = data.patrolSpeed;
            }

            // 씬에서 플레이어를 찾아 참조해둔다
            playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null) player = playerHealth.transform;

            ChangeState(State.Patrol);
        }

        protected virtual void Update()
        {
            if (data == null || player == null) return;

            // ★ 플레이어가 숨어 있으면 추격/공격 중이던 개체는 '놓친' 것으로 처리해
            //   배회 상태로 되돌린다. (CanSensePlayer 가 숨은 동안 false 를 반환하므로
            //   아래 FSM 이 자연스럽게 Patrol 로 돌아가 계속 돌아다닌다.)
            if (Core.PlayerState.IsHidden &&
                (currentState == State.Chase || currentState == State.Attack || currentState == State.Search))
            {
                ChangeState(State.Patrol);
            }

            // 상태별 행동 처리 (FSM의 핵심)
            switch (currentState)
            {
                case State.Idle:   UpdateIdle();   break;
                case State.Patrol: UpdatePatrol(); break;
                case State.Chase:  UpdateChase();  break;
                case State.Attack: UpdateAttack(); break;
                case State.Search: UpdateSearch(); break;
            }

            // 플레이어가 근처면 정신력을 지속적으로 깎는다 (공포 압박)
            DrainPlayerSanity();
        }

        // ─────────────────────────────────────────────
        // 상태별 행동
        // ─────────────────────────────────────────────

        protected virtual void UpdateIdle()
        {
            // 잠깐 멈춰 있다가 다시 순찰
            if (CanSensePlayer()) { ChangeState(State.Chase); return; }
        }

        protected virtual void UpdatePatrol()
        {
            // 순찰 속도가 0으로 설정된 개체도 배회하도록 최소 속도 보장
            agent.speed = data.patrolSpeed > 0.1f ? data.patrolSpeed : Mathf.Max(1.5f, data.chaseSpeed * 0.4f);
            agent.isStopped = false;

            // 플레이어를 감지하면 추격으로 전환 (숨어있을 땐 CanSensePlayer가 false)
            if (CanSensePlayer())
            {
                ChangeState(State.Chase);
                return;
            }

            // 경로를 계산 중이면 완료될 때까지 기다린다 (여기서 목적지를 새로 정하면
            // 계산이 영원히 끝나지 않아 그 자리에 멈춰버린다 — 중요!)
            if (agent.pathPending) return;

            // 목적지가 없거나(처음) 목적지에 도착했으면 새 순찰 지점을 정한다
            if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + 0.5f)
            {
                GoToNextPatrolPoint();
            }
        }

        protected virtual void UpdateChase()
        {
            agent.speed = data.chaseSpeed;

            // 시야에 계속 보이면 마지막 위치 갱신하며 추격
            if (CanSensePlayer())
            {
                lastKnownPos = player.position;
                agent.SetDestination(player.position);

                // 공격 사거리 안에 들어오면 공격 상태로
                if (DistanceToPlayer() <= data.attackRange)
                    ChangeState(State.Attack);
            }
            else
            {
                // 놓쳤다면 마지막으로 본 위치로 이동해 탐색
                ChangeState(State.Search);
            }
        }

        protected virtual void UpdateAttack()
        {
            // 숨어 있으면 공격하지 않고 배회로 복귀 (안전장치)
            if (Core.PlayerState.IsHidden) { ChangeState(State.Patrol); return; }

            // 플레이어를 바라보게 회전
            FaceTarget(player.position);

            // 사거리를 벗어나면 다시 추격
            if (DistanceToPlayer() > data.attackRange)
            {
                ChangeState(State.Chase);
                return;
            }

            // 쿨다운마다 한 번씩 공격
            if (Time.time - lastAttackTime >= data.attackCooldown)
            {
                lastAttackTime = Time.time;
                DoAttack();
            }
        }

        protected virtual void UpdateSearch()
        {
            agent.speed = data.chaseSpeed;
            agent.SetDestination(lastKnownPos);

            // 탐색 지점에 도착했는데도 플레이어가 안 보이면 순찰로 복귀
            if (!agent.pathPending && agent.remainingDistance < 1f)
            {
                if (CanSensePlayer()) ChangeState(State.Chase);
                else ChangeState(State.Patrol);
            }
            else if (CanSensePlayer())
            {
                ChangeState(State.Chase);
            }
        }

        // ─────────────────────────────────────────────
        // 감지 로직 (자식 클래스에서 override 가능)
        // ─────────────────────────────────────────────

        /// <summary>플레이어를 감지했는가? (시야 + 소리)</summary>
        protected virtual bool CanSensePlayer()
        {
            // 사물함 등에 숨어 있으면 감지하지 못한다
            // (SCP-096 처럼 이 함수를 override 한 개체는 예외 — 원작 설정)
            if (Core.PlayerState.IsHidden) return false;

            float dist = DistanceToPlayer();

            // 1) 아주 가까이서 나는 소리로 감지 (뒤에 있어도 들림)
            if (dist <= data.hearingRange) return true;

            // 2) 시야 감지: 감지 거리 + 시야각 + 장애물 없음
            if (dist <= data.detectionRange && IsPlayerInSight())
                return true;

            return false;
        }

        /// <summary>플레이어가 시야각 안에 있고, 사이에 벽이 없는가?</summary>
        protected bool IsPlayerInSight()
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;

            // 시야각(FOV) 체크: 정면과 플레이어 방향의 각도 비교
            float angle = Vector3.Angle(transform.forward, dirToPlayer);
            if (angle > data.fieldOfView * 0.5f) return false;

            // 시선 사이에 벽 등 장애물이 있으면 못 본 것으로 처리
            Vector3 eye = transform.position + Vector3.up * 1.5f;
            if (Physics.Raycast(eye, dirToPlayer, out RaycastHit hit, data.detectionRange))
            {
                // 레이가 먼저 맞은 게 플레이어가 아니면 가려진 것
                if (!hit.collider.GetComponentInParent<PlayerHealth>())
                    return false;
            }
            return true;
        }

        // ─────────────────────────────────────────────
        // 공용 도우미 함수들
        // ─────────────────────────────────────────────

        /// <summary>공격 실행 (플레이어에게 데미지)</summary>
        /// <summary>공격 실행 (플레이어에게 데미지 + 애니메이션 재생)</summary>
        protected virtual void DoAttack()
        {
            // 애니메이터가 붙어 있으면 공격 모션을 재생한다
            if (animDriver == null) animDriver = GetComponent<SCPAnimatorDriver>();
            if (animDriver != null) animDriver.PlayAttack();

            if (playerHealth != null && playerHealth.IsAlive)
            {
                playerHealth.TakeDamage(data.attackDamage, gameObject);
                Debug.Log($"{data.nickname}이(가) 공격! ({data.attackDamage} 데미지)");
            }
        }

        /// <summary>플레이어 근접 시 정신력 감소</summary>
        protected virtual void DrainPlayerSanity()
        {
            if (playerHealth == null || data.sanityDrainPerSecond <= 0f) return;

            // 감지 거리 안이고 실제로 보고 있을 때만 압박
            if (DistanceToPlayer() <= data.detectionRange && IsPlayerInSight())
                playerHealth.ReduceSanity(data.sanityDrainPerSecond * Time.deltaTime);
        }

        /// <summary>다음 순찰 지점으로 목적지 설정</summary>
        protected void GoToNextPatrolPoint()
        {
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                // 지정된 순찰 지점을 순서대로 돈다
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[patrolIndex].position);
            }
            else
            {
                // 지정 지점이 없으면 맵 전체를 랜덤하게 배회한다.
                // NavMesh 위의 임의 지점을 여러 번 시도해서 '멀리' 떨어진 곳을 고른다.
                Vector3 best = transform.position;
                float bestDist = 0f;
                for (int i = 0; i < 6; i++)
                {
                    // 현재 위치 기준 넓은 범위(최대 45m)에서 랜덤 방향
                    Vector2 r = Random.insideUnitCircle * 45f;
                    Vector3 cand = transform.position + new Vector3(r.x, 0f, r.y);
                    if (NavMesh.SamplePosition(cand, out NavMeshHit hit, 12f, NavMesh.AllAreas))
                    {
                        float d = Vector3.Distance(hit.position, transform.position);
                        if (d > bestDist) { bestDist = d; best = hit.position; }
                    }
                }
                agent.SetDestination(best);
            }
        }

        /// <summary>대상 쪽으로 부드럽게 회전</summary>
        protected void FaceTarget(Vector3 targetPos)
        {
            Vector3 dir = (targetPos - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) return;
            Quaternion look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * 8f);
        }

        protected float DistanceToPlayer()
        {
            return Vector3.Distance(transform.position, player.position);
        }

        /// <summary>상태 전환 (로그 출력으로 학습 시 상태 흐름 확인 용이)</summary>
        protected void ChangeState(State newState)
        {
            if (currentState == newState) return;
            currentState = newState;
            // Debug.Log($"{data.nickname} 상태 → {newState}");
        }

        // ── IDamageable 구현: SCP도 피해를 받을 수 있음 ──
        public virtual void TakeDamage(float damage, GameObject attacker = null)
        {
            // 데이터상 무적(maxHealth<=0)이면 무시
            if (data == null || data.maxHealth <= 0f) return;

            currentHealth -= damage;
            if (currentHealth <= 0f)
            {
                Debug.Log($"{data.nickname} 제압됨.");
                Destroy(gameObject);
            }
        }

        // ── 씬 뷰에서 감지 범위를 시각적으로 확인하는 기즈모 ──
        protected virtual void OnDrawGizmosSelected()
        {
            if (data == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, data.detectionRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, data.attackRange);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, data.hearingRange);
        }
    }
}