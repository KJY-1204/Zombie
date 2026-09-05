# Plan

## 현재 마일스톤: Slice 1 — 원형 미니맵 (좀비 미표시)

### 왜
CLAUDE.md의 확장 로드맵(Product Goal: Project Zomboid 스타일 탐험 루프 + 기존 Zombie Survivor 전투)에서 미니맵/레이더는 명시적으로 요청된 6개 확장 시스템 중 하나다. CLAUDE.md 16장은 레이더(Slice 2)보다 반드시 "좀비가 보이지 않는 기본 미니맵"(Slice 1)을 먼저 완성하도록 순서를 못박고 있다 — 레이더가 먼저 있으면 "기본적으로 좀비가 숨겨져 있다"는 전제를 검증할 방법이 없기 때문이다.

### 의존성 (Phase A 인스펙션으로 확인)
- 씬: `Assets/Scenes/Main.unity` (유일한 게임플레이 씬).
- 플레이어: `Player Character` 프리팹, tag `Player`, layer `Player`(9), 컴포넌트 `PlayerMovement/PlayerInput/PlayerHealth/PlayerShooter`.
- 좀비: `Assets/Prefabs/Zombie.prefab`, layer 0(Default), `Zombie/NavMeshAgent/Animator/Collider` 부착. `ZombieSpawner`가 런타임에 스폰.
- HUD: `Assets/Prefabs/HUD Canvas.prefab` — Canvas(ScreenSpaceOverlay, Scale With Screen Size, 1280x720 기준), `UIManager` 부착.
- 베이스라인은 이미 완전히 구현되어 있음(스텁 아님) — 별도 완성 작업 불필요.

### 리스크
- 레이어 변경이 기존 사격/AI 판정에 영향을 줄 수 있음 → 사전 확인 결과 `Gun.cs`의 Raycast와 `Zombie.cs`의 `whatIsTarget`는 Zombie 자신의 레이어를 참조하지 않으므로 안전.
- 미니맵 전용 마커가 메인 카메라에 노출되면 안 됨 → 메인 카메라 컬링 마스크에서 마커 레이어 명시적 제외 필요.

### 완료 기준 (Acceptance)
CLAUDE.md 19장 "Normal minimap" 섹션과 동일.
- 원형 미니맵이 우측 상단에 항상 표시됨.
- 플레이어 마커가 항상 보임.
- 좀비 마커는 (레이더 미구현 상태이므로) 전혀 보이지 않음.
- 메인 카메라 화면에는 미니맵 전용 마커가 노출되지 않음.

## 다음 마일스톤 (순서대로)
Slice 2 레이더 → Slice 3 인벤토리 → Slice 4 플레이어 상태 UI → Slice 5 차량 → Slice 6 월드 확장.
