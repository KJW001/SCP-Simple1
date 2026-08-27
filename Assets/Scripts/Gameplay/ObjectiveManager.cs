using System;
using UnityEngine;

namespace SCPGame.Core
{
    // 게임 진행 단계
    public enum Stage
    {
        FindKeycards,   // 1단계: 키카드 수집
        RestorePower,   // 2단계: 발전기 복구
        ReachExit,      // 3단계: 탈출구로
        Escaped         // 완료
    }

    // ─────────────────────────────────────────────────────────────
    // 게임의 "목표"를 관리합니다.
    //  키카드 수집 -> 발전기 복구 -> 탈출  순서로 단계가 진행됩니다.
    //  단계가 바뀌면 OnObjectiveChanged 이벤트로 UI에 알립니다.
    // ─────────────────────────────────────────────────────────────
    public class ObjectiveManager : MonoBehaviour
    {
        public static ObjectiveManager Instance { get; private set; }

        [Header("목표 수치")]
        [Tooltip("탈출에 필요한 키카드 수")]
        public int keycardsRequired = 3;
        [Tooltip("복구해야 할 발전기 수")]
        public int generatorsRequired = 3;

        [Header("현재 진행 상황 (읽기용)")]
        public int keycardsFound = 0;
        public int generatorsOn = 0;
        public Stage stage = Stage.FindKeycards;

        // UI가 구독해서 문구를 갱신합니다
        public event Action<string> OnObjectiveChanged;
        // 짧게 화면에 띄울 알림 (예: "키카드를 획득했다")
        public event Action<string> OnNotify;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            PlayerState.Reset();   // static 상태는 직접 초기화해야 합니다
        }

        private void Start() { PushObjective(); }

        /// <summary>키카드를 하나 주웠을 때 호출</summary>
        public void AddKeycard()
        {
            keycardsFound++;
            Notify("키카드 확보 (" + keycardsFound + "/" + keycardsRequired + ")");
            if (stage == Stage.FindKeycards && keycardsFound >= keycardsRequired)
                stage = Stage.RestorePower;
            PushObjective();
        }

        /// <summary>발전기를 하나 켰을 때 호출</summary>
        public void AddGenerator()
        {
            generatorsOn++;
            Notify("발전기 가동 (" + generatorsOn + "/" + generatorsRequired + ")");
            if (generatorsOn >= generatorsRequired)
            {
                PlayerState.PowerRestored = true;
                stage = Stage.ReachExit;
                Notify("시설 전원 복구. Gate A 가 열렸다.");
            }
            PushObjective();
        }

        /// <summary>탈출 성공</summary>
        public void Escape()
        {
            if (stage == Stage.Escaped) return;
            stage = Stage.Escaped;
            PushObjective();
            Notify("탈출 성공!");
            if (GameManager.Instance != null) GameManager.Instance.LockCursor(false);
            Time.timeScale = 0f;
        }

        /// <summary>발전기를 켤 수 있는 상태인가 (키카드를 다 모았는가)</summary>
        public bool CanUseGenerators => keycardsFound >= keycardsRequired;

        private void Notify(string msg)
        {
            Debug.Log("[목표] " + msg);
            if (OnNotify != null) OnNotify(msg);
        }

        // 현재 단계에 맞는 안내 문구를 만들어 UI에 전달
        private void PushObjective()
        {
            string t;
            switch (stage)
            {
                case Stage.FindKeycards:
                    t = "키카드를 찾아라  " + keycardsFound + " / " + keycardsRequired; break;
                case Stage.RestorePower:
                    t = "발전기를 가동하라  " + generatorsOn + " / " + generatorsRequired; break;
                case Stage.ReachExit:
                    t = "Gate A 로 탈출하라"; break;
                default:
                    t = "탈출 완료"; break;
            }
            if (OnObjectiveChanged != null) OnObjectiveChanged(t);
        }
    }
}
