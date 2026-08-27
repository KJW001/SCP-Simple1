using System;
using System.Collections.Generic;
using UnityEngine;
using SCPGame.Player;

namespace SCPGame.Items
{
    // ★ 네이밍 주의 (자주 나오는 컴파일 오류)
    //   이 네임스페이스를 'SCPGame.Inventory' 로 두면
    //   안에 있는 Inventory 클래스와 이름이 겹쳐서
    //   다른 네임스페이스(예: SCPGame.UI)에서 쓸 때 오류가 납니다.
    //     CS0118: 'Inventory'은(는) 네임스페이스이지만 형식처럼 사용됩니다.
    //   → C#은 'using' 보다 네임스페이스 이름을 먼저 찾기 때문입니다.
    //   그래서 네임스페이스를 'SCPGame.Items' 로 바꿨습니다.
    //   (규칙: 네임스페이스와 그 안의 클래스 이름을 같게 짓지 말 것)

    // ─────────────────────────────────────────────────────────────
    // 인벤토리에 담기는 한 "칸(슬롯)"을 표현합니다.
    // 같은 아이템은 한 슬롯에 개수(count)를 쌓아서 보관합니다.
    // ─────────────────────────────────────────────────────────────
    [Serializable]
    public class InventorySlot
    {
        public ItemData item;  // 무슨 아이템인지 (비어있으면 null)
        public int count;      // 몇 개인지

        public bool IsEmpty => item == null || count <= 0;

        public InventorySlot(ItemData item, int count)
        {
            this.item = item;
            this.count = count;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // 플레이어의 인벤토리 본체.
    //  - 정해진 칸 수(slotCount)만큼 아이템을 보관
    //  - 아이템 추가/제거/사용 기능 제공
    //  - 변화가 생기면 OnInventoryChanged 이벤트로 UI에 알림
    // ─────────────────────────────────────────────────────────────
    public class Inventory : MonoBehaviour
    {
        [Header("인벤토리 설정")]
        [Tooltip("보유 가능한 칸 수")]
        public int slotCount = 12;

        // 실제 아이템이 담기는 리스트 (칸 단위)
        public List<InventorySlot> slots = new List<InventorySlot>();

        // 인벤토리가 바뀔 때마다 호출 → UI가 이걸 구독해서 다시 그린다
        public event Action OnInventoryChanged;

        // 아이템을 사용했을 때 알림 (배터리 등 특수 효과 처리용)
        public event Action<ItemData> OnItemUsed;

        private PlayerHealth playerHealth; // 소모품 사용 시 체력/정신력 회복용

        private void Awake()
        {
            playerHealth = GetComponent<PlayerHealth>();

            // 칸을 미리 빈 슬롯으로 채워둔다
            slots.Clear();
            for (int i = 0; i < slotCount; i++)
                slots.Add(new InventorySlot(null, 0));
        }

        // ── 아이템 추가 ──
        // 반환값: 실제로 넣지 못하고 남은 개수 (0이면 전부 들어감)
        public int AddItem(ItemData item, int amount = 1)
        {
            if (item == null || amount <= 0) return amount;

            // 1) 같은 아이템이 이미 있고 겹칠 수 있으면 거기에 먼저 쌓는다
            if (item.IsStackable)
            {
                foreach (var slot in slots)
                {
                    if (!slot.IsEmpty && slot.item == item && slot.count < item.maxStack)
                    {
                        int canAdd = item.maxStack - slot.count;
                        int added = Mathf.Min(canAdd, amount);
                        slot.count += added;
                        amount -= added;
                        if (amount <= 0) break;
                    }
                }
            }

            // 2) 남은 개수는 빈 칸에 새로 넣는다
            while (amount > 0)
            {
                InventorySlot empty = FindEmptySlot();
                if (empty == null) break; // 인벤토리가 가득 참

                int added = Mathf.Min(item.maxStack, amount);
                empty.item = item;
                empty.count = added;
                amount -= added;
            }

            OnInventoryChanged?.Invoke();
            return amount; // 못 넣고 남은 수
        }

        // ── 특정 슬롯의 아이템을 사용 ──
        public void UseItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count) return;
            InventorySlot slot = slots[slotIndex];
            if (slot.IsEmpty) return;

            ItemData item = slot.item;
            bool used = false;

            // 소모품이면 체력/정신력 회복 처리
            if (item.itemType == ItemType.Consumable && playerHealth != null)
            {
                if (item.healAmount > 0f) { playerHealth.Heal(item.healAmount); used = true; }
                if (item.sanityAmount > 0f)
                {
                    // 정신력은 '음수 감소'의 반대 → 음수를 넣어 회복시킨다
                    playerHealth.ReduceSanity(-item.sanityAmount);
                    used = true;
                }
            }

            Debug.Log($"아이템 사용: {item.displayName}");

            // 외부(PlayerItemEffects 등)에 사용 사실을 알린다
            if (OnItemUsed != null) OnItemUsed(item);

            // 사용에 성공한 소모품이면 개수를 1 줄인다
            if (used && item.itemType == ItemType.Consumable)
                RemoveFromSlot(slotIndex, 1);
            else
                OnInventoryChanged?.Invoke();
        }

        // ── 특정 슬롯에서 개수만큼 제거 ──
        public void RemoveFromSlot(int slotIndex, int amount)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count) return;
            InventorySlot slot = slots[slotIndex];
            if (slot.IsEmpty) return;

            slot.count -= amount;
            if (slot.count <= 0)
            {
                // 다 쓰면 칸을 비운다
                slot.item = null;
                slot.count = 0;
            }
            OnInventoryChanged?.Invoke();
        }

        // ── 특정 아이템을 보유 중인지 확인 (열쇠 문 등에서 사용) ──
        public bool HasItem(ItemData item, int amount = 1)
        {
            int total = 0;
            foreach (var slot in slots)
                if (!slot.IsEmpty && slot.item == item)
                    total += slot.count;
            return total >= amount;
        }

        // ── 빈 슬롯 찾기 (없으면 null) ──
        private InventorySlot FindEmptySlot()
        {
            foreach (var slot in slots)
                if (slot.IsEmpty) return slot;
            return null;
        }
    }
}
