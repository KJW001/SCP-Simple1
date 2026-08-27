using UnityEngine;
using SCPGame.Player;

namespace SCPGame.SCP
{
    // ─────────────────────────────────────────────────────────────
    // SCP-096 "부끄럼쟁이" 전용 행동.
    //
    // 특징:
    //  - 평소에는 얌전히 웅크리고(트리거 전) 먼저 공격하지 않는다.
    //  - 플레이어가 ★3초 이상 연속으로 응시★하면 격노(Enrage)한다.
    //    (잠깐 스쳐 보는 정도로는 격노하지 않음 — 난이도 완화)
    //  - 한 번 격노하면 맵 어디에 있든 끝까지 추격한다.
    //
    // 구현 포인트:
    //  - 매 프레임 '보고 있는가'를 검사해 응시 시간을 누적한다.
    //  - 시선을 떼면 누적 시간이 서서히 줄어든다(잠깐 끊겨도 바로 리셋 안 됨).
    // ─────────────────────────────────────────────────────────────
    public class Scp096Entity : SCPEntity
    {
        [Header("SCP-096 설정")]
        [Tooltip("격노 상태인지 (트리거되면 true)")]
        public bool isEnraged = false;

        [Tooltip("격노까지 필요한 연속 응시 시간(초)")]
        public float gazeToEnrage = 3f;

        [Tooltip("시선을 뗐을 때 응시 게이지가 식는 속도(배). 1이면 쳐다본 속도와 같게 감소")]
        public float gazeCooldownMultiplier = 1.5f;

        private float gazeTimer = 0f;   // 현재까지 응시한 시간
        private Camera playerCamera;

        // 다른 시스템(UI 경고 등)이 참조할 수 있는 응시 진행도 0~1
        public float GazeProgress { get { return Mathf.Clamp01(gazeTimer / gazeToEnrage); } }

        protected override void Start()
        {
            base.Start();
            if (Camera.main != null) playerCamera = Camera.main;
        }

        protected override void Update()
        {
            if (data == null || player == null) return;

            if (!isEnraged)
            {
                // 보고 있으면 응시 시간을 쌓고, 안 보면 서서히 식힌다
                if (IsSeenByPlayer())
                {
                    gazeTimer += Time.deltaTime;
                    if (gazeTimer >= gazeToEnrage)
                    {
                        Enrage();
                        return;
                    }
                }
                else
                {
                    gazeTimer -= Time.deltaTime * gazeCooldownMultiplier;
                    if (gazeTimer < 0f) gazeTimer = 0f;
                }

                // 트리거 전에는 느리게 배회
                UpdatePatrol();
                return;
            }

            // 격노 후에는 오로지 플레이어만 추격 (기본 FSM 재사용)
            base.Update();
        }

        // ── 격노 발동 ──
        private void Enrage()
        {
            isEnraged = true;
            agent.speed = data.chaseSpeed;
            lastKnownPos = player.position;
            ChangeState(State.Chase);
            Debug.Log($"{data.nickname} 격노! 너무 오래 쳐다봤습니다. 도망치세요.");
        }

        // 격노 후에는 시야/소리와 무관하게 항상 감지된 것으로 처리
        protected override bool CanSensePlayer()
        {
            return isEnraged;
        }

        // ── 플레이어가 이 개체를 '보고 있는가' 판정 ──
        private bool IsSeenByPlayer()
        {
            if (playerCamera == null) return false;

            // 조준점: 콜라이더의 세계 중심 (바닥에 붙은 개체도 몸통을 조준하게)
            Vector3 aimPoint = GetAimPoint();

            // 1) 화면 안(뷰포트)에 들어와 있는지
            Vector3 vp = playerCamera.WorldToViewportPoint(aimPoint);
            bool onScreen = vp.z > 0f && vp.x > 0.05f && vp.x < 0.95f && vp.y > 0.05f && vp.y < 0.95f;
            if (!onScreen) return false;

            // 2) 감지 거리 안인지
            if (DistanceToPlayer() > data.detectionRange) return false;

            // 3) 사이에 벽이 없는지: 여러 지점(머리/가슴/발)으로 레이를 쏘아
            //    하나라도 이 개체를 직접 맞추면 '보고 있다'로 인정 (바닥에 묻혀도 인식)
            Vector3 eye = playerCamera.transform.position;
            Vector3[] targets = new Vector3[]
            {
                aimPoint,
                aimPoint + Vector3.up * 0.6f,
                aimPoint - Vector3.up * 0.4f
            };
            foreach (var tp in targets)
            {
                Vector3 dir = tp - eye;
                if (Physics.Raycast(eye, dir.normalized, out RaycastHit hit, data.detectionRange))
                {
                    if (hit.transform == transform || hit.transform.IsChildOf(transform))
                        return true;
                }
            }
            return false;
        }

        // 이 개체의 조준 기준점(콜라이더 세계 중심). 콜라이더가 없으면 위치+1m.
        private Vector3 GetAimPoint()
        {
            var col = GetComponent<Collider>();
            if (col != null) return col.bounds.center;
            return transform.position + Vector3.up;
        }
    }
}
