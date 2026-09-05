# Context Notes

Append-only. Verified project facts and decisions only.

## 2026-09-05 — Phase A 인스펙션 결과 (Slice 1 착수 전)

### Git / 원격
- 저장소: https://github.com/KJY-1204/Zombie (Public), 브랜치 `main`.
- 최초 커밋에 로컬 완성본 450개 파일 포함 (이전에 동일 이름으로 존재하던 저장소는 스텁 코드 기준으로 잘못 만들어졌던 것이라 삭제 후 재생성함 — 그 저장소의 context-notes.md 내용은 이 프로젝트와 무관하므로 참고하지 말 것).

### 베이스라인 상태 (Unity MCP 라이브 조회로 확인, 스텁 아님)
- `Assets/Scripts`의 `Gun.cs`(182줄), `Zombie.cs`(186줄), `PlayerHealth.cs`(102줄), `PlayerShooter.cs`(83줄), `ZombieSpawner.cs`(77줄) 모두 완전히 구현되어 있음.
- 씬 `Assets/Scenes/Main.unity` 루트 오브젝트 12개, 전부 배치 완료: Main Camera(Cinemachine Brain), Level Art, Player Character, Follow Cam, EventSystem, HUD Canvas, Game Manager, Spawn Points, Zombie Spawner, Item Spawner, Global Volume, Navigation(NavMeshSurface).
- `Assets/Prefabs/Zombie.prefab` 존재, 컴포넌트: Transform/Animator/BoxCollider/CapsuleCollider/AudioSource/Zombie/NavMeshAgent.
- 레이어: `Player`(9)만 커스텀 존재. 태그: 커스텀 없음.
- `Gun.Shot()`의 `Physics.Raycast(fireTransform.position, fireTransform.forward, out hit, fireDistance)`는 레이어마스크 인자 없음 → 모든 레이어 히트. `Zombie.cs`의 `whatIsTarget`(LayerMask)은 Player 레이어를 가리킴(Zombie 자신의 레이어 아님). **결론: Zombie의 GameObject 레이어를 바꿔도 사격/AI 타겟팅에 영향 없음.**
- `HUD Canvas.prefab`: Canvas RenderMode=ScreenSpaceOverlay(0), CanvasScaler UiScaleMode=ScaleWithScreenSize(1), ReferenceResolution 1280x720, MatchWidthOrHeight=1(높이 기준).
- `Assets/Sprites/Health Circle.png` 존재 — 원형 미니맵 마스크용으로 재사용 예정.
- `UIManager.cs`는 싱글톤(`FindObjectOfType` 1회 캐싱 패턴), ammo/score/wave 텍스트와 게임오버 UI만 관리. 체력 UI 없음(Slice 4에서 다룰 예정, 지금은 손대지 않음).
- 프로젝트에 Minimap/Radar 관련 코드/에셋 전혀 없음.

### 결정 사항
- Zombie 프리팹의 실제 3D 비주얼은 `Enemy` 레이어로 이동시켜 미니맵 카메라가 기본적으로 렌더링하지 못하게 한다(별도 마커 컴포넌트 없이 레이어만으로 Slice 1 요건 충족, 마커는 Slice 2 레이더에서 추가).
- 미니맵 카메라 컬링 마스크는 화이트리스트 방식(Default + MinimapPlayer만 포함)으로 구성 — 이후 레이더 슬라이스에서 MinimapZombie 레이어만 추가하면 됨.
- Level Art(맵 지형)는 레이어를 바꾸지 않고 Default 그대로 유지 — Zombie만 격리하면 충분하고, Level Art의 방대한 자식 트리를 건드리는 리스크를 피함.

## 2026-09-05 — Slice 1 구현 중 발견 사항

