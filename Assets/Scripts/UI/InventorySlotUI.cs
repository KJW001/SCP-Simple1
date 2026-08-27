using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SCPGame.Items;

namespace SCPGame.UI
{
    // ─────────────────────────────────────────────────────────────
    // 인벤토리 한 칸(슬롯)을 그리는 UI 버튼입니다.
    //  아이콘이 없을 땐 아이템 "이름"을 텍스트로 대신 표시합니다.
    //  클릭하면 그 칸의 아이템을 사용합니다.
    // ─────────────────────────────────────────────────────────────
    public class InventorySlotUI : MonoBehaviour
    {
        public Image iconImage;      // 아이템 아이콘 (없을 수 있음)
        public TMP_Text countText;   // 개수 텍스트
        public TMP_Text nameText;    // ★ 아이템 이름 텍스트 (아이콘 대체)
        public Button button;        // 클릭 처리용 버튼

        private InventoryUI owner;
        private int slotIndex;

        public void Init(InventoryUI owner, int index)
        {
            this.owner = owner;
            this.slotIndex = index;
            if (button != null)
                button.onClick.AddListener(OnClick);
        }

        public void Refresh(InventorySlot slot)
        {
            bool empty = slot == null || slot.IsEmpty;

            // 아이콘: 스프라이트가 있을 때만 표시
            if (iconImage != null)
            {
                bool hasIcon = !empty && slot.item.icon != null;
                iconImage.enabled = hasIcon;
                if (hasIcon) iconImage.sprite = slot.item.icon;
            }

            // ★ 이름 텍스트: 비어있지 않으면 아이템 이름 표시
            if (nameText != null)
            {
                nameText.text = empty ? string.Empty : slot.item.displayName;
            }

            // 개수: 2개 이상일 때만
            if (countText != null)
            {
                countText.text = (!empty && slot.count > 1) ? ("x" + slot.count.ToString()) : string.Empty;
            }
        }

        private void OnClick()
        {
            owner.OnSlotClicked(slotIndex);
        }
    }
}
