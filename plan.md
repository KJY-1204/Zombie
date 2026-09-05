# Plan

## 현재 마일스톤: Slice 2 — 레이더로 좀비 임시 표시

### 왜
Slice 1(원형 미니맵)이 검증 완료되어 "좀비는 기본적으로 미니맵에 보이지 않는다"는 전제가 확립됐다. CLAUDE.md 16장 순서대로 다음은 레이더 아이템 사용 시 일정 시간 동안 좀비를 미니맵에 드러내는 기능이다. 인벤토리 시스템(Slice 3)이 아직 없으므로, CLAUDE.md가 명시한 대로 기존 AmmoPack/HealthPack/Coin과 동일한 "주우면 즉시 발동" 패턴을 재사용하는 임시 테스트 인벤토리로 진행한다.

### 의존성 (Phase A 인스펙션으로 확인)
- 아이템 패턴: `IItem.Use(GameObject target)`, `PlayerHealth.OnTriggerEnter`가 트리거 충돌 시 호출. `AmmoPack.cs`가 가장 단순한 참고 예.
- 싱글톤 패턴: `GameManager`/`UIManager`와 동일한 `public static instance` + lazy `FindObjectOfType` + `Awake` 중복 파괴.
- `ItemSpawner.items` 배열에 프리팹만 추가하면 기존 무작위 스폰 루프에 편입됨.
- Zombie.prefab의 실제 렌더러는 `Zombie/Zombie_Cylinder`, `BloodSprayEffect`(+`BloodGlobs`) 3곳뿐, 전부 `Enemy`(10) 레이어로 격리되어 있음(Slice 1에서 확인).
- 레이어 현황: `Player`(9), `Enemy`(10), `MinimapPlayer`(11) 사용 중.
- `Minimap Camera`는 씬 전체 생명주기 동안 유지되는 안정적 오브젝트 — 레이더 타이머를 붙이기에 적합(CLAUDE.md 18장).

### 완료 기준 (Acceptance)
CLAUDE.md 19장 "Radar active"/"Radar expired" 섹션과 동일.
- 레이더 사용 시 좀비 마커가 즉시 보임, 사용 중 스폰된 좀비도 표시됨.
- 파괴된 좀비는 잔상 없이 사라짐.
- 설정된 시간 후 자동으로 숨겨짐, 재사용 시 처음부터 다시 카운트.
- 좀비 AI/월드 렌더링/데미지/충돌에는 영향 없음.

## 완료: Slice 1 — 원형 미니맵 (좀비 미표시)
검증 완료(checklist.md 참조). Enemy/MinimapPlayer 레이어, 전용 미니맵 카메라+RenderTexture, 원형 마스크 UI, 방위 라벨(N/S/E/W), 플레이어 마커까지 구현. 이후 사용자 요청으로 마우스 조준+WASD 이동 컨트롤 변경, 스트레이프 애니메이션 개선도 별도로 진행됨(미니맵과 무관, context-notes.md 참조).

## 다음 마일스톤 (순서대로)
Slice 3 인벤토리 → Slice 4 플레이어 상태 UI → Slice 5 차량 → Slice 6 월드 확장.
