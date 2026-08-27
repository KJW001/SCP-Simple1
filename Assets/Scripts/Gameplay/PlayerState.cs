namespace SCPGame.Core
{
    // ─────────────────────────────────────────────────────────────
    // 플레이어의 '상태'를 여러 스크립트가 함께 보게 해주는 공용 저장소.
    //
    // 왜 static 인가요?
    //  숨어있는지 여부는 SCP AI, UI, 사운드 등 여러 곳에서 봐야 합니다.
    //  매번 참조를 연결하는 대신 한 곳에 모아두면 훨씬 간단합니다.
    //  (단, static 은 남용하면 관리가 어려워지니 꼭 필요한 것만!)
    // ─────────────────────────────────────────────────────────────
    public static class PlayerState
    {
        // 사물함 등에 숨어 있는가 (숨으면 대부분의 SCP가 감지하지 못함)
        public static bool IsHidden = false;

        // 시설 전원이 복구되었는가
        public static bool PowerRestored = false;

        // 정전 이벤트가 진행 중인가
        public static bool IsBlackout = false;

        // 씬을 다시 시작할 때 초기화 (static 은 자동으로 초기화되지 않음!)
        public static void Reset()
        {
            IsHidden = false;
            PowerRestored = false;
            IsBlackout = false;
        }
    }
}
