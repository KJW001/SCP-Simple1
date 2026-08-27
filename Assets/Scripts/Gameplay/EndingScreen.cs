using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using SCPGame.Core;

namespace SCPGame.Gameplay
{
    // ─────────────────────────────────────────────────────────────
    // 게임 종료 화면 (엔딩).
    //  - 승리: 탈출 성공 (초록)
    //  - 패배: 사망      (빨강)
    //  [R] 다시 시작 / [M] 메뉴로.
    // ─────────────────────────────────────────────────────────────
    public class EndingScreen : MonoBehaviour
    {
        [Header("참조")]
        public CanvasGroup panel;
        public TMP_Text titleText;
        public TMP_Text subText;

        [Header("씬 이름")]
        public string menuSceneName = "MainMenu";

        private bool showing = false;
        private float fade = 0f;

        private void Start()
        {
            if (panel != null) { panel.alpha = 0f; panel.gameObject.SetActive(false); }
            if (ObjectiveManager.Instance != null)
                ObjectiveManager.Instance.OnObjectiveChanged += CheckEscape;
            var ph = FindFirstObjectByType<SCPGame.Player.PlayerHealth>();
            if (ph != null) ph.OnDied += ShowDefeat;
        }

        private void OnDestroy()
        {
            if (ObjectiveManager.Instance != null)
                ObjectiveManager.Instance.OnObjectiveChanged -= CheckEscape;
        }

        private void CheckEscape(string _)
        {
            if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.stage == Stage.Escaped)
                ShowVictory();
        }

        public void ShowVictory()
        {
            Show("탈출 성공", "당신은 시설을 빠져나왔다.\n\n[R] 다시 시작        [M] 메뉴로",
                 new Color(0.12f, 0.45f, 0.22f, 0.94f), new Color(0.4f, 1f, 0.5f));
        }

        public void ShowDefeat()
        {
            Show("사 망", "당신은 격리에 실패했다.\n\n[R] 다시 시작        [M] 메뉴로",
                 new Color(0.45f, 0.09f, 0.09f, 0.94f), new Color(1f, 0.4f, 0.4f));
        }

        private void Show(string title, string sub, Color bg, Color titleCol)
        {
            if (showing) return;
            showing = true;
            if (panel != null)
            {
                panel.gameObject.SetActive(true);
                var img = panel.GetComponent<Image>();
                if (img != null) img.color = bg;
            }
            if (titleText != null) { titleText.text = title; titleText.color = titleCol; }
            if (subText != null) subText.text = sub;
            if (GameManager.Instance != null) GameManager.Instance.LockCursor(false);
        }

        private void Update()
        {
            if (!showing) return;
            if (panel != null && fade < 1f)
            {
                fade += Time.unscaledDeltaTime * 1.5f;
                panel.alpha = Mathf.Clamp01(fade);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                Time.timeScale = 1f;
                PlayerState.Reset();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                Time.timeScale = 1f;
                PlayerState.Reset();
                SceneManager.LoadScene(menuSceneName);
            }
        }
    }
}
