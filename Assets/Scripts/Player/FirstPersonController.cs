using UnityEngine;

namespace SCPGame.Player
{
    // ─────────────────────────────────────────────────────────────
    // 1인칭 이동 컨트롤러.
    // CharacterController 컴포넌트를 이용해 걷기/달리기/앉기/점프/중력을
    // 처리하고, 마우스로 시점을 회전합니다.
    //
    // [사용법]
    //  - 플레이어 오브젝트에 CharacterController 를 추가하고
    //  - 자식으로 Camera 를 두고, 그 Transform 을 cameraTransform 에 연결하세요.
    // ─────────────────────────────────────────────────────────────
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("이동 속도")]
        [Tooltip("기본 걷기 속도")]
        public float walkSpeed = 3.5f;
        [Tooltip("달리기 속도 (Shift)")]
        public float runSpeed = 6f;
        [Tooltip("앉았을 때 속도 (Ctrl)")]
        public float crouchSpeed = 1.8f;

        [Header("점프 / 중력")]
        [Tooltip("점프 높이")]
        public float jumpHeight = 1.1f;
        [Tooltip("중력 가속도 (음수)")]
        public float gravity = -19.62f;

        [Header("마우스 시점")]
        [Tooltip("마우스 감도")]
        public float mouseSensitivity = 2f;
        [Tooltip("위/아래 시야 제한 각도")]
        public float verticalLookLimit = 85f;
        [Tooltip("플레이어 카메라 Transform")]
        public Transform cameraTransform;

        // 숨는 중 등: 이동만 잠그고 시점 회전은 허용
        [System.NonSerialized] public bool MovementLocked = false;

        [Header("앉기")]
        [Tooltip("서 있을 때 키")]
        public float standHeight = 1.8f;
        [Tooltip("앉았을 때 키")]
        public float crouchHeight = 1.0f;

        [Header("헤드밥 (걸을 때 화면 흔들림)")]
        public bool enableHeadBob = true;
        public float bobFrequency = 8f;
        public float bobAmount = 0.04f;

        // ── 내부 상태 변수 ──
        private CharacterController controller;   // 캐릭터 컨트롤러(충돌/이동 담당)
        private float verticalVelocity;           // 수직 속도(중력/점프 누적)
        private float cameraPitch;                // 카메라 상하 회전 각도
        private float defaultCamY;                // 카메라 기본 높이(헤드밥 복원용)
        private float bobTimer;                   // 헤드밥 타이머
        private bool isCrouching;                 // 현재 앉은 상태인지

        // 현재 프레임에 얼마나 빠르게 움직였는지 (발소리 등에서 참조)
        public float CurrentSpeed { get; private set; }
        // 지금 달리는 중인지 (발소리 간격 등에서 참조)
        public bool IsRunning { get; private set; }
        // 지면에 닿아 있는지
        public bool IsGrounded => controller.isGrounded;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            // 카메라를 지정하지 않았다면 자식에서 자동으로 찾아본다
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;

            if (cameraTransform != null)
                defaultCamY = cameraTransform.localPosition.y;
        }

        private void Update()
        {
            // 일시정지/게임오버 중에는 조작을 막는다
            if (Core.GameManager.Instance != null &&
                (Core.GameManager.Instance.isPaused || Core.GameManager.Instance.isGameOver))
                return;

            HandleLook();      // 1) 마우스 시점 회전
            HandleMovement();  // 2) 키보드 이동
            HandleHeadBob();   // 3) 걸을 때 화면 흔들림
        }

        // ── 마우스로 시점을 회전 ──
        private void HandleLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // 좌우 회전은 몸통(플레이어) 전체를 돌린다
            transform.Rotate(Vector3.up * mouseX);

            // 상하 회전은 카메라만 돌리되, 위아래로 너무 꺾이지 않게 제한한다
            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, -verticalLookLimit, verticalLookLimit);
            if (cameraTransform != null)
                cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }

        // ── 키보드로 이동 ──
        private void HandleMovement()
        {
            // 숨는 중이면 이동 입력을 무시 (그 자리에 머무름)
            if (MovementLocked) { return; }

            // 앉기 토글 처리
            if (Input.GetKeyDown(KeyCode.LeftControl))
                ToggleCrouch();

            // 지면에 닿아있고 아래로 떨어지는 중이면 수직 속도를 살짝 눌러준다
            // (완전히 0으로 두면 경사면에서 붕 뜨는 문제가 생김)
            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;

            // WASD 입력을 방향 벡터로 변환
            float x = Input.GetAxis("Horizontal"); // A/D
            float z = Input.GetAxis("Vertical");   // W/S
            Vector3 move = transform.right * x + transform.forward * z;
            move = Vector3.ClampMagnitude(move, 1f); // 대각선 이동이 더 빨라지지 않게

            // 현재 속도 결정 (앉기 > 달리기 > 걷기 우선순위)
            float targetSpeed = walkSpeed;
            IsRunning = false;
            if (isCrouching)
            {
                targetSpeed = crouchSpeed;
            }
            else if (Input.GetKey(KeyCode.LeftShift) && z > 0.1f)
            {
                targetSpeed = runSpeed;
                IsRunning = true;
            }

            controller.Move(move * targetSpeed * Time.deltaTime);

            // 실제로 얼마나 움직였는지(수평 속도)를 기록 → 발소리에서 사용
            CurrentSpeed = new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;

            // 점프 (앉은 상태에서는 불가)
            if (Input.GetButtonDown("Jump") && controller.isGrounded && !isCrouching)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // 중력 적용
            verticalVelocity += gravity * Time.deltaTime;
            controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
        }

        // ── 앉기/일어서기 전환 ──
        private void ToggleCrouch()
        {
            isCrouching = !isCrouching;
            controller.height = isCrouching ? crouchHeight : standHeight;
            // 키가 줄어든 만큼 중심도 내려서 바닥을 뚫지 않게 한다
            controller.center = new Vector3(0f, controller.height / 2f, 0f);
        }

        // ── 걸을 때 카메라를 위아래로 살짝 흔들어 몰입감을 준다 ──
        private void HandleHeadBob()
        {
            if (!enableHeadBob || cameraTransform == null) return;

            if (controller.isGrounded && CurrentSpeed > 0.1f)
            {
                // 이동 중이면 사인파로 위아래 흔들기
                bobTimer += Time.deltaTime * bobFrequency * (IsRunning ? 1.5f : 1f);
                float offsetY = Mathf.Sin(bobTimer) * bobAmount;
                Vector3 p = cameraTransform.localPosition;
                p.y = defaultCamY + offsetY;
                cameraTransform.localPosition = p;
            }
            else
            {
                // 멈추면 원래 위치로 부드럽게 복귀
                bobTimer = 0f;
                Vector3 p = cameraTransform.localPosition;
                p.y = Mathf.Lerp(p.y, defaultCamY, Time.deltaTime * 6f);
                cameraTransform.localPosition = p;
            }
        }
    }
}
