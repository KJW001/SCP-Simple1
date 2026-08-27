using UnityEngine;
using UnityEngine.UI;

namespace SCPGame.Player
{
    // ─────────────────────────────────────────────────────────────
    // 플레이어의 화면 효과(VFX)를 담당합니다.
    //  - 피격 시 화면 가장자리가 빨갛게 번쩍(비네트)
    //  - 정신력이 낮으면 화면 가장자리가 어둡게 물듦
    //  - 피격 시 카메라 흔들림(쉐이크)
    //
    // [준비물]
    //  - 화면을 덮는 Canvas 위 Image 하나(가장자리 붉은 비네트 텍스처)를
    //    damageVignette 에 연결하세요.
    // ─────────────────────────────────────────────────────────────
    public class PlayerVFX : MonoBehaviour
    {
        [Header("피격 비네트")]
        [Tooltip("화면을 덮는 붉은 비네트 이미지")]
        public Image damageVignette;
        [Tooltip("피격 시 최대 불투명도")]
        public float damageFlashAlpha = 0.6f;
        [Tooltip("사라지는 속도")]
        public float fadeSpeed = 2f;

        [Header("저정신력 비네트")]
        [Tooltip("정신력이 낮을 때 어둡게 물드는 이미지")]
        public Image sanityVignette;

        [Header("카메라 흔들림")]
        public Transform cameraTransform;
        public float shakeDuration = 0.25f;
        public float shakeMagnitude = 0.15f;

        private PlayerHealth health;
        private float currentDamageAlpha;   // 현재 피격 비네트 투명도
        private float shakeTimer;           // 남은 흔들림 시간
        private Vector3 camDefaultLocalPos; // 카메라 원위치(흔들림 복원용)

        private void Awake()
        {
            health = GetComponent<PlayerHealth>();
            if (cameraTransform != null)
                camDefaultLocalPos = cameraTransform.localPosition;
        }

        private void OnEnable()
        {
            // 체력/정신력 이벤트를 구독해 자동으로 화면 효과를 갱신
            if (health != null)
            {
                health.OnDamaged += OnDamaged;
                health.OnSanityChanged += OnSanityChanged;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnDamaged -= OnDamaged;
                health.OnSanityChanged -= OnSanityChanged;
            }
        }

        private void Update()
        {
            FadeDamageVignette();
            HandleCameraShake();
        }

        // ── 피격 순간: 비네트를 확 밝히고 카메라를 흔든다 ──
        private void OnDamaged()
        {
            currentDamageAlpha = damageFlashAlpha;
            shakeTimer = shakeDuration;
        }

        // ── 피격 비네트를 서서히 투명하게 되돌린다 ──
        private void FadeDamageVignette()
        {
            if (damageVignette == null) return;

            currentDamageAlpha = Mathf.MoveTowards(currentDamageAlpha, 0f, fadeSpeed * Time.deltaTime);
            SetImageAlpha(damageVignette, currentDamageAlpha);
        }

        // ── 정신력이 바뀔 때 저정신력 비네트 농도 갱신 ──
        private void OnSanityChanged(float current, float max)
        {
            if (sanityVignette == null) return;

            float ratio = current / max;
            // 정신력 40% 이하부터 서서히 어두워지고, 0%에서 가장 짙어진다
            float alpha = Mathf.InverseLerp(0.4f, 0f, ratio) * 0.7f;
            SetImageAlpha(sanityVignette, alpha);
        }

        // ── 카메라 흔들림 처리 ──
        private void HandleCameraShake()
        {
            if (cameraTransform == null) return;

            if (shakeTimer > 0f)
            {
                shakeTimer -= Time.deltaTime;
                // 무작위 방향으로 살짝 흔든다
                Vector3 offset = Random.insideUnitSphere * shakeMagnitude;
                offset.z = 0f; // 앞뒤로는 흔들지 않음
                cameraTransform.localPosition = camDefaultLocalPos + offset;
            }
            else
            {
                // 흔들림이 끝나면 원위치로 복귀
                cameraTransform.localPosition = Vector3.Lerp(
                    cameraTransform.localPosition, camDefaultLocalPos, Time.deltaTime * 10f);
            }
        }

        // ── 이미지의 알파(투명도)만 바꾸는 도우미 함수 ──
        private void SetImageAlpha(Image img, float a)
        {
            Color c = img.color;
            c.a = a;
            img.color = c;
        }
    }
}
