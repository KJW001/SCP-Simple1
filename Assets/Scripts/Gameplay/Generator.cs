using UnityEngine;
using SCPGame.Core;

namespace SCPGame.Gameplay
{
    // ─────────────────────────────────────────────────────────────
    // 발전기. 키카드를 모두 모은 뒤에만 가동할 수 있습니다.
    //  가동을 시작하면 몇 초간 "작동 대기" 상태가 되고, 그동안
    //  소음이 나서 SCP 가 몰려온다는 설정으로 긴장감을 줍니다.
    // ─────────────────────────────────────────────────────────────
    public class Generator : MonoBehaviour, IInteractable
    {
        [Header("가동 설정")]
        [Tooltip("가동 완료까지 걸리는 시간(초)")]
        public float startupTime = 6f;

        [Header("상태 (읽기용)")]
        public bool isRunning = false;
        public bool isStarting = false;

        [Header("연출")]
        [Tooltip("가동되면 켜질 표시등")]
        public Light indicator;

        private float timer;

        public string InteractionPrompt
        {
            get
            {
                if (isRunning) return string.Empty;
                if (isStarting) return "가동 준비 중...";
                if (ObjectiveManager.Instance != null && !ObjectiveManager.Instance.CanUseGenerators)
                    return "[E] 잠김 — 키카드를 모두 모아야 한다";
                return "[E] 발전기 가동";
            }
        }

        public void Interact(GameObject interactor)
        {
            if (isRunning || isStarting) return;
            if (ObjectiveManager.Instance != null && !ObjectiveManager.Instance.CanUseGenerators)
            {
                Debug.Log("키카드가 부족하다.");
                return;
            }
            isStarting = true;
            timer = startupTime;
            Debug.Log(gameObject.name + " 가동 시작... 소음이 발생한다.");
        }

        private void Update()
        {
            if (!isStarting) return;
            timer -= Time.deltaTime;
            if (timer > 0f) return;

            isStarting = false;
            isRunning = true;
            if (indicator != null) { indicator.color = Color.green; indicator.intensity = 3f; }
            if (ObjectiveManager.Instance != null) ObjectiveManager.Instance.AddGenerator();
        }
    }
}
