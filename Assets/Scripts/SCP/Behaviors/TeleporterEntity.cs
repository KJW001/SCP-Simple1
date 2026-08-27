using UnityEngine;
using UnityEngine.AI;

namespace SCPGame.SCP
{
    // ─────────────────────────────────────────────────────────────
    // Teleporter 유형 SCP 전용 행동.
    //
    // 특징:
    //  - 플레이어 시야에서 벗어나면(안 볼 때) 플레이어 근처로 순간이동해
    //    갑자기 뒤에 나타난다 (점프 스케어).
    //  - 보고 있는 동안에는 순간이동하지 않는다.
    // ─────────────────────────────────────────────────────────────
    public class TeleporterEntity : SCPEntity
    {
        [Header("순간이동 설정")]
        [Tooltip("순간이동 재사용 대기시간(초)")]
        public float teleportCooldown = 5f;
        [Tooltip("플레이어 뒤 몇 m 지점에 나타날지")]
        public float appearDistance = 4f;

        private Camera playerCamera;
        private float lastTeleportTime;

        protected override void Start()
        {
            base.Start();
            if (Camera.main != null) playerCamera = Camera.main;
        }

        protected override void Update()
        {
            if (Core.PlayerState.IsHidden) return; // 숨어 있으면 순간이동/추격 안 함
            base.Update(); // 기본 추격/공격은 그대로 사용

            // 쿨다운마다, 플레이어가 안 보고 있을 때 순간이동 시도
            if (Time.time - lastTeleportTime >= teleportCooldown && !IsVisibleToPlayer())
            {
                TryTeleportBehindPlayer();
            }
        }

        // ── 플레이어 뒤쪽으로 순간이동 ──
        private void TryTeleportBehindPlayer()
        {
            // 플레이어 뒤쪽 방향 계산
            Vector3 behind = player.position - player.forward * appearDistance;

            // 그 지점 근처의 유효한 NavMesh 위치를 찾는다
            if (NavMesh.SamplePosition(behind, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position); // NavMeshAgent 를 안전하게 순간이동
                lastTeleportTime = Time.time;
                Debug.Log($"{data.nickname}이(가) 당신 뒤에 나타났다...");
            }
        }

        // ── 플레이어 화면에 이 개체가 보이는가 ──
        private bool IsVisibleToPlayer()
        {
            if (playerCamera == null) return false;
            Vector3 vp = playerCamera.WorldToViewportPoint(transform.position + Vector3.up);
            return vp.z > 0f && vp.x > 0f && vp.x < 1f && vp.y > 0f && vp.y < 1f;
        }
    }
}
