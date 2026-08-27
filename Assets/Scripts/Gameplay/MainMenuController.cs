using UnityEngine;
using UnityEngine.SceneManagement;

namespace SCPGame.Gameplay
{
    // ─────────────────────────────────────────────────────────────
    // 시작(타이틀) 화면 컨트롤러.
    //  - [게임 시작] : 게임 씬 로드
    //  - [조작법]    : 조작 안내 패널 토글
    //  - [종료]      : 게임 종료
    // ─────────────────────────────────────────────────────────────
    public class MainMenuController : MonoBehaviour
    {
        [Header("게임 씬 이름")]
        public string gameSceneName = "SCP_Facility";

        [Header("패널")]
        public GameObject howToPanel;   // 조작법 패널

        private void Start()
        {
            // 메뉴에서는 커서를 보이게
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 1f;
            if (howToPanel != null) howToPanel.SetActive(false);
        }

        // 버튼: 게임 시작
        public void StartGame()
        {
            SceneManager.LoadScene(gameSceneName);
        }

        // 버튼: 조작법 열기/닫기
        public void ToggleHowTo()
        {
            if (howToPanel != null) howToPanel.SetActive(!howToPanel.activeSelf);
        }

        // 버튼: 종료
        public void QuitGame()
        {
            Debug.Log("게임 종료");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