### 프리팹 레이어 변경은 재귀적으로 적용되지 않음
- `manage_prefabs modify_contents`의 `layer` 파라미터는 `target`으로 지정한 오브젝트 **하나에만** 적용됨(자식 전체에 재귀 적용 안 됨).
- `Zombie.prefab`은 루트 아래 본(Hips 등, Transform만 있음)과 실제 렌더러(`Zombie/Zombie_Cylinder` = SkinnedMeshRenderer, `BloodSprayEffect`/`BloodSprayEffect/BloodGlobs` = ParticleSystemRenderer)가 분리되어 있음 → 렌더러가 붙은 오브젝트 3곳을 각각 타겟으로 지정해서 레이어를 바꿔야 실제로 미니맵 카메라에서 안 보임. 본 트랜스폼은 렌더러가 없어 레이어를 안 바꿔도 무방.
- **다음에 비슷한 작업(레이어 일괄 변경) 할 때**: 프리팹의 실제 컴포넌트 구성을 `manage_prefabs get_hierarchy`로 먼저 확인하고, Renderer 계열 컴포넌트(MeshRenderer/SkinnedMeshRenderer/ParticleSystemRenderer 등)가 붙은 오브젝트만 골라서 개별적으로 `target` 지정할 것.

### `Health Circle.png`는 링(도넛) 모양 — 원형 마스크로 쓸 수 없음
- 픽셀 알파를 코드로 샘플링해 확인: 중심부(반지름의 0~60%) alpha=0, 바깥 고리(60~95%)만 alpha>0.
- 원형 UI 마스크(꽉 찬 원)가 필요하면 이 스프라이트 대신 `Assets/Sprites/Minimap Mask Circle.png`(이번에 코드로 생성한 채워진 원, 가장자리 1.5px 안티앨리어싱)를 사용할 것. Health Circle은 아마 체력 게이지 UI용으로 남겨두고 그대로 둠(용도 다름, 손대지 않음).

### `manage_components set_property`는 Vector2/Vector3 직렬화 필드를 지원하지 않음
- `RectTransform`의 `m_AnchorMin`/`m_SizeDelta` 등, `Camera`의 `m_TargetTexture`(오브젝트 참조는 되지만 Vector류는 안 됨) 같은 Vector 타입 프로퍼티는 `manage_components`로 설정 시 `"Unsupported SerializedPropertyType: Vector2"` 에러 발생.
- 우회: `execute_code`로 `PrefabStageUtility.GetCurrentPrefabStage()` / `GameObject.Find` 등을 이용해 C# 코드로 직접 값 대입. 이번 세션에서 Minimap UI의 RectTransform 앵커/사이즈, Mask/RawImage/Image 컴포넌트 부착 및 스프라이트/텍스처 연결을 전부 이 방식으로 처리함.
- `manage_gameobject`의 `target`/`parent` 파라미터는 문자열(이름 또는 경로)만 허용 — 숫자 instanceID를 그대로 넘기면 pydantic 타입 에러 발생. `manage_components`의 `target`은 반대로 instanceID(정수)를 그대로 받아줌. 헷갈리지 말 것.

### 헤드리스 자동화 세션의 Play Mode 프레임 진행 한계
- MCP를 통해 Play Mode를 실행해도 Unity Editor 창이 OS 포커스를 받지 못하는 환경에서는 `Time.frameCount`가 1에서 멈춰있고 `EditorApplication.QueuePlayerLoopUpdate()`를 반복 호출해도 진행되지 않음(원인: Editor 자체가 백그라운드에서 Update 루프를 안 돌림, `Application.runInBackground`는 false가 기본값이고 이건 플레이어 설정이라 임의로 바꾸지 않음).
- `manage_camera screenshot`은 그 순간의 프레임을 강제로 렌더링/캡처하는 것으로 보이나, 연속적인 실시간 동작(카메라 추적 등)은 이 방식으로 관찰 불가.
- **결론**: 정적 스크린샷 + 코드로 하는 레이캐스트/상태 조회로 최대한 검증하고, 프레임 단위 연속 동작(예: MinimapFollow가 실제로 매 프레임 따라오는지)은 스크립트 로직 리뷰로 대체 — 사용자가 에디터에 포커스를 두고 직접 플레이하며 최종 확인 필요.

### 신규 생성 에셋/레이어 목록
- 레이어: `Enemy`(10), `MinimapPlayer`(11).
- `Assets/Textures/MinimapRenderTexture.renderTexture` (512x512).
- `Assets/Sprites/Minimap Mask Circle.png` (코드로 생성한 채워진 원형 스프라이트).
- `Assets/Scripts/MinimapFollow.cs`.
- 씬에 `Minimap Camera` GameObject 추가.
- `Player Character.prefab`에 `Minimap Marker`(Quad) 자식 추가.
- `HUD Canvas.prefab`에 `Minimap`(Mask+RawImage) 계층 추가.
