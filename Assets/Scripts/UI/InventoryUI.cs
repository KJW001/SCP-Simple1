using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SCPGame.Items;

namespace SCPGame.UI
{
    // ─────────────────────────────────────────────────────────────
    // 인벤토리 전체 UI를 관리합니다.
    //  - [I] 또는 [Tab] 으로 인벤토리 창을 열고 닫음
    //  - 슬롯 UI들을 자동 생성하고, 인벤토리 변화에 맞춰 갱신
    //  - 창이 열리면 커서를 풀어 클릭할 수 있게 함
    //
    // [준비물]
    //  - slotUIPrefab : InventorySlotUI 가 붙은 슬롯 프리팹
    //  - slotParent   : 슬롯들이 배치될 부모(Grid Layout Group 권장)
    //  - inventoryPanel : 켜고 끌 인벤토리 창 전체 오브젝트
    // ─────────────────────────────────────────────────────────────
    public class InventoryUI : MonoBehaviour
    {
        [Header("참조")]
        public Inventory inventory;
        [Tooltip("켜고 끌 인벤토리 창 오브젝트")]
        public GameObject inventoryPanel;
        [Tooltip("슬롯 UI 프리팹")]
        public InventorySlotUI slotUIPrefab;
        [Tooltip("슬롯들이 놓일 부모 (Grid Layout Group)")]
        public Transform slotParent;

        [Header("입력")]
        public KeyCode toggleKey = KeyCode.I;

        // 생성된 슬롯 UI들을 보관
        private InventorySlotUI[] slotUIs;
        private bool isOpen = false;

        private void Start()
        {
            if (inventory == null)
                inventory = FindFirstObjectByType<Inventory>();

            BuildSlots();

            // 인벤토리 변화 이벤트 구독 → 자동 갱신
            if (inventory != null)
                inventory.OnInventoryChanged += RefreshAll;

            // 시작할 땐 닫아둔다
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (inventory != null)
                inventory.OnInventoryChanged -= RefreshAll;
        }

        private void Update()
        {
            // 토글 키로 열고 닫기
            if (Input.GetKeyDown(toggleKey))
                Toggle();
        }

        // ── 슬롯 UI들을 인벤토리 칸 수만큼 생성 ──
        private void BuildSlots()
        {
            if (inventory == null || slotUIPrefab == null || slotParent == null) return;

            slotUIs = new InventorySlotUI[inventory.slots.Count];
            for (int i = 0; i < inventory.slots.Count; i++)
            {
                InventorySlotUI slotUI = Instantiate(slotUIPrefab, slotParent);
                slotUI.Init(this, i);
                slotUIs[i] = slotUI;
            }
            RefreshAll();
        }

        // ── 모든 슬롯 UI를 현재 인벤토리 내용으로 갱신 ──
        private void RefreshAll()
        {
            if (slotUIs == null || inventory == null) return;
            for (int i = 0; i < slotUIs.Length; i++)
                slotUIs[i].Refresh(inventory.slots[i]);
        }

        // ── 슬롯이 클릭되면 해당 아이템 사용 ──
        public void OnSlotClicked(int index)
        {
            if (inventory != null)
                inventory.UseItem(index);
        }

        // ── 인벤토리 창 열기/닫기 ──
        private void Toggle()
        {
            isOpen = !isOpen;
            if (inventoryPanel != null) inventoryPanel.SetActive(isOpen);

            // 창이 열리면 커서를 풀고(클릭 가능), 닫히면 다시 잠근다
            if (Core.GameManager.Instance != null)
                Core.GameManager.Instance.LockCursor(!isOpen);

            // 창이 열려있는 동안 게임을 잠깐 멈추고 싶다면 아래 주석 해제
            // Time.timeScale = isOpen ? 0f : 1f;
        }
    }
}
