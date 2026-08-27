using System.Collections.Generic;
using UnityEngine;

namespace SCPGame.SCP
{
    // ─────────────────────────────────────────────────────────────
    // 25종 SCP 개체 데이터를 "코드로" 정의한 카탈로그입니다.
    //
    // 왜 이렇게 하나요?
    //  - ScriptableObject 에셋을 25개 일일이 만드는 대신, 코드로 한 번에
    //    정의하면 강의에서 전체 목록을 한눈에 보며 수치를 비교/수정하기 쉽습니다.
    //  - 실제 프로젝트에서는 이 목록을 참고해 ItemData 처럼 에셋으로
    //    옮겨두면 기획자가 인스펙터에서 관리할 수 있습니다.
    //
    // 사용:  SCPCatalog.GetAll() → 모든 SCPData 리스트
    //        SCPCatalog.Get("SCP-096") → 특정 개체
    // ─────────────────────────────────────────────────────────────
    public static class SCPCatalog
    {
        private static List<SCPData> _all;

        /// <summary>25종 전체 데이터를 가져온다 (최초 1회만 생성)</summary>
        public static List<SCPData> GetAll()
        {
            if (_all != null) return _all;
            _all = BuildCatalog();
            return _all;
        }

        /// <summary>번호로 특정 개체를 찾는다 (예: "SCP-173")</summary>
        public static SCPData Get(string scpNumber)
        {
            foreach (var d in GetAll())
                if (d.scpNumber == scpNumber) return d;
            return null;
        }

        // ── SCPData 하나를 코드로 만드는 도우미 ──
        // (ScriptableObject.CreateInstance 로 런타임 생성)
        private static SCPData Make(
            string number, string nick, string cls, SCPArchetype type,
            float patrol, float chase, float detect, float fov, float hear,
            float atkRange, float atkDmg, float atkCd, float sanity, float hp,
            string desc)
        {
            var d = ScriptableObject.CreateInstance<SCPData>();
            d.scpNumber = number;
            d.nickname = nick;
            d.containmentClass = cls;
            d.archetype = type;
            d.patrolSpeed = patrol;
            d.chaseSpeed = chase;
            d.detectionRange = detect;
            d.fieldOfView = fov;
            d.hearingRange = hear;
            d.attackRange = atkRange;
            d.attackDamage = atkDmg;
            d.attackCooldown = atkCd;
            d.sanityDrainPerSecond = sanity;
            d.maxHealth = hp;
            d.description = desc;
            return d;
        }

