using UnityEngine;
using UnityEngine.AI;

namespace SCPGame.SCP
{
    // ─────────────────────────────────────────────────────────────
    // SCP 개체를 씬에 생성(스폰)하는 스포너입니다.
    //  - 카탈로그(코드) 또는 인스펙터에 지정한 데이터로 개체를 만든다
    //  - 아키타입에 맞는 '행동 스크립트'를 자동으로 붙여준다
    //
    // [준비물]
    //  - bodyPrefab : SCP의 겉모습(모델 + Collider). NavMeshAgent 없어도 됨
    //                 (스포너가 NavMeshAgent 를 자동으로 추가)
    //  - 씬 바닥에 NavMesh 를 미리 Bake 해둘 것
    // ─────────────────────────────────────────────────────────────
    public class SCPSpawner : MonoBehaviour
    {
        [Header("스폰 대상")]
        [Tooltip("SCP 겉모습 프리팹 (콜라이더 포함 권장)")]
        public GameObject bodyPrefab;

        [Tooltip("스폰할 SCP 번호 (예: SCP-096). 카탈로그에서 찾음")]
        public string spawnScpNumber = "SCP-173";

        [Tooltip("인스펙터로 직접 데이터를 넣고 싶으면 여기에 (있으면 번호보다 우선)")]
        public SCPData overrideData;

        [Header("스폰 위치")]
        [Tooltip("여기서 스폰. 비우면 이 스포너 위치")]
        public Transform spawnPoint;

        [Tooltip("게임 시작 시 자동 스폰")]
        public bool spawnOnStart = true;

        private void Start()
        {
            if (spawnOnStart) Spawn();
        }

        /// <summary>지정된 SCP를 스폰한다</summary>
        public GameObject Spawn()
        {
            if (bodyPrefab == null)
            {
                Debug.LogWarning("SCPSpawner: bodyPrefab 이 지정되지 않았습니다.");
                return null;
            }

            // 사용할 데이터 결정: override 가 있으면 그것, 없으면 카탈로그에서 검색
            SCPData data = overrideData != null ? overrideData : SCPCatalog.Get(spawnScpNumber);
            if (data == null)
            {
                Debug.LogWarning($"SCPSpawner: '{spawnScpNumber}' 데이터를 찾을 수 없습니다.");
                return null;
            }

            // 스폰 위치 결정
            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;

            // 겉모습 생성
            GameObject go = Instantiate(bodyPrefab, pos, Quaternion.identity);
            go.name = $"{data.scpNumber} ({data.nickname})";

            // NavMeshAgent 가 없으면 붙여준다 (이동에 필수)
            if (go.GetComponent<NavMeshAgent>() == null)
                go.AddComponent<NavMeshAgent>();

            // 아키타입에 맞는 행동 스크립트를 붙이고 데이터를 연결한다
            SCPEntity entity = AttachBehavior(go, data.archetype);
            entity.data = data;

            Debug.Log($"스폰됨: {go.name} / 유형: {data.archetype}");
            return go;
        }

        // ── 아키타입 → 알맞은 SCPEntity 파생 클래스를 부착 ──
        private SCPEntity AttachBehavior(GameObject go, SCPArchetype archetype)
        {
            switch (archetype)
            {
                case SCPArchetype.LineOfSight:
                    return go.AddComponent<Scp096Entity>();   // 시야 트리거 격노형

                case SCPArchetype.Stalker:
                    return go.AddComponent<Scp173Entity>();   // 관측 시 정지형

                case SCPArchetype.Relentless:
                    return go.AddComponent<Scp049Entity>();   // 끈질긴 추격형

                case SCPArchetype.Teleporter:
                    return go.AddComponent<TeleporterEntity>(); // 순간이동형

                case SCPArchetype.Chaser:
                case SCPArchetype.Wanderer:
                default:
                    // Chaser/Wanderer 는 기본 FSM 그대로 사용
                    return go.AddComponent<SCPEntity>();
            }
        }
    }
}
