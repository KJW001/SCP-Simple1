using UnityEngine;
using TMPro;

namespace SCPGame.UI
{
    // ─────────────────────────────────────────────────────────────
    // 화면 중앙 근처에 "[E] 줍기" 같은 상호작용 안내 문구를 띄웁니다.
    // PlayerInteractor 가 바라보는 대상에 따라 Show/Hide 를 호출합니다.
    //
    // [준비물]
    //  - Canvas 위에 TextMeshPro 텍스트를 만들고 promptText 에 연결
    // ─────────────────────────────────────────────────────────────
    public class InteractionPromptUI : MonoBehaviour
    {
        [Tooltip("안내 문구를 표시할 텍스트")]
        public TMP_Text promptText;

        private void Awake()
        {
            Hide(); // 시작할 땐 숨겨둔다
        }

        /// <summary>안내 문구를 표시</summary>
        public void Show(string message)
        {
            if (promptText == null) return;
            promptText.text = message;
            promptText.gameObject.SetActive(true);
        }

        /// <summary>안내 문구를 숨김</summary>
        public void Hide()
        {
            if (promptText == null) return;
            promptText.gameObject.SetActive(false);
        }
    }
}
