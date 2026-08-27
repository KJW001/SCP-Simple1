using UnityEngine;
using UnityEngine.AI;

namespace SCPGame.SCP
{
    // ─────────────────────────────────────────────────────────────
    // SCP 애니메이션 구동기.
    //
    // NavMeshAgent 가 "얼마나 빠르게 움직이는지"를 읽어서
    // Animator 의 Speed 파라미터에 넣어줍니다.
    // 그러면 Animator 가 알아서 Idle <-> Move 를 전환합니다.
    //
    // [왜 분리했나요?]
    //  AI(생각)와 애니메이션(표현)을 나누면, AI 코드를 건드리지 않고도
    //  애니메이션만 교체할 수 있습니다. 역할 분리의 좋은 예입니다.
    //
    // [필요한 Animator 파라미터]
    //  - Speed  (Float)   : 이동 속도
    //  - Attack (Trigger) : 공격 순간
    // ─────────────────────────────────────────────────────────────
    [RequireComponent(typeof(NavMeshAgent))]
    public class SCPAnimatorDriver : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("비우면 자식에서 자동으로 찾습니다")]
        public Animator animator;

        [Header("설정")]
        [Tooltip("속도 값이 급변하지 않게 부드럽게 만드는 정도")]
        public float smoothing = 8f;

        private NavMeshAgent agent;
        private float smoothedSpeed;

        // Animator 파라미터 이름을 숫자 ID로 미리 변환해두면 더 빠릅니다
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            if (animator == null || agent == null) return;

            // 실제 이동 속도(수평 성분만)를 구한다
            Vector3 v = agent.velocity;
            v.y = 0f;
            float target = v.magnitude;

            // 값이 튀지 않도록 부드럽게 보간
            smoothedSpeed = Mathf.Lerp(smoothedSpeed, target, Time.deltaTime * smoothing);
            animator.SetFloat(SpeedHash, smoothedSpeed);
        }

        /// <summary>공격 애니메이션 재생 (SCPEntity 에서 호출)</summary>
        public void PlayAttack()
        {
            if (animator == null) return;
            animator.SetTrigger(AttackHash);
        }
    }
}
