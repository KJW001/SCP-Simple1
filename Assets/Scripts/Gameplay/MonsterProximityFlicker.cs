using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SCPGame.SCP;
using SCPGame.Player;

namespace SCPGame.Gameplay
{
    // ─────────────────────────────────────────────────────────────
    // 몬스터가 플레이어에게 가까워지면 주변 조명이 깜빡이는 연출.
    //  - 가장 가까운 SCP 와 플레이어의 거리를 재고,
    //    triggerRange 안으로 들어오면 플레이어 주변(flickerRadius) 조명이
    //    지지직 깜빡입니다. 가까울수록 더 심하게 깜빡입니다.
    //  - 화면 가장자리에 붉은 위험 비네트가 함께 맥동합니다(선택).
    //  - 심장박동 오디오(선택)도 가까울수록 커지고 빨라집니다.
    //
    //  성능: 몬스터 탐색·대상 조명 선정은 scanInterval 마다만 하고,
    //        실제 밝기 흔들기는 매 프레임 부드럽게 적용합니다.
    // ─────────────────────────────────────────────────────────────
    public class MonsterProximityFlicker : MonoBehaviour
    {
        [Header("반응 거리")]
        [Tooltip("가장 가까운 몬스터가 이 거리 안이면 깜빡임 시작")]
        public float triggerRange = 14f;
        [Tooltip("플레이어 기준 이 반경 안의 조명만 깜빡인다")]
        public float flickerRadius = 16f;

        [Header("깜빡임 세기")]
        [Tooltip("가장 어두워질 때의 밝기 배율(0=완전 소등)")]
        [Range(0f, 1f)] public float minIntensityMul = 0.15f;
        [Tooltip("깜빡임 속도")]
        public float flickerSpeed = 22f;

        [Header("위험 비네트(선택)")]
        [Tooltip("화면 가장자리에 깔 붉은 Image (없으면 생략)")]
        public Image dangerVignette;
        [Tooltip("가장 가까울 때 비네트 최대 알파")]
        [Range(0f, 1f)] public float vignetteMaxAlpha = 0.5f;

        [Header("심장박동(선택)")]
        public AudioSource heartbeat;
        public float heartbeatMaxVolume = 0.9f;

        [Header("스캔 주기")]
        public float scanInterval = 0.15f;

        // ── 내부 상태 ──
        private Transform player;
        private readonly List<Light> allLights = new List<Light>();
        private readonly List<float> baseIntensity = new List<float>();
        private readonly List<Light> affected = new List<Light>();

        private float scanTimer;
        private float closeness;    // 0(안전)~1(코앞) — 가까울수록 1
        private float flickerTimer;
        private Color vignetteColorCache = new Color(0.6f, 0f, 0f, 0f);

        private void Start()
        {
            var ph = FindFirstObjectByType<PlayerHealth>();
            if (ph != null) player = ph.transform;

            // 씬의 모든 Light 를 캐싱 (손전등 = 플레이어 자식 은 제외)
            var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var l in lights)
            {
                if (player != null && l.transform.IsChildOf(player)) continue; // 손전등 제외
                allLights.Add(l);
                baseIntensity.Add(l.intensity);
            }

            if (dangerVignette != null)
            {
                vignetteColorCache = dangerVignette.color;
                vignetteColorCache.a = 0f;
                dangerVignette.color = vignetteColorCache;
            }
        }

        private void Update()
        {
            if (player == null) return;

            // ── 주기적 스캔: 가장 가까운 몬스터 거리 + 대상 조명 선정 ──
            scanTimer -= Time.deltaTime;
            if (scanTimer <= 0f)
            {
                scanTimer = scanInterval;
                Rescan();
            }

            // ── 매 프레임: 부드러운 깜빡임 적용 ──
            ApplyFlicker();
            ApplyVignette();
            ApplyHeartbeat();
        }

        private void Rescan()
        {
            // 가장 가까운 몬스터 거리 찾기
            float nearest = float.MaxValue;
            var monsters = FindObjectsByType<SCPEntity>(FindObjectsSortMode.None);
            foreach (var m in monsters)
            {
                if (m == null) continue;
                float d = Vector3.Distance(player.position, m.transform.position);
                if (d < nearest) nearest = d;
            }

            // 거리 → 0~1 근접도 (triggerRange 밖=0, 코앞=1)
            closeness = nearest >= triggerRange ? 0f : 1f - (nearest / triggerRange);

            // 대상 조명 갱신 (플레이어 주변만)
            affected.Clear();
            if (closeness > 0f)
            {
                for (int i = 0; i < allLights.Count; i++)
                {
                    var l = allLights[i];
                    if (l == null) continue;
                    if (Vector3.Distance(player.position, l.transform.position) <= flickerRadius)
                        affected.Add(l);
                }
            }
        }

        private void ApplyFlicker()
        {
            if (closeness <= 0f)
            {
                // 안전: 모든 조명을 원래 밝기로 복원
                for (int i = 0; i < allLights.Count; i++)
                {
                    if (allLights[i] == null) continue;
                    allLights[i].intensity = Mathf.Lerp(allLights[i].intensity, baseIntensity[i], Time.deltaTime * 6f);
                }
                return;
            }

            flickerTimer += Time.deltaTime * flickerSpeed;
            // 노이즈로 자연스러운 깜빡임 (0~1)
            float n = Mathf.PerlinNoise(flickerTimer, 0.5f);
            // 가까울수록 minIntensityMul 쪽으로 더 깊게 떨어짐
            float floor = Mathf.Lerp(1f, minIntensityMul, closeness);
            float mul = Mathf.Lerp(floor, 1f, n);

            for (int i = 0; i < allLights.Count; i++)
            {
                var l = allLights[i];
                if (l == null) continue;
                float baseI = baseIntensity[i];
                if (affected.Contains(l))
                    l.intensity = baseI * mul;      // 플레이어 주변 조명: 깜빡임
                else
                    l.intensity = Mathf.Lerp(l.intensity, baseI, Time.deltaTime * 6f); // 그 외: 원래대로
            }
        }

        private void ApplyVignette()
        {
            if (dangerVignette == null) return;
            float targetA = vignetteMaxAlpha * closeness;
            // 맥동: 가까울수록 빠르게 두근거림
            if (closeness > 0f)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * (3f + closeness * 6f));
                targetA *= Mathf.Lerp(0.55f, 1f, pulse);
            }
            vignetteColorCache.a = Mathf.Lerp(vignetteColorCache.a, targetA, Time.deltaTime * 8f);
            dangerVignette.color = vignetteColorCache;
        }

        private void ApplyHeartbeat()
        {
            if (heartbeat == null) return;
            if (closeness > 0.05f)
            {
                if (!heartbeat.isPlaying) { heartbeat.loop = true; heartbeat.Play(); }
                heartbeat.volume = Mathf.Lerp(heartbeat.volume, heartbeatMaxVolume * closeness, Time.deltaTime * 5f);
                heartbeat.pitch = Mathf.Lerp(0.9f, 1.5f, closeness);
            }
            else
            {
                heartbeat.volume = Mathf.Lerp(heartbeat.volume, 0f, Time.deltaTime * 5f);
                if (heartbeat.volume < 0.02f && heartbeat.isPlaying) heartbeat.Stop();
            }
        }
    }
}
