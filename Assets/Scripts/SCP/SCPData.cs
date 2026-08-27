using UnityEngine;

namespace SCPGame.SCP
{
    // ─────────────────────────────────────────────────────────────
    // SCP의 "행동 유형(아키타입)".
    // 개체 20~30종을 전부 다른 클래스로 만들면 관리가 어렵기 때문에,
    // 몇 가지 대표 행동 패턴으로 묶고, 세부 수치는 SCPData 로 조절합니다.
    // ─────────────────────────────────────────────────────────────
    public enum SCPArchetype
    {
        Chaser,       // 플레이어를 보면 곧장 추격 (가장 기본)
        Stalker,      // 관측당하면 멈추고, 안 볼 때만 다가옴 (예: SCP-173)
        LineOfSight,  // 얼굴/모습을 보면 격노하여 폭주 (예: SCP-096 부끄럼쟁이)
        Wanderer,     // 정해진 지점을 배회, 가까우면 공격 (환경형)
        Relentless,   // 느리지만 끈질기게 영원히 추격 (예: SCP-049 역병의사)
        Teleporter    // 시야에서 벗어나면 순간이동으로 접근
    }

    // ─────────────────────────────────────────────────────────────
    // 한 SCP 개체의 스펙(설계 수치)을 담는 데이터입니다.
    // ScriptableObject 이므로 에셋으로 만들어 관리할 수도 있고,
    // 코드(SCPCatalog)에서 직접 생성해 쓸 수도 있습니다.
    // ─────────────────────────────────────────────────────────────
    [CreateAssetMenu(fileName = "NewSCP", menuName = "SCP Game/SCP Data")]
    public class SCPData : ScriptableObject
    {
        [Header("식별 정보")]
        [Tooltip("예: SCP-096")]
        public string scpNumber = "SCP-000";
        [Tooltip("별명. 예: 부끄럼쟁이")]
        public string nickname = "미분류 개체";
        [Tooltip("위험 등급 (Safe / Euclid / Keter)")]
        public string containmentClass = "Euclid";
        [TextArea] public string description = "설명 없음";

        [Header("행동 유형")]
        public SCPArchetype archetype = SCPArchetype.Chaser;

        [Header("이동")]
        [Tooltip("순찰(배회) 속도")]
        public float patrolSpeed = 1.5f;
        [Tooltip("추격 속도")]
        public float chaseSpeed = 4f;

        [Header("감지")]
        [Tooltip("플레이어를 알아채는 거리")]
        public float detectionRange = 12f;
        [Tooltip("시야각(도). 이 범위 안이어야 '봤다'고 판정")]
        public float fieldOfView = 110f;
        [Tooltip("소리로 감지하는 거리 (뒤에 있어도 감지)")]
        public float hearingRange = 6f;

        [Header("전투")]
        [Tooltip("공격이 닿는 거리")]
        public float attackRange = 1.8f;
        [Tooltip("한 번 공격 데미지")]
        public float attackDamage = 20f;
        [Tooltip("공격 간격(초)")]
        public float attackCooldown = 1.2f;

        [Header("정신력 압박")]
        [Tooltip("가까이 있을 때 초당 깎이는 플레이어 정신력")]
        public float sanityDrainPerSecond = 3f;

        [Header("체력")]
        [Tooltip("이 개체의 체력 (0 이하면 무적 취급)")]
        public float maxHealth = 0f;
    }
}
