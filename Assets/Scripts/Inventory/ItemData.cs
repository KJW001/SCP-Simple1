using UnityEngine;

namespace SCPGame.Items
{
    // 아이템의 큰 분류 (사용 방식 결정에 활용)
    public enum ItemType
    {
        Consumable, // 소모품 (구급상자, 진정제 등) - 사용하면 개수 감소
        KeyItem,    // 열쇠/카드키 등 핵심 아이템
        Document,   // 문서/메모 (읽기용)
        Tool        // 손전등 등 도구
    }

    // ─────────────────────────────────────────────────────────────
    // 아이템 "설계도"에 해당하는 데이터입니다. (ScriptableObject)
    // 실제 게임에서는 이 데이터를 여러 개 만들어(에셋으로 저장) 사용합니다.
    //
    // [만드는 법]
    //  Project 창에서 우클릭 → Create → SCP Game → Item Data
    //
    // ScriptableObject 를 쓰면 데이터를 코드가 아닌 에셋 파일로 관리하므로,
    // 기획자가 프로그래밍 없이도 아이템을 추가/수정할 수 있습니다.
    // ─────────────────────────────────────────────────────────────
    [CreateAssetMenu(fileName = "NewItem", menuName = "SCP Game/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("아이템 고유 ID (겹치지 않게)")]
        public string itemId = "item_id";
        [Tooltip("화면에 보일 이름")]
        public string displayName = "이름 없는 아이템";
        [Tooltip("아이템 설명")]
        [TextArea] public string description = "설명을 입력하세요.";
        [Tooltip("인벤토리에 표시할 아이콘")]
        public Sprite icon;

        [Header("분류 / 스택")]
        public ItemType itemType = ItemType.Consumable;
        [Tooltip("한 칸에 겹쳐 담을 수 있는 최대 개수")]
        public int maxStack = 5;

        [Header("사용 효과 (소모품용)")]
        [Tooltip("사용 시 회복하는 체력 (0이면 없음)")]
        public float healAmount = 0f;
        [Tooltip("사용 시 회복하는 정신력 (0이면 없음)")]
        public float sanityAmount = 0f;

        // 여러 칸에 겹칠 수 있는 아이템인지 (예: 소모품은 true, 열쇠는 보통 1개)
        public bool IsStackable => maxStack > 1;
    }
}
