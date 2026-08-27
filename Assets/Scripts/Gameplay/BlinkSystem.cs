using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SCPGame.Gameplay
{
    // ─────────────────────────────────────────────────────────────
    // 눈 깜빡임 시스템.
    //  - 20초마다 자동으로 한 번 깜빡인다 (화면이 잠깐 검게)
    //  - [Q] 로 직접 깜빡이면 타이머가 초기화된다
    //  - UI 로 다음 깜빡임까지 남은 시간을 보여준다
    //
    //  공포게임에서 "깜빡이는 순간"은 SCP-173 같은 개체가 움직이는
    //  빈틈이 됩니다. (지금은 연출 위주, 원하면 173 연동 가능)
    // ─────────────────────────────────────────────────────────────
    public class BlinkSystem : MonoBehaviour
    {
        [Header("타이밍")]
        [Tooltip("자동 깜빡임 간격(초)")]
        public float blinkInterval = 20f;
        [Tooltip("눈 감고 있는 시간(초)")]
        public float blinkDuration = 0.32f;

        [Header("입력")]
        public KeyCode blinkKey = KeyCode.Q;

        [Header("UI 참조")]
        [Tooltip("화면을 덮는 검은 이미지 (눈꺼풀)")]
        public Image eyelid;
        [Tooltip("남은 시간 게이지")]
        public Image timerFill;
        [Tooltip("남은 시간 텍스트")]
        public TMP_Text timerText;

        private float timer;
        private bool blinking = false;
        private float blinkT = 0f;

        // 지금 눈을 감고 있는가 (다른 시스템이 참조 가능 - 예: 173)
        public bool IsBlinking { get { return blinking; } }

        private void Start()
        {
            timer = blinkInterval;
            if (eyelid != null) SetEyelid(0f);
        }

        private void Update()
        {
            // 수동 깜빡임
            if (Input.GetKeyDown(blinkKey) && !blinking)
            {
                DoBlink();
                timer = blinkInterval;   // 타이머 초기화
            }

            // 자동 깜빡임 카운트다운 (일시정지 중엔 진행 안 함)
            if (!blinking)
            {
                timer -= Time.deltaTime;
                if (timer <= 0f) { DoBlink(); timer = blinkInterval; }
            }

            // 깜빡임 애니메이션 (감았다 뜨기)
            // ★ unscaledDeltaTime 사용: timeScale=0(키패드 등 일시정지) 상태에서도
            //   깜빡임이 실제 시간으로 반드시 끝나, 눈 감은 채로 멈춰버리지 않게 한다.
            if (blinking)
            {
                blinkT += Time.unscaledDeltaTime;
                float half = blinkDuration * 0.5f;
                float a;
                if (blinkT < half) a = blinkT / half;            // 감기 0->1
                else a = 1f - (blinkT - half) / half;            // 뜨기 1->0
                SetEyelid(Mathf.Clamp01(a));
                if (blinkT >= blinkDuration) { blinking = false; SetEyelid(0f); }
            }

            UpdateUI();
        }

        private void DoBlink()
        {
            blinking = true;
            blinkT = 0f;
        }

        private void SetEyelid(float a)
        {
            if (eyelid == null) return;
            var c = eyelid.color; c.a = a; eyelid.color = c;
        }

        private void UpdateUI()
        {
            float ratio = Mathf.Clamp01(timer / blinkInterval);
            if (timerFill != null) timerFill.fillAmount = ratio;
            if (timerText != null) timerText.text = "깜빡임 " + Mathf.CeilToInt(timer) + "s";
        }
    }
}
