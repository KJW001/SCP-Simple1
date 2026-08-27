using UnityEngine;
using TMPro;
using SCPGame.Core;

namespace SCPGame.Gameplay
{
    // ─────────────────────────────────────────────────────────────
    // 비밀번호 힌트 쪽지.
    //  [E] 로 읽으면 연결된 KeypadDoor 의 코드를 알려줍니다.
    //  코드는 KeypadDoor.Start() 에서 SetCode() 로 자동 주입되므로,
    //  매 판 랜덤으로 바뀐 코드가 이 쪽지에도 반영됩니다.
    //
    //  worldLabel 을 지정하면 쪽지 표면(월드 텍스트)에도 코드가 표시됩니다.
    // ─────────────────────────────────────────────────────────────
    public class PasswordHint : MonoBehaviour, IInteractable
    {
        [Header("표시(선택)")]
        [Tooltip("쪽지 표면에 코드를 그릴 월드 TMP 텍스트(선택)")]
        public TMP_Text worldLabel;

        [Tooltip("읽었을 때 나올 문구 형식. {door}=문이름, {code}=비밀번호")]
        [TextArea]
        public string template = "낙서: \"{door}\" 비밀번호 = {code}";

        // KeypadDoor 로부터 주입되는 값
        private string code = "????";
        private string doorName = "문";
        private bool codeAssigned = false;

        /// <summary>KeypadDoor 가 시작 시 호출해 코드를 채운다</summary>
        public void SetCode(string c, string dName)
        {
            code = c;
            doorName = dName;
            codeAssigned = true;
            if (worldLabel != null) worldLabel.text = code;
        }

        private string BuildText()
        {
            if (!codeAssigned) return "알아볼 수 없는 낙서다...";
            return template.Replace("{door}", doorName).Replace("{code}", code);
        }

        public string InteractionPrompt { get { return "[E] 쪽지 읽기"; } }

        public void Interact(GameObject interactor)
        {
            string msg = BuildText();
            if (ObjectiveManager.Instance != null)
                ObjectiveManager.Instance.SendMessage("Notify", msg, SendMessageOptions.DontRequireReceiver);
            Debug.Log("[힌트] " + msg);
        }
    }
}
