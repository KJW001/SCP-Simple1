using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SCPGame.Core;
using SCPGame.Gameplay;

namespace SCPGame.UI
{
    // ─────────────────────────────────────────────────────────────
    // 화면에 뜨는 비밀번호 키패드(0~9, 지움, 입력).
    //  KeypadDoor.Interact() 가 Open(door) 를 부르면 나타납니다.
    //
    //  ★ 버튼은 '이름'으로 자동 연결됩니다.
    //     패널 아래에 버튼을 다음 이름으로 두기만 하면 됩니다:
    //       Key_0 ~ Key_9, Key_Clear, Key_Enter, Key_Close
    //     (Inspector 에서 onClick 을 일일이 연결할 필요가 없습니다.)
    //
    //  ★ 키패드가 열려 있는 동안에는 게임을 일시정지(Time.timeScale=0)하고
    //     마우스 커서를 풀어 버튼을 클릭할 수 있게 합니다.
    //     키보드 숫자/엔터/백스페이스/ESC 로도 조작할 수 있습니다.
    // ─────────────────────────────────────────────────────────────
    public class KeypadUI : MonoBehaviour
    {
        public static KeypadUI Instance { get; private set; }

        [Header("참조(비우면 자식에서 자동 검색)")]
        [Tooltip("키패드 패널 루트(껐다 켜짐)")]
        public GameObject panelRoot;
        [Tooltip("입력한 숫자를 보여줄 텍스트")]
        public TMP_Text displayText;
        [Tooltip("문 이름 제목")]
        public TMP_Text titleText;
        [Tooltip("성공/실패 안내")]
        public TMP_Text feedbackText;

        [Header("설정")]
        public int codeLength = 4;

        private KeypadDoor current;
        private string entered = "";

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;

            // 참조 자동 검색
            if (panelRoot == null) panelRoot = gameObject;
        }

        private void Start()
        {
            WireButtons();
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        // ── 패널 아래의 버튼을 이름으로 찾아 리스너 연결 ──
        private void WireButtons()
        {
            var buttons = panelRoot.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                string n = b.gameObject.name;
                if (n.StartsWith("Key_"))
                {
                    string key = n.Substring(4); // "0".."9","Clear","Enter","Close"
                    Button captured = b;
                    if (key == "Clear") captured.onClick.AddListener(OnClear);
                    else if (key == "Enter") captured.onClick.AddListener(OnSubmit);
                    else if (key == "Close") captured.onClick.AddListener(Close);
                    else
                    {
                        int digit;
                        if (int.TryParse(key, out digit))
                            captured.onClick.AddListener(delegate { OnDigit(digit); });
                    }
                }
            }
        }

        // ── 문에서 호출: 키패드 열기 ──
        public void Open(KeypadDoor door)
        {
            if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

            current = door;
            entered = "";
            if (titleText != null) titleText.text = (door != null ? door.doorName : "키패드");
            if (feedbackText != null) feedbackText.text = codeLength + "자리 비밀번호를 입력하세요";
            UpdateDisplay();

            if (panelRoot != null) panelRoot.SetActive(true);
            SetPaused(true);
        }

        public void Close()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            current = null;
            entered = "";
            SetPaused(false);
        }

        private void OnDigit(int d)
        {
            if (entered.Length >= codeLength) return;
            entered += d.ToString();
            UpdateDisplay();
        }

        private void OnClear()
        {
            entered = "";
            if (feedbackText != null) feedbackText.text = codeLength + "자리 비밀번호를 입력하세요";
            UpdateDisplay();
        }

        private void OnSubmit()
        {
            if (current == null) return;
            if (entered.Length < codeLength)
            {
                if (feedbackText != null) feedbackText.text = "<color=#ffcc00>" + codeLength + "자리를 모두 입력하세요</color>";
                return;
            }

            bool ok = current.TryUnlock(entered);
            if (ok)
            {
                if (feedbackText != null) feedbackText.text = "<color=#66ff88>■ 잠금 해제!</color>";
                StartCoroutine(CloseAfter(0.6f));
            }
            else
            {
                if (feedbackText != null) feedbackText.text = "<color=#ff5555>■ 틀렸습니다</color>";
                entered = "";
                UpdateDisplay();
            }
        }

        private IEnumerator CloseAfter(float seconds)
        {
            // 일시정지(timeScale=0) 중에도 흐르는 '실제 시간' 대기
            yield return new WaitForSecondsRealtime(seconds);
            Close();
        }

        private void UpdateDisplay()
        {
            if (displayText == null) return;
            // 입력한 자리는 숫자, 남은 자리는 밑줄로 표시: 3 7 _ _
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < codeLength; i++)
            {
                sb.Append(i < entered.Length ? entered[i].ToString() : "_");
                if (i < codeLength - 1) sb.Append("  ");
            }
            displayText.text = sb.ToString();
        }

        // ── 키패드가 열려 있을 때만: 키보드로도 입력 가능 ──
        private void Update()
        {
            if (current == null) return; // 닫혀 있으면 무시

            for (int d = 0; d <= 9; d++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + d) || Input.GetKeyDown(KeyCode.Keypad0 + d))
                    OnDigit(d);
            }
            if (Input.GetKeyDown(KeyCode.Backspace)) OnClear();
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) OnSubmit();
            if (Input.GetKeyDown(KeyCode.Escape)) Close();
        }

        // ── 일시정지 + 커서 잠금/해제 ──
        private void SetPaused(bool paused)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.isPaused = paused;
                GameManager.Instance.LockCursor(!paused);
            }
            else
            {
                Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = paused;
            }
            Time.timeScale = paused ? 0f : 1f;
        }
    }
}
