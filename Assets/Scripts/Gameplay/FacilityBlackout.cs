using UnityEngine;
using SCPGame.Core;
using SCPGame.Player;

namespace SCPGame.Gameplay
{
    // ─────────────────────────────────────────────────────────────
    // 주기적으로 시설 전체 정전을 일으킵니다.
    //  정전 중에는 조명이 꺼지고 정신력이 빠르게 깎여, 손전등에
    //  의존하게 만듭니다. 공포 연출의 핵심 장치입니다.
    // ─────────────────────────────────────────────────────────────
    public class FacilityBlackout : MonoBehaviour
    {
        [Header("주기")]
        [Tooltip("정전까지의 최소/최대 간격(초)")]
        public float minInterval = 45f;
        public float maxInterval = 90f;
        [Tooltip("정전 지속 시간(초)")]
        public float blackoutDuration = 12f;

        [Header("영향")]
        [Tooltip("정전 중 초당 추가 정신력 감소")]
        public float sanityDrain = 4f;

        private Light[] facilityLights;
        private float[] savedIntensity;
        private float timer;
        private bool blackout = false;
        private float blackoutTimer;
        private PlayerHealth health;

        private void Start()
        {
            health = FindFirstObjectByType<PlayerHealth>();
            CollectLights();
            timer = Random.Range(minInterval, maxInterval);
        }

        // 시설 조명만 모은다 (플레이어 손전등은 제외해야 함!)
        private void CollectLights()
        {
            var all = FindObjectsOfType<Light>();
            var list = new System.Collections.Generic.List<Light>();
            foreach (var l in all)
            {
                if (l.type == LightType.Directional) continue;              // 태양광 제외
                if (l.GetComponentInParent<Flashlight>() != null) continue; // 손전등 제외
                list.Add(l);
            }
            facilityLights = list.ToArray();
            savedIntensity = new float[facilityLights.Length];
            for (int i = 0; i < facilityLights.Length; i++)
                savedIntensity[i] = facilityLights[i].intensity;
        }

        private void Update()
        {
            if (blackout)
            {
                blackoutTimer -= Time.deltaTime;
                if (health != null) health.ReduceSanity(sanityDrain * Time.deltaTime);
                if (blackoutTimer <= 0f) EndBlackout();
                return;
            }

            timer -= Time.deltaTime;
            if (timer <= 0f) StartBlackout();
        }

        private void StartBlackout()
        {
            blackout = true;
            PlayerState.IsBlackout = true;
            blackoutTimer = blackoutDuration;
            for (int i = 0; i < facilityLights.Length; i++)
                if (facilityLights[i] != null) facilityLights[i].intensity = 0f;
            Debug.Log("정전! 비상 전력 복구까지 " + blackoutDuration + "초");
        }

        private void EndBlackout()
        {
            blackout = false;
            PlayerState.IsBlackout = false;
            for (int i = 0; i < facilityLights.Length; i++)
                if (facilityLights[i] != null) facilityLights[i].intensity = savedIntensity[i];
            timer = Random.Range(minInterval, maxInterval);
            Debug.Log("전력 복구됨.");
        }
    }
}
