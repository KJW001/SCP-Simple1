using UnityEngine;
using UnityEngine.UI;
using SCPGame.Core;
using SCPGame.Player;

namespace SCPGame.Gameplay
{
    // ─────────────────────────────────────────────────────────────
    // 사물함 등 "숨을 수 있는 장소".
    //
    //  [E] 로 숨으면:
    //   - 플레이어를 ★사물함 안으로 이동★시켜 실제로 들어가 숨는다
    //   - PlayerState.IsHidden 이 켜져 SCP 가 추격/공격을 멈춘다
    //   - 이동은 잠기지만 시점(마우스)은 살아있어 안에서 밖을 살필 수 있다
    //   - 화면 테두리를 어둡게 → "사물함 안" 느낌
    //
    //  ★ 나올 때: 사물함 안에서는 시선이 밖을 향해 조준선 상호작용이
    //     불가능하므로, 숨은 동안에는 이 스크립트가 직접 [E] 입력을 받아
    //     빠져나온다(어디를 보든 E 로 나올 수 있음). 원래 위치/방향 복귀.
    //
    //  ※ 이동이 잠긴 동안(MovementLocked)에는 Move 가 호출되지 않으므로
    //    안으로 옮겨도 끼이지 않는다.
    //  ※ SCP-096 은 이미 격노했다면 숨어도 소용없다 (원작 설정)
    // ─────────────────────────────────────────────────────────────
    public class HidingSpot : MonoBehaviour, IInteractable
    {
        [Header("사물함 안 기준점(선택)")]
        [Tooltip("지정하면 이 지점으로 들어간다. 비우면 사물함 중심으로 자동 계산")]
        public Transform insideAnchor;

        [Header("들어갈 때 웅크림 정도")]
        [Tooltip("사물함 안에서 카메라를 낮출 높이(0이면 그대로)")]
        public float crouchDrop = 0.35f;

        private bool occupied = false;
        private GameObject player;
        private FirstPersonController fpc;
        private Vector3 exitPos;
        private Quaternion exitRot;

        private KeyCode exitKey = KeyCode.E;   // 나오기 키(플레이어 설정에서 읽어옴)
        private int enterFrame = -1;           // 들어간 프레임(같은 프레임 즉시 탈출 방지)
        private float lockUntil = 0f;          // 나온 직후 재진입 방지 쿨다운

        private static Image hideOverlay;

        public string InteractionPrompt
        {
            get { return occupied ? "[E] 나오기" : "[E] 숨기"; }
        }

        public void Interact(GameObject interactor)
        {
            if (Time.time < lockUntil) return;      // 토글 디바운스
            if (!occupied) Enter(interactor);
            else Exit();
        }

        // 숨은 동안에는 조준선과 무관하게 [E] 로 나올 수 있게 직접 입력 처리
        private void Update()
        {
            if (!occupied) return;
            if (Time.frameCount == enterFrame) return;          // 들어간 그 프레임은 무시
            if (Input.GetKeyDown(exitKey)) Exit();
        }

        private void Enter(GameObject interactor)
        {
            player = interactor;
            fpc = player.GetComponent<FirstPersonController>();

            // 플레이어의 상호작용 키를 그대로 사용(리매핑 대응)
            var pin = player.GetComponent<PlayerInteractor>();
            if (pin != null) exitKey = pin.interactKey;

            // 나올 위치/방향 기억
            exitPos = player.transform.position;
            exitRot = player.transform.rotation;

            if (fpc != null) fpc.MovementLocked = true;

            // ── 사물함 안으로 이동 ──
            Vector3 inside = insideAnchor != null
                ? insideAnchor.position
                : new Vector3(transform.position.x, exitPos.y, transform.position.z);

            Vector3 outDir = exitPos - inside; outDir.y = 0f;
            if (outDir.sqrMagnitude < 0.01f) outDir = -transform.forward;

            player.transform.position = inside;
            player.transform.rotation = Quaternion.LookRotation(outDir.normalized, Vector3.up);

            if (crouchDrop > 0f && fpc != null && fpc.cameraTransform != null)
            {
                var p = fpc.cameraTransform.localPosition;
                p.y -= crouchDrop;
                fpc.cameraTransform.localPosition = p;
            }

            occupied = true;
            enterFrame = Time.frameCount;
            PlayerState.IsHidden = true;

            ShowOverlay(true);
            Notify("사물함 안에 숨었다. [E] 로 나올 수 있다.");
        }

        private void Exit()
        {
            if (crouchDrop > 0f && fpc != null && fpc.cameraTransform != null)
            {
                var p = fpc.cameraTransform.localPosition;
                p.y += crouchDrop;
                fpc.cameraTransform.localPosition = p;
            }

            if (player != null)
            {
                player.transform.position = exitPos;
                player.transform.rotation = exitRot;
            }

            if (fpc != null) fpc.MovementLocked = false;

            occupied = false;
            PlayerState.IsHidden = false;
            lockUntil = Time.time + 0.35f;   // 나온 직후 즉시 재진입 방지

            ShowOverlay(false);
            Notify("사물함에서 나왔다.");
        }

        // 화면 테두리를 어둡게 덮어 "안에 들어간" 느낌을 준다
        private void ShowOverlay(bool on)
        {
            if (hideOverlay == null && on)
            {
                var canvas = Object.FindFirstObjectByType<Canvas>();
                if (canvas == null) return;
                var go = new GameObject("HideVignette");
                go.transform.SetParent(canvas.transform, false);
                var rt = go.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                hideOverlay = go.AddComponent<Image>();
                hideOverlay.raycastTarget = false;
                hideOverlay.color = new Color(0f, 0f, 0f, 0f);
                go.transform.SetAsLastSibling();
            }
            if (hideOverlay != null)
                hideOverlay.color = new Color(0f, 0f, 0f, on ? 0.82f : 0f);
        }

        private void Notify(string msg)
        {
            if (ObjectiveManager.Instance != null)
                ObjectiveManager.Instance.SendMessage("Notify", msg, SendMessageOptions.DontRequireReceiver);
        }
    }
}
