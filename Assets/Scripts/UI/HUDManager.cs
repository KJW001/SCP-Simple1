using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SCPGame.Player;

namespace SCPGame.UI
{
    // ─────────────────────────────────────────────────────────────
    // 화면 위 상태 표시(HUD) — 체력 / 정신력 / 기력(스태미나) / 배터리.
    //
    //  ★ 폴링 방식: 매 프레임 각 소스(PlayerHealth / PlayerStamina /
    //     Flashlight)의 현재 비율을 직접 읽어 Fill(슬라이드)에 반영한다.
    //     이벤트 구독은 실행 순서에 따라 놓칠 수 있어, 확실한 폴링을 쓴다.
    //
    //  [준비물] 각 바의 Fill 은 Image(Image Type=Filled, Fill Method=Horizontal)
    // ─────────────────────────────────────────────────────────────
    public class HUDManager : MonoBehaviour
    {
        [Header("참조 소스 (비우면 자동 검색)")]
        public PlayerHealth playerHealth;
        public PlayerStamina playerStamina;
        public Flashlight flashlight;

        [Header("체력 UI")]
        public Image healthFill;
        public TMP_Text healthText;

        [Header("정신력 UI")]
        public Image sanityFill;
        public TMP_Text sanityText;

        [Header("기력(스태미나) UI")]
        public Image staminaFill;
        public TMP_Text staminaText;

        [Header("배터리 UI")]
        public Image batteryFill;
        public TMP_Text batteryText;

        [Header("저체력 경고 비네트(선택)")]
        [Tooltip("체력이 낮을 때 붉게 맥동할 Image")]
        public Image lowHealthVignette;
        [Range(0f, 1f)] public float lowHealthThreshold = 0.3f;
        [Range(0f, 1f)] public float lowHealthMaxAlpha = 0.45f;

        private Color lowHpColor = new Color(0.7f, 0f, 0f, 0f);

        private void Start()
        {
            // 지정 안 된 소스는 씬에서 자동으로 찾는다
            if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerStamina == null) playerStamina = FindFirstObjectByType<PlayerStamina>();
            if (flashlight == null) flashlight = FindFirstObjectByType<Flashlight>();

            if (lowHealthVignette != null)
            {
                lowHpColor = lowHealthVignette.color;
                lowHpColor.a = 0f;
                lowHealthVignette.color = lowHpColor;
            }
        }

        private void Update()
        {
            // ── 체력 ──
            if (playerHealth != null)
            {
                float hp = playerHealth.HealthRatio;                 // 0~1
                if (healthFill != null) healthFill.fillAmount = hp;
                if (healthText != null)
                    healthText.text = Mathf.CeilToInt(hp * playerHealth.maxHealth) + " / " + Mathf.CeilToInt(playerHealth.maxHealth);

                // ── 정신력 ──
                float sn = playerHealth.SanityRatio;
                if (sanityFill != null) sanityFill.fillAmount = sn;
                if (sanityText != null) sanityText.text = Mathf.CeilToInt(sn * 100f) + "%";

                // ── 저체력 비네트 맥동 ──
                if (lowHealthVignette != null)
                {
                    float t = hp < lowHealthThreshold ? 1f - (hp / lowHealthThreshold) : 0f; // 0~1
                    float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4f);
                    float targetA = lowHealthMaxAlpha * t * Mathf.Lerp(0.5f, 1f, pulse);
                    lowHpColor.a = Mathf.Lerp(lowHpColor.a, targetA, Time.deltaTime * 8f);
                    lowHealthVignette.color = lowHpColor;
                }
            }

            // ── 기력(스태미나) ──  ★ 이전에 UI 연동이 빠져 있던 부분
            if (playerStamina != null)
            {
                float st = playerStamina.StaminaRatio;               // 0~1
                if (staminaFill != null)
                {
                    staminaFill.fillAmount = st;
                    // 지쳤을 땐 회색빛으로 흐려 표현(선택적 시각 피드백)
                    var c = staminaFill.color;
                    c.a = playerStamina.IsExhausted ? 0.45f : 1f;
                    staminaFill.color = c;
                }
                if (staminaText != null) staminaText.text = Mathf.CeilToInt(st * 100f) + "%";
            }

            // ── 배터리 ──
            if (flashlight != null)
            {
                float bt = flashlight.BatteryRatio;                  // 0~1
                if (batteryFill != null) batteryFill.fillAmount = bt;
                if (batteryText != null) batteryText.text = Mathf.CeilToInt(bt * 100f) + "%";
            }
        }
    }
}
