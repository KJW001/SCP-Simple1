using UnityEngine;
using UnityEngine.UI;
using SCPGame.SCP;

namespace SCPGame.UI
{
    // ─────────────────────────────────────────────────────────────
    // SCP-096 을 응시하는 동안 화면 상단에 경고 게이지를 표시한다.
    //  - 어떤 096 이든 응시 진행도가 가장 높은 값을 따라간다.
    //  - 진행도가 0 이면 숨기고, 조금이라도 쌓이면 나타난다.
    //  - 격노가 시작되면(진행도 1) 게이지를 숨긴다(이미 늦었으므로).
    // ─────────────────────────────────────────────────────────────
    public class GazeWarningUI : MonoBehaviour
    {
        [Tooltip("게이지 채우기 이미지")]
        public Image fill;
        [Tooltip("경고 UI 루트(켜고 끌 대상). 비우면 이 오브젝트를 사용")]
        public GameObject root;

        private Scp096Entity[] shyGuys;

        private void Start()
        {
            if (root == null) root = gameObject;
            shyGuys = Object.FindObjectsByType<Scp096Entity>(FindObjectsSortMode.None);
            root.SetActive(false);
        }

        private void Update()
        {
            if (shyGuys == null || shyGuys.Length == 0) return;

            // 응시 진행도가 가장 높은 096 을 찾는다
            float maxProgress = 0f;
            foreach (var s in shyGuys)
            {
                if (s == null || s.isEnraged) continue;      // 이미 격노한 개체는 제외
                if (s.GazeProgress > maxProgress) maxProgress = s.GazeProgress;
            }

            // 진행 중이면 표시, 아니면 숨김
            bool show = maxProgress > 0.01f && maxProgress < 1f;
            if (root.activeSelf != show) root.SetActive(show);
            if (show && fill != null) fill.fillAmount = maxProgress;
        }
    }
}
