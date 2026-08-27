using UnityEngine;

namespace SCPGame.SCP
{
    // ─────────────────────────────────────────────────────────────
    // SCP-049 "역병의사" 전용 행동 (Relentless 유형).
    //
    // 특징:
    //  - 느리지만 결코 멈추지 않는다. 한 번 감지하면 끝까지 쫓아온다.
    //  - 시야에서 사라져도 플레이어의 마지막 위치가 아니라
    //    '현재 위치'를 계속 알고 추적한다 (끈질김).
    //  - 손이 닿으면 강력한 피해.
    //
    // 구현 포인트:
    //  - 한 번 감지되면 hasSpottedPlayer = true 로 고정 → 영원히 추격
    // ─────────────────────────────────────────────────────────────
    public class Scp049Entity : SCPEntity
    {
        [Header("SCP-049 설정")]
        [Tooltip("플레이어를 한 번이라도 발견했는지")]
        public bool hasSpottedPlayer = false;

        protected override void Update()
        {
            if (data == null || player == null) return;

            // 숨어 있으면 추적을 멈춘다 (제자리 대기)
            if (Core.PlayerState.IsHidden) { agent.isStopped = true; return; }
            agent.isStopped = false;

            // 아직 발견 못 했으면 일반 순찰 + 감지
            if (!hasSpottedPlayer)
            {
                base.Update(); // 기본 FSM (순찰/감지)

                // 기본 로직이 추격에 들어갔다면 = 발견한 것
                if (currentState == State.Chase)
                {
                    hasSpottedPlayer = true;
                    Debug.Log($"{data.nickname}: \"이 병을 치료해 드리겠습니다...\"");
                }
                return;
            }

            // 발견 이후에는 끈질기게 현재 위치로 계속 추격
            agent.speed = data.chaseSpeed;
            agent.SetDestination(player.position);

            if (DistanceToPlayer() <= data.attackRange &&
                Time.time - lastAttackTime >= data.attackCooldown)
            {
                lastAttackTime = Time.time;
                FaceTarget(player.position);
                DoAttack();
            }

            // 근처에 있으면 정신력도 계속 압박
            DrainPlayerSanity();
        }
    }
}
