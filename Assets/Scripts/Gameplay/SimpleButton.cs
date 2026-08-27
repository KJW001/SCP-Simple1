using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SCPGame.Gameplay
{
    // ─────────────────────────────────────────────────────────────
    // 마우스를 올리면 색이 밝아지는 간단한 버튼 효과.
    // (코드로 UI를 만들 때 호버 피드백을 주기 위함)
    // ─────────────────────────────────────────────────────────────
    public class SimpleButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image target;
        public Color normal = new Color(0.12f, 0.14f, 0.18f, 0.9f);
        public Color hover  = new Color(0.25f, 0.45f, 0.35f, 0.95f);

        private void Start()
        {
            if (target == null) target = GetComponent<Image>();
            if (target != null) target.color = normal;
        }
        public void OnPointerEnter(PointerEventData e) { if (target != null) target.color = hover; }
        public void OnPointerExit(PointerEventData e)  { if (target != null) target.color = normal; }
    }
}
