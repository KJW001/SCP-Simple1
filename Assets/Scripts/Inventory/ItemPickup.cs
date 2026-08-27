using UnityEngine;
using SCPGame.Core;

namespace SCPGame.Items
{
    // ─────────────────────────────────────────────────────────────
    // 바닥/책상 등에 놓여 있어 플레이어가 주울 수 있는 아이템입니다.
    // IInteractable 을 구현하므로 PlayerInteractor 가 [E] 로 주울 수 있습니다.
    //
    // [사용법]
    //  - 3D 오브젝트에 Collider 를 붙이고 이 스크립트를 추가
    //  - itemData 에 주울 아이템(ItemData 에셋)을, amount 에 개수를 지정
    // ─────────────────────────────────────────────────────────────
    public class ItemPickup : MonoBehaviour, IInteractable
    {
        [Header("주울 아이템")]
        public ItemData itemData;
        [Tooltip("한 번에 줍는 개수")]
        public int amount = 1;

        // UI에 표시될 안내 문구 (예: "[E] 구급상자 줍기")
        public string InteractionPrompt =>
            itemData != null ? $"[E] {itemData.displayName} 줍기" : "[E] 줍기";

        // ── 플레이어가 상호작용(줍기) 했을 때 ──
        public void Interact(GameObject interactor)
        {
            // 상호작용한 대상(플레이어)에게서 인벤토리를 찾는다
            Inventory inventory = interactor.GetComponent<Inventory>();
            if (inventory == null || itemData == null) return;

            // 인벤토리에 넣고, 다 못 넣었으면 남은 만큼만 필드에 남긴다
            int remaining = inventory.AddItem(itemData, amount);

            if (remaining <= 0)
            {
                // 전부 주웠으면 필드에서 오브젝트 제거
                Destroy(gameObject);
            }
            else
            {
                // 인벤토리가 꽉 차서 일부만 주운 경우
                amount = remaining;
                Debug.Log("인벤토리가 가득 찼습니다.");
            }
        }
    }
}
