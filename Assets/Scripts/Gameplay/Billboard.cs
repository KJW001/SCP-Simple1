using UnityEngine;

namespace SCPGame.Gameplay
{
    // ─────────────────────────────────────────────────────────────
    // 이 오브젝트가 항상 카메라를 바라보게 만듭니다 (빌보드).
    // 몬스터 머리 위 이름표가 어느 각도에서 봐도 정면으로 읽히게 합니다.
    // ─────────────────────────────────────────────────────────────
    public class Billboard : MonoBehaviour
    {
        private Camera cam;

        private void Start() { cam = Camera.main; }

        private void LateUpdate()
        {
            if (cam == null) { cam = Camera.main; return; }
            // 카메라의 forward 방향을 그대로 바라보면 글자가 안 뒤집힘
            transform.forward = cam.transform.forward;
        }
    }
}