        // ── 25종 정의 ──
        // 인자 순서: 번호, 별명, 등급, 아키타입,
        //            순찰속도, 추격속도, 감지거리, 시야각, 청각거리,
        //            공격거리, 공격력, 공격쿨, 정신력압박/초, 체력, 설명
        private static List<SCPData> BuildCatalog()
        {
            var list = new List<SCPData>
            {
                Make("SCP-173","조각상","Euclid",SCPArchetype.Stalker,
                     0f,12f,30f,360f,0f, 1.5f,100f,0.5f, 6f,0f,
                     "보고 있으면 멈추고, 시선을 떼면 순식간에 다가와 목을 꺾는다."),

                Make("SCP-096","부끄럼쟁이","Euclid",SCPArchetype.LineOfSight,
                     0f,7f,80f,360f,0f, 2f,80f,1f, 4f,0f,
                     "얼굴을 본 대상을 지구 끝까지 추격해 갈기갈기 찢는다."),

                Make("SCP-049","역병의사","Euclid",SCPArchetype.Relentless,
                     1.2f,2.2f,20f,120f,5f, 2f,45f,1.5f, 5f,300f,
                     "느리지만 멈추지 않는다. 손이 닿으면 '치료'라는 이름의 죽음."),

                Make("SCP-106","노인","Keter",SCPArchetype.Teleporter,
                     1.5f,3f,18f,110f,6f, 2f,50f,2f, 4f,0f,
                     "벽과 바닥을 통과하며 시야 밖에서 갑자기 나타난다."),

                Make("SCP-939","여러목소리","Keter",SCPArchetype.Chaser,
                     2f,6.5f,10f,90f,14f, 2f,35f,1f, 3f,200f,
                     "앞이 거의 보이지 않지만 소리에 극도로 민감하다. 조용히 움직여라."),

                Make("SCP-682","불멸도마뱀","Keter",SCPArchetype.Relentless,
                     2f,5f,25f,120f,8f, 3f,60f,1.2f, 5f,999f,
                     "죽지 않는 파충류. 극한의 적응력으로 무엇이든 파괴한다."),

                Make("SCP-035","가면","Keter",SCPArchetype.Wanderer,
                     1.5f,4f,14f,110f,5f, 2f,30f,1f, 6f,120f,
                     "매혹적인 가면. 다가서면 부식성 체액이 흘러나온다."),

                Make("SCP-457","불꽃존재","Euclid",SCPArchetype.Chaser,
                     2f,4.5f,16f,120f,6f, 2.5f,40f,1f, 3f,150f,
                     "불로 이루어진 존재. 연료를 향해 끝없이 커진다."),

                Make("SCP-966","불면증","Keter",SCPArchetype.Stalker,
                     0f,6f,12f,360f,4f, 1.5f,25f,0.8f, 8f,80f,
                     "적외선으로만 보인다. 잠들지 못하게 만들며 서서히 다가온다."),

                Make("SCP-1471","말로","Euclid",SCPArchetype.Teleporter,
                     1.5f,4f,20f,110f,5f, 2f,30f,1.5f, 5f,100f,
                     "설치하면 사진 속 배경에 나타나기 시작하는 거대한 개의 형상."),

                Make("SCP-3008","무한이케아","Euclid",SCPArchetype.Wanderer,
                     1.8f,4f,15f,110f,6f, 2f,35f,1.2f, 4f,140f,
                     "밤이 되면 깨어나는 직원들. 끝없는 매장을 배회한다."),

                Make("SCP-087-1","계단의얼굴","Euclid",SCPArchetype.Chaser,
                     1f,5f,10f,60f,4f, 1.8f,45f,1f, 7f,0f,
                     "끝없는 계단 아래에서 올려다보는 눈코 없는 얼굴."),

                Make("SCP-058","심장","Keter",SCPArchetype.Chaser,
                     3f,7f,14f,120f,5f, 2f,55f,0.8f, 4f,180f,
                     "촉수 달린 심장. 매우 공격적이며 빠르게 돌진한다."),

                Make("SCP-1048","조립곰","Keter",SCPArchetype.Wanderer,
                     1f,3f,10f,90f,4f, 1.5f,20f,1f, 6f,50f,
                     "귀여워 보이지만 인간의 신체 부위로 새 개체를 조립한다."),

                Make("SCP-173B","분열체","Euclid",SCPArchetype.Stalker,
                     0f,10f,25f,360f,0f, 1.5f,70f,0.5f, 5f,0f,
                     "173의 변종. 조금 더 넓은 감지 범위를 가진다."),

                Make("SCP-2521","●●|●●●●●|●●|●","Keter",SCPArchetype.Teleporter,
                     2f,5f,22f,110f,10f, 2f,50f,1.5f, 6f,0f,
                     "글이나 말로 언급하면 나타나 그 정보를 가져간다. 이미지로만 존재."),

                Make("SCP-999","간지럼괴물","Safe",SCPArchetype.Wanderer,
                     1.5f,2f,8f,120f,4f, 1.5f,0f,1f, -5f,60f,
                     "주황색 슬라임. 해롭지 않고 오히려 정신력을 회복시켜 준다."),

                Make("SCP-096R","격노체","Euclid",SCPArchetype.LineOfSight,
                     0f,8f,90f,360f,0f, 2f,90f,0.8f, 4f,0f,
                     "096의 강화 개체. 더 빠르고 더 멀리서도 반응한다."),

                Make("SCP-513-1","방울요괴","Euclid",SCPArchetype.Teleporter,
                     1.5f,4f,16f,110f,8f, 1.8f,25f,1.2f, 7f,40f,
                     "종소리를 들은 대상에게만 보이는 마른 형상. 주변시로만 포착된다."),

                Make("SCP-966J","추격자","Keter",SCPArchetype.Relentless,
                     1.5f,3f,20f,120f,6f, 2f,40f,1.3f, 5f,220f,
                     "한 번 표적을 정하면 끈질기게 추격하는 유형."),

                Make("SCP-1000","빅풋","Euclid",SCPArchetype.Chaser,
                     2.5f,6f,18f,120f,7f, 2.5f,45f,1f, 3f,250f,
                     "거대한 유인원. 경계심이 강하지만 위협받으면 맹렬히 돌진한다."),

                Make("SCP-4666","키큰남자","Keter",SCPArchetype.Stalker,
                     0f,7f,20f,360f,3f, 2f,60f,0.7f, 8f,120f,
                     "겨울에 나타나는 비정상적으로 키가 큰 형상. 관측되면 정지한다."),

                Make("SCP-303","문의공포","Euclid",SCPArchetype.Wanderer,
                     1f,4f,12f,90f,5f, 2f,35f,1f, 6f,90f,
                     "문 너머에서 이빨을 드러내며 통과를 막는 존재."),

                Make("SCP-076-2","아벨","Keter",SCPArchetype.Relentless,
                     2f,6.5f,24f,140f,8f, 2.5f,65f,0.9f, 4f,350f,
                     "고대의 전사. 검을 소환해 무엇이든 베어버린다."),

                Make("SCP-610","살덩이","Keter",SCPArchetype.Chaser,
                     1.8f,4.5f,14f,110f,6f, 2f,38f,1.1f, 5f,160f,
                     "감염성 피부병 덩어리. 접촉한 대상을 변형시킨다."),
            };

            return list;
        }
    }
}
