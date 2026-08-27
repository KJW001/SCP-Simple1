using UnityEngine;

namespace SCPGame.Player
{
    // ─────────────────────────────────────────────────────────────
    // 달리기 스태미나. 무한정 도망칠 수 없게 만들어 긴장감을 줍니다.
    //
    // FirstPersonController 를 고치지 않고도 동작하도록,
    // 지쳤을 때 runSpeed 를 walkSpeed 로 낮추는 방식을 씁니다.
    // ─────────────────────────────────────────────────────────────
    [RequireComponent(typeof(FirstPersonController))]
    public class PlayerStamina : MonoBehaviour
    {
        [Header("스태미나")]
        public float maxStamina = 100f;
        public float stamina = 100f;
        [Tooltip("달릴 때 초당 소모")]
        public float drainPerSecond = 18f;
        [Tooltip("쉴 때 초당 회복")]
        public float regenPerSecond = 12f;
        [Tooltip("지친 뒤 이 수치까지 차야 다시 달릴 수 있다")]
        public float recoverThreshold = 30f;

        private FirstPersonController fpc;
        private float originalRunSpeed;
        private bool exhausted = false;

        public float StaminaRatio { get { return maxStamina <= 0f ? 0f : stamina / maxStamina; } }
        public bool IsExhausted { get { return exhausted; } }

        private void Awake()
        {
            fpc = GetComponent<FirstPersonController>();
            originalRunSpeed = fpc.runSpeed;
        }

        private void Update()
        {
            bool tryingToRun = fpc.IsRunning;

            if (tryingToRun && !exhausted)
            {
                stamina -= drainPerSecond * Time.deltaTime;
                if (stamina <= 0f) { stamina = 0f; exhausted = true; }
            }
            else
            {
                stamina += regenPerSecond * Time.deltaTime;
                if (stamina > maxStamina) stamina = maxStamina;
                // 충분히 회복되면 다시 달릴 수 있다
                if (exhausted && stamina >= recoverThreshold) exhausted = false;
            }

            // 지쳤으면 달리기 속도를 걷기 속도로 낮춘다
            fpc.runSpeed = exhausted ? fpc.walkSpeed : originalRunSpeed;
        }
    }
}
