using UnityEngine;

namespace SCPGame.Core
{
    // ─────────────────────────────────────────────────────────────
    // 게임 전반에서 공용으로 쓰는 인터페이스 모음입니다.
    // 인터페이스로 "규칙"만 정의해두면, 플레이어든 몬스터든 문이든
    // 같은 방식으로 상호작용/피해 처리를 할 수 있어 확장이 쉽습니다.
    // ─────────────────────────────────────────────────────────────

    /// <summary>데미지를 받을 수 있는 대상(플레이어, SCP 등)</summary>
    public interface IDamageable
    {
        // 현재 체력이 남아있는지 여부
        bool IsAlive { get; }

        // damage 만큼 피해를 준다. attacker 는 공격 주체(없으면 null)
        void TakeDamage(float damage, GameObject attacker = null);
    }

    /// <summary>플레이어가 조준선으로 바라보고 [E]로 상호작용할 수 있는 대상</summary>
    public interface IInteractable
    {
        // UI에 표시할 문구 (예: "줍기", "열기")
        string InteractionPrompt { get; }

        // 실제 상호작용이 일어날 때 호출된다. interactor 는 상호작용한 주체
        void Interact(GameObject interactor);
    }

    /// <summary>인벤토리에서 사용(Use)할 수 있는 아이템의 규칙</summary>
    public interface IUsable
    {
        // 아이템을 사용한다. 성공하면 true(→ 소모품이면 개수 감소)
        bool Use(GameObject user);
    }
}
