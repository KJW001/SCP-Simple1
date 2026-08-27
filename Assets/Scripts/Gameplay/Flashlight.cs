using UnityEngine;

namespace SCPGame.Player
{
    // ─────────────────────────────────────────────────────────────
    // 배터리가 닳는 손전등. 공포게임의 핵심 압박 장치입니다.
    //  [F] 로 켜고 끄며, 켜져 있는 동안 배터리가 줄어듭니다.
    //  배터리가 적으면 깜빡이기 시작합니다.
    // ─────────────────────────────────────────────────────────────
    public class Flashlight : MonoBehaviour
    {
        [Header("참조")]
        [Tooltip("손전등 Light 컴포넌트")]
        public Light spot;

        [Header("배터리")]
        [Tooltip("최대 배터리 (초 단위)")]
        public float maxBattery = 180f;
        public float battery = 180f;
        [Tooltip("초당 소모량")]
        public float drainPerSecond = 1f;

        [Header("입력")]
        public KeyCode toggleKey = KeyCode.F;

        [Header("깜빡임")]
        [Tooltip("이 비율 아래로 내려가면 깜빡인다")]
        public float flickerBelow = 0.25f;

        private bool isOn = true;
        private float baseIntensity;
        private float flickerTimer;

        // 다른 스크립트/UI가 참조할 값
        public float BatteryRatio { get { return maxBattery <= 0f ? 0f : battery / maxBattery; } }
        public bool IsOn { get { return isOn && battery > 0f; } }

        private void Awake()
        {
            if (spot == null) spot = GetComponent<Light>();
            if (spot != null) baseIntensity = spot.intensity;
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey)) isOn = !isOn;

            if (isOn && battery > 0f)
            {
                battery -= drainPerSecond * Time.deltaTime;
                if (battery < 0f) battery = 0f;
            }

            if (spot == null) return;
            spot.enabled = IsOn;
            if (!IsOn) return;

            // 배터리가 얼마 안 남으면 불안하게 깜빡인다
            if (BatteryRatio < flickerBelow)
            {
                flickerTimer += Time.deltaTime * 18f;
                float n = Mathf.PerlinNoise(flickerTimer, 0f);      // 0~1 자연스러운 노이즈
                float t = Mathf.InverseLerp(flickerBelow, 0f, BatteryRatio); // 0->1
                spot.intensity = baseIntensity * Mathf.Lerp(1f, n, t);
            }
            else spot.intensity = baseIntensity;
        }

        /// <summary>배터리 아이템 사용 시 호출</summary>
        public void Recharge(float amount)
        {
            battery = Mathf.Min(maxBattery, battery + amount);
        }
    }
}
