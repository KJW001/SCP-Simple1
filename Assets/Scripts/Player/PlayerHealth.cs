using System;
using UnityEngine;
using SCPGame.Core;

namespace SCPGame.Player
{
    // ─────────────────────────────────────────────────────────────
    // 플레이어의 체력(Health)과 정신력(Sanity)을 관리합니다.
    // - 체력이 0이 되면 사망 → 게임오버
    // - 정신력은 SCP를 오래 바라보거나 어둠에 있으면 감소 (공포 연출)
    //
    // IDamageable 을 구현하므로 SCP가 TakeDamage() 로 피해를 줄 수 있습니다.
    // ─────────────────────────────────────────────────────────────
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("체력")]
        public float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        [Header("정신력 (SANITY)")]
        [Tooltip("최대 정신력")]
        public float maxSanity = 100f;
        [SerializeField] private float currentSanity;
        [Tooltip("초당 자연 회복량")]
        public float sanityRegenPerSecond = 1.5f;

        [Header("체력 자동 회복")]
        [Tooltip("피격 후 이 시간이 지나면 서서히 회복 시작")]
        public float regenDelay = 6f;
        [Tooltip("초당 체력 회복량")]
        public float healthRegenPerSecond = 4f;

        private float lastDamageTime; // 마지막으로 피해받은 시각

        // 다른 스크립트(UI, VFX, 사운드)가 값 변화를 감지할 수 있게 이벤트 제공
        // (예: 체력이 바뀌면 → HUD가 자동으로 갱신)
        public event Action<float, float> OnHealthChanged; // (현재, 최대)
        public event Action<float, float> OnSanityChanged; // (현재, 최대)
        public event Action OnDamaged;                     // 피격 순간
        public event Action OnDied;                        // 사망 순간

        // 인터페이스 요구사항: 살아있는지
        public bool IsAlive => currentHealth > 0f;

        // 0~1 비율값 (UI 게이지에서 사용하기 편함)
        public float HealthRatio => currentHealth / maxHealth;
        public float SanityRatio => currentSanity / maxSanity;

        private void Start()
        {
            currentHealth = maxHealth;
            currentSanity = maxSanity;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnSanityChanged?.Invoke(currentSanity, maxSanity);
        }

        private void Update()
        {
            RegenerateHealth();
            RegenerateSanity();
        }

        // ── IDamageable 구현: 피해 받기 ──
        public void TakeDamage(float damage, GameObject attacker = null)
        {
            if (!IsAlive) return;

            currentHealth = Mathf.Max(0f, currentHealth - damage);
            lastDamageTime = Time.time;

            OnDamaged?.Invoke();
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            Debug.Log($"플레이어가 {damage} 피해를 입음 (남은 체력: {currentHealth})");

            if (currentHealth <= 0f)
                Die();
        }

        // ── 정신력 감소 (SCP 근접, 시야 등 외부에서 호출) ──
        public void ReduceSanity(float amount)
        {
            currentSanity = Mathf.Max(0f, currentSanity - amount);
            OnSanityChanged?.Invoke(currentSanity, maxSanity);

            // 정신력이 바닥나면 체력이 갉히는 패널티 (공포 압박)
            if (currentSanity <= 0f)
                TakeDamage(amount * 0.5f);
        }

        // ── 체력 회복 아이템 등에서 호출 ──
        public void Heal(float amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        // ── 시간이 지나면 체력이 서서히 회복 ──
        private void RegenerateHealth()
        {
            if (!IsAlive) return;
            if (Time.time - lastDamageTime < regenDelay) return; // 아직 회복 대기 중
            if (currentHealth >= maxHealth) return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + healthRegenPerSecond * Time.deltaTime);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        // ── 정신력은 안전할 때 서서히 회복 ──
        private void RegenerateSanity()
        {
            if (currentSanity >= maxSanity) return;
            currentSanity = Mathf.Min(maxSanity, currentSanity + sanityRegenPerSecond * Time.deltaTime);
            OnSanityChanged?.Invoke(currentSanity, maxSanity);
        }

        // ── 사망 처리 ──
        private void Die()
        {
            OnDied?.Invoke();
            Debug.Log("플레이어 사망.");
            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
        }
    }
}
