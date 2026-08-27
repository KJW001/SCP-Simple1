using UnityEngine;

namespace SCPGame.Player
{
    // ─────────────────────────────────────────────────────────────
    // 플레이어의 사운드를 담당합니다.
    //  - 발소리 : 이동 속도에 맞춰 일정 간격으로 재생
    //  - 심장박동/거친 숨소리 : 정신력이 낮을수록 크고 빠르게 (공포 연출)
    //  - 피격음 : 데미지를 받을 때
    //
    // 인스펙터에서 AudioClip 들을 넣어주면 동작합니다.
    // ─────────────────────────────────────────────────────────────
    [RequireComponent(typeof(FirstPersonController))]
    public class PlayerSound : MonoBehaviour
    {
        [Header("발소리")]
        [Tooltip("걸을 때 발소리 후보(랜덤 재생)")]
        public AudioClip[] footstepClips;
        [Tooltip("걷기 발소리 간격(초)")]
        public float walkStepInterval = 0.5f;
        [Tooltip("달리기 발소리 간격(초)")]
        public float runStepInterval = 0.32f;

        [Header("숨소리 / 심장박동")]
        public AudioClip heartbeatClip;

        [Header("피격음")]
        public AudioClip[] hurtClips;

        [Header("오디오 소스")]
        [Tooltip("발소리/피격음용 (없으면 자동 생성)")]
        public AudioSource sfxSource;
        [Tooltip("심장박동 루프용 (없으면 자동 생성)")]
        public AudioSource heartbeatSource;

        private FirstPersonController controller;
        private PlayerHealth health;
        private float stepTimer;

        private void Awake()
        {
            controller = GetComponent<FirstPersonController>();
            health = GetComponent<PlayerHealth>();

            // AudioSource 가 지정되지 않았으면 코드로 만들어 붙인다
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
            if (heartbeatSource == null) heartbeatSource = gameObject.AddComponent<AudioSource>();

            // 심장박동은 계속 반복 재생
            heartbeatSource.loop = true;
            heartbeatSource.playOnAwake = false;
            heartbeatSource.clip = heartbeatClip;
        }

        private void OnEnable()
        {
            // 피격 이벤트를 구독해서 피격음을 재생하도록 연결
            if (health != null) health.OnDamaged += PlayHurt;
        }

        private void OnDisable()
        {
            if (health != null) health.OnDamaged -= PlayHurt;
        }

        private void Update()
        {
            HandleFootsteps();
            HandleHeartbeat();
        }

        // ── 이동 중일 때 일정 간격으로 발소리 재생 ──
        private void HandleFootsteps()
        {
            if (footstepClips == null || footstepClips.Length == 0) return;

            // 땅에 닿아 실제로 움직이는 중일 때만
            if (controller.IsGrounded && controller.CurrentSpeed > 0.5f)
            {
                stepTimer -= Time.deltaTime;
                if (stepTimer <= 0f)
                {
                    // 후보 중 랜덤으로 하나 골라 재생 → 단조로움 방지
                    AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
                    sfxSource.PlayOneShot(clip);

                    // 달리는 중이면 간격을 짧게
                    stepTimer = controller.IsRunning ? runStepInterval : walkStepInterval;
                }
            }
            else
            {
                stepTimer = 0f; // 멈추면 즉시 다음 걸음을 낼 준비
            }
        }

        // ── 정신력이 낮을수록 심장박동을 크고 빠르게 ──
        private void HandleHeartbeat()
        {
            if (heartbeatClip == null || health == null) return;

            // 정신력이 50% 이하로 떨어지면 심장박동 시작
            bool shouldBeat = health.SanityRatio < 0.5f;

            if (shouldBeat)
            {
                if (!heartbeatSource.isPlaying) heartbeatSource.Play();

                // 정신력이 낮을수록(=t가 1에 가까울수록) 더 빠르고 크게
                float t = 1f - (health.SanityRatio / 0.5f); // 0~1
                heartbeatSource.pitch = Mathf.Lerp(0.9f, 1.6f, t);
                heartbeatSource.volume = Mathf.Lerp(0.2f, 1f, t);
            }
            else if (heartbeatSource.isPlaying)
            {
                heartbeatSource.Stop();
            }
        }

        // ── 피격음 재생 (이벤트로 호출됨) ──
        private void PlayHurt()
        {
            if (hurtClips == null || hurtClips.Length == 0) return;
            AudioClip clip = hurtClips[Random.Range(0, hurtClips.Length)];
            sfxSource.PlayOneShot(clip);
        }
    }
}
