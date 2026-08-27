using UnityEngine;

namespace SCPGame.SCP
{
    // ─────────────────────────────────────────────────────────────
    // SCP-173 "조각상" 전용 행동 (Stalker 유형의 대표).
    //
    // 특징:
    //  - 플레이어가 '보고 있는 동안'에는 절대 움직이지 않는다 (완전 정지)
    //  - 플레이어가 시선을 돌리거나 눈을 깜빡이는 순간 초고속으로 접근
    //  - 닿으면 즉사급 피해
    //
    // 구현 포인트:
    //  - '관측당하는 중'인지 판정: 플레이어 카메라 시야 안 + 벽 없음
    //  - 관측 중이면 NavMeshAgent 를 정지, 아니면 추격
    // ─────────────────────────────────────────────────────────────
    public class Scp173Entity : SCPEntity
    {
        private Camera playerCamera;
        private SCPGame.Gameplay.BlinkSystem blinkSystem;

        [Tooltip("눈 감은 순간 173이 접근하는 속도 배율 (0=정지, 1=전속력) — 관측 중에도 '조금' 움직임")]
        [Range(0f, 1f)]
        public float blinkMoveFactor = 0.4f;
        [Tooltip("관측 상태 유지 유예(초) — 순간적 관측 실패로 173이 튀어나오는 것 방지")]
        public float observeGrace = 0.2f;

        private float lastObservedTime = -999f;
        private Collider selfCol;

        protected override void Start()
        {
            base.Start();
            if (Camera.main != null) playerCamera = Camera.main;
            blinkSystem = FindFirstObjectByType<SCPGame.Gameplay.BlinkSystem>();
            selfCol = GetComponent<Collider>();
        }

        protected override void Update()
        {
            if (data == null || player == null) return;

            // 플레이어가 숨어 있으면 173은 플레이어를 놓치고 배회한다
            if (Core.PlayerState.IsHidden)
            {
                agent.isStopped = false;
                UpdatePatrol();
                return;
            }

            // 관측되면 시각 갱신. 유예(observeGrace) 동안은 '관측 중'으로 간주해
            // 순간적으로 관측이 끊겨도 튀어나오지 않게 한다.
            if (IsObservedByPlayer()) lastObservedTime = Time.time;
            bool recentlyObserved = (Time.time - lastObservedTime) < observeGrace;
            bool blinking = blinkSystem != null && blinkSystem.IsBlinking;

            // 관측 중이고 눈도 뜨고 있으면 완전 정지 (시간이 멈춘 듯)
            if (recentlyObserved && !blinking)
            {
                if (agent.hasPath) agent.ResetPath();  // 남은 경로 제거 → 눈 뜬 뒤 잔여 이동 방지
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            else
            {
                // 시선 밖이면 전속력, '보고 있어도 눈을 깜빡이는 순간'엔 조금만(느리게) 접근
                agent.isStopped = false;
                agent.speed = (recentlyObserved && blinking)
                    ? data.chaseSpeed * blinkMoveFactor
                    : data.chaseSpeed;
                agent.SetDestination(player.position);

                // 닿을 만큼 가까우면 공격
                if (DistanceToPlayer() <= data.attackRange &&
                    Time.time - lastAttackTime >= data.attackCooldown)
                {
                    lastAttackTime = Time.time;
                    DoAttack();
                }
            }
        }

        // ── 플레이어가 이 개체를 현재 보고 있는가 ──
        // ── 플레이어가 이 개체를 현재 보고 있는가 (다지점 판정으로 견고화) ──
        private bool IsObservedByPlayer()
        {
            if (playerCamera == null) return false;

            // 콜라이더 중심을 기준으로 위/중앙/아래 세 지점 검사
            Vector3 c = selfCol != null ? selfCol.bounds.center : transform.position + Vector3.up;
            Vector3 eye = playerCamera.transform.position;
            Vector3[] pts = { c, c + Vector3.up * 0.7f, c - Vector3.up * 0.7f };

            foreach (var p in pts)
            {
                Vector3 vp = playerCamera.WorldToViewportPoint(p);
                bool onScreen = vp.z > 0f && vp.x > 0f && vp.x < 1f && vp.y > 0f && vp.y < 1f;
                if (!onScreen) continue;

                Vector3 dir = p - eye;
                float dist = dir.magnitude;
                if (Physics.Raycast(eye, dir.normalized, out RaycastHit hit, dist + 0.3f))
                {
                    // 173(또는 자식)을 먼저 맞았으면 보이는 것
                    if (hit.transform == transform || hit.transform.IsChildOf(transform))
                        return true;
                    // 다른 것이 먼저 가로막음 → 이 지점은 가려짐, 다음 지점 검사
                }
                else
                {
                    // 지점 앞에 아무것도 없음(근접 시 빗맞음 등) → 보이는 것으로 간주
                    return true;
                }
            }
            return false;
        }
    }
}
