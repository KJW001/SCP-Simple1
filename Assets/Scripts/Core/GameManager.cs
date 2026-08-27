using UnityEngine;

namespace SCPGame.Core
{
    // ─────────────────────────────────────────────────────────────
    // 게임 전체를 관리하는 매니저(싱글톤).
    // 어디서든 GameManager.Instance 로 접근할 수 있습니다.
    // 커서 잠금, 일시정지, 게임오버 같은 "전역 상태"를 담당합니다.
    // ─────────────────────────────────────────────────────────────
    public class GameManager : MonoBehaviour
    {
        // 싱글톤 인스턴스 (프로그램 전체에서 딱 하나만 존재)
        public static GameManager Instance { get; private set; }

        [Header("게임 상태")]
        [Tooltip("일시정지 여부")]
        public bool isPaused = false;

        [Tooltip("게임오버 여부")]
        public bool isGameOver = false;

        private void Awake()
        {
            // 이미 인스턴스가 있으면 중복 제거 (씬 전환 시 중복 방지)
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // 게임 시작 시 마우스 커서를 화면 중앙에 잠그고 숨김 (FPS 기본)
            LockCursor(true);
        }

        /// <summary>마우스 커서 잠금 여부를 설정</summary>
        public void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        /// <summary>게임 일시정지 토글 (시간 정지 + 커서 표시)</summary>
        public void TogglePause()
        {
            if (isGameOver) return;

            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f; // 0이면 물리/시간이 멈춤
            LockCursor(!isPaused);
        }

        /// <summary>플레이어 사망 등으로 게임오버 처리</summary>
        public void GameOver()
        {
            if (isGameOver) return;

            isGameOver = true;
            Time.timeScale = 0f;
            LockCursor(false);
            Debug.Log("게임 오버! 당신은 격리에 실패했습니다.");
            // TODO: 게임오버 UI 표시, 재시작 버튼 등
        }
    }
}
