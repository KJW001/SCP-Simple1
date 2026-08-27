using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SCPGame.Core;
using SCPGame.Player;

namespace SCPGame.Gameplay
{
    // ─────────────────────────────────────────────────────────────
    // 목표 문구, 알림 메시지, 스태미나/배터리 게이지를 표시합니다.
    // ObjectiveManager 의 이벤트를 구독해 자동으로 갱신됩니다.
    // ─────────────────────────────────────────────────────────────
    public class ObjectiveUI : MonoBehaviour
    {
        [Header("텍스트")]
        public TMP_Text objectiveText;   // 상시 표시되는 현재 목표
        public TMP_Text notifyText;      // 잠깐 떴다 사라지는 알림

        [Header("게이지")]
        public Image staminaFill;
        public TMPro.TMP_Text staminaText;   // 기력 수치
        public Image batteryFill;
        public TMPro.TMP_Text batteryText;   // 배터리 수치

        [Header("참조")]
        public PlayerStamina stamina;
        public Flashlight flashlight;

        private float notifyTimer;

        private void Start()
        {
            if (stamina == null)   stamina = FindFirstObjectByType<PlayerStamina>();
            if (flashlight == null) flashlight = FindFirstObjectByType<Flashlight>();

            var om = ObjectiveManager.Instance;
            if (om != null)
            {
                om.OnObjectiveChanged += SetObjective;
                om.OnNotify += Notify;
            }
            if (notifyText != null) notifyText.text = string.Empty;
        }

        private void OnDestroy()
        {
            var om = ObjectiveManager.Instance;
            if (om != null)
            {
                om.OnObjectiveChanged -= SetObjective;
                om.OnNotify -= Notify;
            }
        }

        private void Update()
        {
            if (staminaFill != null && stamina != null) staminaFill.fillAmount = stamina.StaminaRatio;
            if (staminaText != null && stamina != null) staminaText.text = Mathf.CeilToInt(stamina.StaminaRatio * 100f).ToString();
            if (batteryFill != null && flashlight != null) batteryFill.fillAmount = flashlight.BatteryRatio;
            if (batteryText != null && flashlight != null) batteryText.text = Mathf.CeilToInt(flashlight.BatteryRatio * 100f).ToString() + "%";

            // 알림 문구를 몇 초 뒤 서서히 지운다
            if (notifyTimer > 0f)
            {
                notifyTimer -= Time.unscaledDeltaTime;
                if (notifyTimer <= 0f && notifyText != null) notifyText.text = string.Empty;
            }
        }

        private void SetObjective(string t)
        {
            if (objectiveText != null) objectiveText.text = "목표 :  " + t;
        }

        private void Notify(string t)
        {
            if (notifyText == null) return;
            notifyText.text = t;
            notifyTimer = 3.5f;
        }
    }
}
