using UnityEngine;
using SCPGame.Core;

namespace SCPGame.Gameplay
{
    // ─────────────────────────────────────────────────────────────
    // 탈출 지점. 전원이 복구된 뒤에 도달하면 게임 클리어입니다.
    // 트리거 콜라이더를 사용합니다 (Is Trigger 체크 필수).
    // ─────────────────────────────────────────────────────────────
    [RequireComponent(typeof(Collider))]
    public class ExitZone : MonoBehaviour
    {
        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            // 플레이어인지 확인
            if (other.GetComponentInParent<SCPGame.Player.PlayerHealth>() == null) return;

            if (!PlayerState.PowerRestored)
            {
                Debug.Log("게이트가 잠겨 있다. 전원을 먼저 복구해야 한다.");
                return;
            }
            if (ObjectiveManager.Instance != null) ObjectiveManager.Instance.Escape();
        }
    }
}
