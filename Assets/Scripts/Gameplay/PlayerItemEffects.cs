using UnityEngine;
using SCPGame.Core;
using SCPGame.Items;
using SCPGame.Player;

namespace SCPGame.Gameplay
{
    // ─────────────────────────────────────────────────────────────
    // 인벤토리 변화를 감시해서
    //  - 키카드 개수를 세어 목표에 반영하고
    //  - 배터리 아이템을 쓰면 손전등을 충전합니다.
    //
    // 기존 Inventory 코드를 크게 고치지 않고 "바깥에서 관찰"하는
    // 방식이라, 역할 분리 예제로도 좋습니다.
    // ─────────────────────────────────────────────────────────────
    [RequireComponent(typeof(Inventory))]
    public class PlayerItemEffects : MonoBehaviour
    {
        [Header("배터리")]
        [Tooltip("배터리 아이템의 itemId")]
        public string batteryItemId = "battery";
        [Tooltip("한 개당 충전량(초)")]
        public float batteryAmount = 60f;

        private Inventory inv;
        private Flashlight flash;
        private int lastKeycardCount = 0;

        private void Awake()
        {
            inv = GetComponent<Inventory>();
            flash = GetComponentInChildren<Flashlight>();
        }

        private void OnEnable()  { if (inv != null) { inv.OnInventoryChanged += OnChanged; inv.OnItemUsed += OnItemUsed; } }
        private void OnDisable() { if (inv != null) { inv.OnInventoryChanged -= OnChanged; inv.OnItemUsed -= OnItemUsed; } }

        private void OnChanged()
        {
            // 인벤토리 안의 키카드(KeyItem) 개수를 센다
            int cards = 0;
            foreach (var slot in inv.slots)
                if (!slot.IsEmpty && slot.item.itemType == ItemType.KeyItem)
                    cards += slot.count;

            // 늘어난 만큼만 목표에 반영
            if (cards > lastKeycardCount && ObjectiveManager.Instance != null)
            {
                int gained = cards - lastKeycardCount;
                for (int i = 0; i < gained; i++) ObjectiveManager.Instance.AddKeycard();
            }
            lastKeycardCount = cards;
        }

        /// <summary>배터리 아이템 사용 시 Inventory 가 호출</summary>
        public void OnItemUsed(ItemData item)
        {
            if (item == null || flash == null) return;
            if (item.itemId == batteryItemId)
            {
                flash.Recharge(batteryAmount);
                Debug.Log("손전등 배터리를 교체했다.");
            }
        }
    }
}
