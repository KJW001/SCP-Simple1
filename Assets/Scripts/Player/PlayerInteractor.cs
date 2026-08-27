using UnityEngine;
using SCPGame.Core;
using SCPGame.UI;

namespace SCPGame.Player
{
    // ─────────────────────────────────────────────────────────────
    // 플레이어가 화면 중앙(조준선)으로 바라보는 물체를 감지하고,
    // [E] 키로 상호작용(IInteractable)합니다.
    //  - 문 열기, 아이템 줍기, 스위치 누르기 등에 사용됩니다.
    //  - 바라보는 대상이 상호작용 가능하면 UI에 안내 문구를 띄웁니다.
    // ─────────────────────────────────────────────────────────────
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("상호작용 설정")]
        [Tooltip("상호작용 가능한 최대 거리")]
        public float interactDistance = 3f;
        [Tooltip("상호작용 키")]
        public KeyCode interactKey = KeyCode.E;
        [Tooltip("레이캐스트가 감지할 레이어 (기본: 모든 레이어)")]
        public LayerMask interactMask = ~0;

        [Header("참조")]
        [Tooltip("시선 기준이 될 카메라 (없으면 메인 카메라)")]
        public Camera playerCamera;
        [Tooltip("상호작용 안내 문구 UI (선택)")]
        public InteractionPromptUI promptUI;

        // 현재 바라보고 있는 상호작용 대상
        private IInteractable currentTarget;

        private void Awake()
        {
            if (playerCamera == null) playerCamera = Camera.main;

            // 상호작용 레이가 자기 몸(CharacterController)을 맞지 않도록
            // 이 오브젝트의 레이어를 마스크에서 제외한다.
            // (카메라가 캡슐 콜라이더 안에 있으면 정면 레이가 자기 몸에 막힘)
            interactMask &= ~(1 << gameObject.layer);
        }

        private void Update()
        {
            // 숨어 있는 동안엔 조준선 상호작용을 멈춘다.
            // (사물함에서 나오기는 HidingSpot 이 직접 [E] 로 처리한다)
            if (Core.PlayerState.IsHidden)
            {
                if (promptUI != null) promptUI.Hide();
                return;
            }

            DetectTarget();  // 1) 바라보는 대상 감지
            HandleInput();   // 2) 키 입력 처리
        }

        // ── 화면 중앙에서 레이를 쏴 상호작용 대상을 찾는다 ──
        private void DetectTarget()
        {
            currentTarget = null;

            if (playerCamera == null) return;

            // 카메라 위치에서 정면으로 레이를 발사
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask))
            {
                // 맞은 물체(또는 부모)에서 IInteractable 을 찾는다
                var interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null)
                    currentTarget = interactable;
            }

            // UI 안내 문구 갱신
            if (promptUI != null)
            {
                if (currentTarget != null)
                    promptUI.Show(currentTarget.InteractionPrompt); // 예: "[E] 줍기"
                else
                    promptUI.Hide();
            }
        }

        // ── E 키를 누르면 현재 대상과 상호작용 ──
        private void HandleInput()
        {
            if (currentTarget != null && Input.GetKeyDown(interactKey))
            {
                currentTarget.Interact(gameObject);
            }
        }
    }
}
