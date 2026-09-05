# Plan

## 현재 마일스톤: Slice 3 — 인벤토리 foundation (체력팩 + 레이더)

### 왜
Slice 1(미니맵)·Slice 2(레이더)가 검증 완료됐다. 지금 레이더는 "임시 테스트 인벤토리"(줍자마자 즉시 발동)로만 동작 중이라, 진짜 인벤토리(줍고 → 모았다가 → 원할 때 사용)로 승격시킨다. 사용자 확인: 체력팩+레이더만 인벤토리화, 탄약/코인은 즉시 적용 유지, 숫자키(1/2)로 즉시 사용.

### 의존성 (Phase A 인스펙션으로 확인)
- 기존 픽업 4종 전부 `IItem.Use(GameObject target)`에서 효과 직접 실행 후 자기 파괴 — 데이터/동작 미분리.
- ScriptableObject 데이터 컨벤션: `ZombieData`/`GunData`가 `[CreateAssetMenu]` 패턴 사용 → `ItemData`도 동일.
- 싱글톤 컨벤션: `GameManager`/`UIManager`/`MinimapRadarController`와 동일 패턴. `Inventory`는 싱글톤 아님(플레이어 소유 컴포넌트).
- `UIManager.cs`의 "게임플레이가 상태 변경 → Update*Text 호출" 패턴을 인벤토리 UI에도 재사용.
- `PlayerInput.cs`가 원시 입력 중앙집중 관리 → 인벤토리 사용 키도 여기 추가.

### 완료 기준 (Acceptance)
CLAUDE.md 16장 Slice 3, 21장 Inventory Acceptance Tests와 동일.
- 아이템을 주우면 인벤토리 수량이 정확히 1회 증가.
- 소비 시 수량이 정확히 1회 감소, 음수가 되지 않음.
- 레이더 사용이 기존 아이템과 동일한 범용 경로(`ItemData.Use`)를 통함 — 하드코딩 없음.
- UI는 실제 인벤토리 변경 후에만 갱신(매 프레임 아님).

## 완료: Slice 2 — 레이더로 좀비 임시 표시
검증 완료(checklist.md 참조). MinimapZombie 레이어, 좀비 미니맵 마커, MinimapRadarController(Time.time 기반 타이머), RadarPack 아이템(임시 즉시발동 방식)까지 구현. 이후 사용자 요청으로 RadarPack 비주얼을 구체→탐지기 모양(받침대+안테나+접시)+배색(회색 본체+초록 접시)으로 개선.

## 완료: Slice 1 — 원형 미니맵 (좀비 미표시)
검증 완료(checklist.md 참조). Enemy/MinimapPlayer 레이어, 전용 미니맵 카메라+RenderTexture, 원형 마스크 UI, 방위 라벨(N/S/E/W), 플레이어 마커까지 구현. 이후 사용자 요청으로 마우스 조준+WASD 이동 컨트롤 변경, 스트레이프 애니메이션 개선도 별도로 진행됨(미니맵과 무관, context-notes.md 참조).

## 다음 마일스톤 (순서대로)
Slice 4 플레이어 상태 UI → Slice 5 차량 → Slice 6 월드 확장.
