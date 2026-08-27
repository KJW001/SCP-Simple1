using UnityEngine;
using UnityEngine.AI;
using SCPGame.UI;
using SCPGame.Core;

namespace SCPGame.Gameplay
{
    // ─────────────────────────────────────────────────────────────
    // 비밀번호(4자리) 키패드로 여는 문.
    //  플레이어가 [E] 로 상호작용하면 화면에 키패드가 뜨고,
    //  올바른 4자리를 입력하면 문이 위로 열립니다.
    //
    //  ★ 비밀번호는 '매 판 랜덤'으로 생성됩니다(randomizeOnStart).
    //  ★ 이 문에 연결된 힌트 쪽지(hints)에 코드가 자동으로 주입됩니다.
    //  ★ NavMeshObstacle(carving)로 닫혀 있는 동안 몬스터(NavMesh 이동)가
    //     통과하지 못하게 막고, 열리면 장애물을 꺼 통행을 허용합니다.
    // ─────────────────────────────────────────────────────────────
    [RequireComponent(typeof(NavMeshObstacle))]
    public class KeypadDoor : MonoBehaviour, IInteractable
    {
        [Header("비밀번호")]
        [Tooltip("직접 지정할 수도 있음. randomizeOnStart 가 켜져 있으면 무시되고 랜덤 생성")]
        public string code = "0000";
        [Tooltip("체크하면 게임 시작 시 매번 랜덤 4자리로 바뀜")]
        public bool randomizeOnStart = true;

        [Header("문 이름 / 힌트")]
        [Tooltip("UI·알림에 표시할 이름")]
        public string doorName = "잠긴 문";
        [Tooltip("이 문의 코드를 알려줄 힌트 쪽지들(선택). 시작 시 코드가 자동 주입됨")]
        public PasswordHint[] hints;

        [Header("열림 동작")]
        [Tooltip("열릴 때 이동할 방향(로컬). 기본은 위로 올라가 천장에 숨음")]
        public Vector3 openOffset = new Vector3(0f, 3.6f, 0f);
        public float openSpeed = 2.5f;

        [Header("상태")]
        public bool isOpen = false;

        public string Code { get { return code; } }

        private Vector3 closedPos;
        private Vector3 openPos;
        private NavMeshObstacle obstacle;

        private void Awake()
        {
            closedPos = transform.position;
            openPos = closedPos + transform.TransformVector(openOffset);

            // 닫힌 문 = NavMesh를 파내어(carve) 몬스터 통행 차단
            obstacle = GetComponent<NavMeshObstacle>();
            if (obstacle != null)
            {
                obstacle.carving = true;
                obstacle.enabled = true;
            }
        }

        private void Start()
        {
            // 매 판 랜덤 코드 생성 (0000 ~ 9999)
            if (randomizeOnStart || string.IsNullOrEmpty(code))
                code = Random.Range(0, 10000).ToString("D4");

            // 연결된 힌트 쪽지에 코드를 채워 넣는다
            if (hints != null)
            {
                foreach (var h in hints)
                    if (h != null) h.SetCode(code, doorName);
            }
        }

        private void Update()
        {
            Vector3 target = isOpen ? openPos : closedPos;
            transform.position = Vector3.MoveTowards(transform.position, target, openSpeed * Time.deltaTime);
        }

        public string InteractionPrompt
        {
            get { return isOpen ? string.Empty : "[E] 키패드 — " + doorName; }
        }

        public void Interact(GameObject interactor)
        {
            if (isOpen) return;
            if (KeypadUI.Instance != null)
                KeypadUI.Instance.Open(this);
            else
                Debug.LogWarning("KeypadUI 가 씬에 없습니다. UI_Canvas 에 KeypadUI를 추가하세요.");
        }

        /// <summary>KeypadUI 가 [입력] 버튼을 누를 때 호출. 정답이면 true</summary>
        public bool TryUnlock(string entered)
        {
            if (entered == code)
            {
                Open();
                return true;
            }
            return false;
        }

        private void Open()
        {
            isOpen = true;

            // 열린 문은 길을 막지 않도록 콜라이더 해제 + NavMesh 차단 해제
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
            if (obstacle != null) obstacle.enabled = false;   // 몬스터도 통과 가능

            if (ObjectiveManager.Instance != null)
                ObjectiveManager.Instance.SendMessage("Notify", doorName + " 개방!",
                    SendMessageOptions.DontRequireReceiver);
            Debug.Log(doorName + " 개방 (코드 " + code + ")");
        }
    }
}
