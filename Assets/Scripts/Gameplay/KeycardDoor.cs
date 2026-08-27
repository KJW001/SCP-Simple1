using UnityEngine;
using SCPGame.Core;
using SCPGame.Items;

namespace SCPGame.Gameplay
{
    // ─────────────────────────────────────────────────────────────
    // 키카드가 있어야 열리는 문.
    //  플레이어가 [E] 로 상호작용하면 인벤토리에 해당 키카드가 있는지
    //  확인하고, 있으면 문을 옆으로 밀어 엽니다.
    // ─────────────────────────────────────────────────────────────
    public class KeycardDoor : MonoBehaviour, IInteractable
    {
        [Header("필요 키카드")]
        [Tooltip("비워두면 아무나 열 수 있는 문")]
        public ItemData requiredCard;

        [Header("열림 동작")]
        [Tooltip("열릴 때 이동할 방향(로컬)")]
        public Vector3 openOffset = new Vector3(0f, 3.6f, 0f);
        public float openSpeed = 2.2f;

        [Header("상태")]
        public bool isOpen = false;

        private Vector3 closedPos;
        private Vector3 openPos;

        private void Awake()
        {
            closedPos = transform.position;
            openPos = closedPos + transform.TransformVector(openOffset);
        }

        private void Update()
        {
            // 목표 위치로 부드럽게 이동
            Vector3 target = isOpen ? openPos : closedPos;
            transform.position = Vector3.MoveTowards(transform.position, target, openSpeed * Time.deltaTime);
        }

        public string InteractionPrompt
        {
            get
            {
                if (isOpen) return string.Empty;
                if (requiredCard == null) return "[E] 문 열기";
                return "[E] " + requiredCard.displayName + " 필요";
            }
        }

        public void Interact(GameObject interactor)
        {
            if (isOpen) return;

            // 카드가 필요 없으면 그냥 열림
            if (requiredCard == null) { Open(); return; }

            var inv = interactor.GetComponent<Inventory>();
            if (inv != null && inv.HasItem(requiredCard))
            {
                Open();
            }
            else
            {
                Debug.Log("잠겨 있다. " + requiredCard.displayName + " 이(가) 필요하다.");
                if (ObjectiveManager.Instance != null)
                    ObjectiveManager.Instance.SendMessage("Notify", "잠겨 있다 — " + requiredCard.displayName + " 필요",
                        SendMessageOptions.DontRequireReceiver);
            }
        }

        private void Open()
        {
            isOpen = true;
            // 열린 문은 더 이상 막지 않도록 콜라이더 해제
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
            Debug.Log(gameObject.name + " 개방");
        }
    }
}
