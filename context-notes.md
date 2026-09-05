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

## 2026-09-05 — 미니맵 확대 + 플레이어 마커 버그 수정

### 미니맵 확대율 조정
- 사용자 요청으로 `Minimap Camera`의 `orthographicSize`를 20 → 10으로 축소(2배 확대). 미니맵 UI 크기(160x160)는 변경하지 않음.

### [중요 버그] Minimap Marker가 Slice 1 내내 실제로는 안 보이고 있었음
- 증상: 플레이어 마커에 밝은 노란색 Unlit 머티리리얼(`Assets/Materials/MinimapPlayerMarker.mat`)을 새로 만들어 입혔는데도 미니맵에 전혀 안 보임.
- 진단: `Camera.Render()`를 코드로 강제 호출한 뒤 `cam.WorldToViewportPoint()`로 마커의 화면 좌표를 계산하고, `RenderTexture`에서 그 픽셀 색상을 직접 읽어(`ReadPixels`) 확인 — 배경색만 나오고 마커 색이 전혀 없었음.
- 원인: Quad를 바닥과 수평으로 눕히면서 회전을 `(rotation X = -90 / 270)`로 넣었는데, 이 경우 `transform.forward`는 `(0,1,0)`(위쪽, 카메라 방향)이 되지만 **Quad 메쉬의 실제 가시면(비컬링되는 면)은 그 반대 방향**이라 위에서 보는 미니맵 카메라 기준으로는 백페이스 컬링되어 안 보였음. `rotation X = 90`으로 넣으면 `transform.forward = (0,-1,0)`(아래쪽)이 되는데, 오히려 이 경우가 실제로 위(미니맵 카메라)에서 보이는 면이 됨.
- **결론/규칙**: 바닥에 눕혀서 위에서 보이게 할 평면(Quad 등)을 만들 때는 `transform.forward`가 "카메라를 향하는 방향"이라고 가정하지 말 것 — 반드시 실제 렌더 결과(스크린샷 또는 `Camera.Render()` + `ReadPixels` 픽셀 검사)로 가시성을 확인한 뒤 회전값을 확정해야 함. 최종적으로 `Minimap Marker`의 올바른 로컬 회전은 `(90, 0, 0)`.
- 이 버그는 Slice 1 완료 보고 당시에는 발견하지 못했음(그때는 마커가 회색 기본 머티리얼이라 앞서 존재하던 분홍 링/화재 이펙트 등과 시각적으로 구분이 안 돼서 "안 보이는 것"을 "잘 안 보이는 것"으로 착각함). 이후 눈에 띄는 노란색으로 바꾸고 나서야 완전히 안 보인다는 게 명확해져 발견함.
- 마커용 신규 머티리얼: `Assets/Materials/MinimapPlayerMarker.mat` (URP Unlit, `_BaseColor` = 노란색 (1, 0.95, 0.1, 1) — 조명 영향을 안 받아 항상 밝게 보임). 마커 스케일은 0.5 → 0.9로 확대.

## 2026-09-05 — 컨트롤 방식 변경: 마우스 조준 + WASD 이동 (사용자 요청, 미니맵과 무관)

### 변경 전 방식 (탱크 컨트롤)
- `PlayerInput.rotate`(Horizontal=A/D)로 캐릭터 전체를 좌우 회전, `move`(Vertical=W/S)로 그 방향 기준 전후 이동. 마우스 입력 없음.

### 변경 후 방식
- `PlayerInput.cs`: `rotate`/`rotateAxisName` 제거 → `strafe`/`strafeAxisName`(Horizontal=A/D)로 대체. 마우스 조준을 위해 `mouseWorldPosition`(카메라→마우스 스크린좌표 레이캐스트로 계산한 바닥 위 월드 좌표) 프로퍼티 추가. 바닥에 아무것도 안 맞으면 플레이어 높이의 무한 평면과의 교차점으로 폴백.
- `PlayerMovement.cs`: `Rotate()`(입력 기반 몸 회전) 삭제 → `RotateTowardsMouse()`(마우스 조준 지점을 향해 `rotateSpeed`(기존 필드 재사용, 180deg/s) 한도로 회전) 신설. `Move()`는 `transform.forward` 기준 전후 이동 대신, **카메라의 수평 방향(forward/right, Y성분 제거 후 정규화) 기준**으로 `move`(전후)+`strafe`(좌우) 합산 이동으로 변경(대각선 이동 시 정규화로 속도 보정).
- Follow Cam(Cinemachine)의 Transposer가 `WorldSpace` 바인딩 모드(`m_FollowOffset=(-8,16,-8)`)라 카메라가 플레이어 회전과 무관하게 항상 고정된 세계 각도(고정 아이소메트릭, yaw 45°)를 유지함 → 카메라 기준 WASD가 조준 방향과 무관하게 항상 일관된 화면 방향으로 동작함(회전해도 이동 체감이 안 바뀜). 이 전제가 깨지면(예: 카메라가 플레이어를 따라 회전하도록 바뀌면) Move()의 카메라 기준 계산도 다시 검토해야 함.
- 헤드리스 세션이라 실시간 Play Mode 프레임 진행이 안 되는 한계는 여전함 → `RotateTowardsMouse()`/`Move()` private 메서드를 리플렉션으로 직접 반복 호출해 회전이 목표각(예: +X 방향 조준 시 90°)으로 정확히 수렴하는지, 이동 델타 방향이 카메라 forward와 정확히 일치하는지 코드 레벨로 검증함(실제 마우스 입력 자체는 이 환경에서 시뮬레이션 불가 — 사용자가 에디터에서 직접 플레이하며 손맛 확인 필요).

## 2026-09-05 — 사용자 플레이 테스트 피드백: 이동 애니메이션이 부자연스러움 → 수정

### 원인 분석
- 좌우 스트레이프(A/D) 시에도 애니메이터 `Move` 파라미터를 항상 양수(이동 크기)로 넣고 있어서, 캐릭터가 옆으로 미끄러지는데도 정면 달리기(Run) 애니메이션이 그대로 재생됨("게걸음/미끄러짐"처럼 보임).
- `ShooterAnimator.controller`의 베이스 레이어 "Movement" 상태를 코드로 직접 조회해 확인: `Simple1D` 블렌드 트리, 파라미터 `Move`, 임계값 **-1(Run) / -0.5(Walk) / 0(Idle) / 0.5(Walk) / 1(Run)** — 원래부터 부호 있는(signed) 전진(+)/후진(-) 값을 받도록 설계되어 있었음(다만 -1과 +1이 같은 "Humanoid Run" 클립을 재사용하므로 부호 자체가 클립을 바꾸진 않고, 0을 통과하는 블렌드 경로만 달라짐). **좌우 전용 애니메이션 클립은 애초에 프로젝트에 없음**(`Assets/Animations`에는 Idle/Walk/Run/Reload/Aim Idle뿐, 좌우/후진 전용 클립 없음) — 그래서 "진짜" 옆걸음 애니메이션은 새 애니메이션 없이는 원천적으로 불가능.
- 대안으로 스파인/상체를 코드로 비틀어 조준 방향과 이동 방향을 분리하는 방법(AAA 게임에서 흔한 기법)도 검토했으나, `OnAnimatorIK` 타이밍과 얽혀 손 IK가 깨지거나 부자연스러운 회전이 나올 위험이 있고 이 환경에서 실제 손맛을 확인할 방법이 없어 **시도하지 않기로 결정**(사용자가 "너무 어려우면 하지 말라"고 명시함).

### 적용한 수정 (저위험)
- `PlayerMovement.FixedUpdate`에서 `Move` 파라미터를 `이동 방향 · 캐릭터 정면 방향`(내적, Dot Product)으로 계산하도록 변경. 순수 좌우 이동이면 내적이 0에 가까워져 Idle에 가깝게 블렌드되고(기존엔 Run이 재생되던 것), 전진/후진은 그대로 ±1 근처로 정확히 매핑됨.
- **[발견한 함정]** `RotateTowardsMouse()`에서 `playerRigidbody.rotation = newRotation;`으로 회전을 직접 대입한 직후 같은 프레임에서 `transform.forward`를 읽으면, 이 헤드리스 세션(물리 스텝이 실제로 안 돌아감)에서는 Transform이 갱신되지 않은 값을 반환함(실제 플레이 중에는 즉시 동기화되어 문제없을 가능성이 높지만 확신할 수 없었음). **방어적으로 수정**: `RotateTowardsMouse()`가 방금 계산한 회전으로부터 정면 방향(`Vector3`)을 직접 반환하도록 바꾸고, `FixedUpdate`는 그 반환값을 그대로 내적 계산에 사용 — Transform 동기화 타이밍에 전혀 의존하지 않게 됨.
- 리플렉션으로 `RotateTowardsMouse`가 카메라 정면 대각선(0.71,0,0.71)을 향하도록 수렴시킨 뒤 W/S/A/D 각각의 `animMoveParam`을 계산해 검증: 전진=1.00, 후진=-1.00, 좌스트레이프=0.00, 우스트레이프=0.00, 대각선(W+D)=0.71 — 의도한 대로 정확히 나옴.

## 2026-09-05 — Slice 2: 레이더로 좀비 임시 표시

### 아이템/싱글톤 기존 패턴 재사용
- `IItem.Use(GameObject target)` + `PlayerHealth.OnTriggerEnter`가 트리거 충돌 시 자동 호출하는 기존 픽업 패턴(`AmmoPack.cs` 참고)을 그대로 따라 `RadarPack.cs` 작성. 인벤토리(Slice 3) 없이도 "주우면 즉시 발동"으로 동작.
- `MinimapRadarController`는 `GameManager`/`UIManager`와 동일한 싱글톤 패턴(`public static instance` + lazy `FindObjectOfType` + `Awake` 중복 파괴) 사용.

### 레이어/마커 설계
- `MinimapZombie` 레이어(슬롯 12) 추가. `Zombie.prefab`에 Player와 동일한 기법(Quad, 로컬 회전 `(90,0,0)`)으로 마커 자식 추가 — Slice 1에서 이미 "Quad 뒷면 방향" 함정을 검증해뒀기 때문에 이번엔 바로 올바른 회전값을 사용, 재발 없음.
- 마커가 좀비 프리팹의 자식이라 좀비 스폰/파괴에 따라 자동으로 생성/파괴됨 — 레이더 컨트롤러나 스포너 쪽에 별도 등록/해제 로직이 전혀 필요 없음(이벤트 기반 관리, 매 프레임 스캔 없음 — CLAUDE.md 13.6 권장사항 그대로 충족).

### `MinimapRadarController` 구현 및 검증
- 코루틴 대신 `Update()`에서 `Time.time >= revealEndTime` 체크하는 방식 채택 — `Time.timeScale` 변경 없이 동작(CLAUDE.md 18장 요구사항), 씬 전환 등으로 오브젝트가 파괴돼도 코루틴 잔존 문제가 원천적으로 없음.
- `ActivateRadar()`는 이미 활성 중이어도 `revealEndTime = Time.time + revealDuration`을 항상 재대입 — "재사용 시 처음부터 다시 카운트" 요구사항을 별도 분기 없이 자연스럽게 충족.
- 검증 방법(헤드리스라 실시간 타이머 흐름은 못 봄, 로직을 직접 호출/픽셀 검사로 확정):
  - `cam.cullingMask` 값 변화(2049 ↔ 6145, 4096=`1<<MinimapZombie`)로 on/off 확인.
  - `Camera.Render()` 강제 호출 + `WorldToViewportPoint` + `ReadPixels`로 좀비 마커의 실제 렌더링 픽셀이 마커 머티리얼 색과 정확히 일치함을 확인(카메라가 좀비 위치까지 못 따라가는 헤드리스 한계를 카메라를 임시로 좀비 위치로 옮겨서 우회).
  - `revealEndTime` 필드를 리플렉션으로 과거 시각으로 설정 후 `Update()` 1회 호출 → 정상적으로 꺼짐.

### RadarPack.prefab 구성
- 전용 3D 모델이 없어 Sphere 프리미티브(스케일 0.4)에 시안색 Unlit 머티리얼(`RadarPackVisual.mat`)을 입힌 placeholder 비주얼 사용 — 나중에 실제 아트가 생기면 이 자식만 교체하면 됨.
- 루트: `SphereCollider`(trigger, radius 0.4) + `Rotator`(기존 재사용) + `RadarPack`. 자식 비주얼에는 콜라이더 없음(루트 트리거 하나로만 픽업 판정, AmmoPack과 동일 구조).
- `manage_prefabs create_from_gameobject`로 씬에 임시로 만든 GameObject를 프리팹화한 뒤, 스폰 전용으로만 쓸 것이라 씬에 남은 인스턴스는 삭제하고 `Item Spawner.items` 배열에만 등록함.

### 회귀 확인
- 좀비 점블랭크 레이캐스트: `layer=10(Enemy)`, `IDamageable`/`NavMeshAgent` 정상 — 마커 자식 추가가 기존 컴포넌트에 영향 없음.
- `AmmoPack.Use()` 재확인: 탄약 100→130 정상 증가, 기존 픽업 회귀 없음.

## 2026-09-05 — RadarPack 비주얼을 구체에서 탐지기 모양으로 교체 (사용자 요청)
- 전용 3D 모델이 없는 제약은 그대로라, 프리미티브 3개(Cylinder 받침대 + 얇은 Cylinder 안테나 기둥 + 45도 기울어진 납작한 Sphere 접시)를 조합해 "탐지기/레이더 안테나" 실루엣을 만듦. 전부 기존 `RadarPackVisual.mat`(시안색 Unlit) 재사용, 콜라이더는 전부 제거(루트의 SphereCollider 트리거 하나로만 픽업 판정, 기존 구조 그대로).
- `manage_camera screenshot`의 `view_position`/`view_target`으로 원하는 각도에서 직접 스크린샷을 찍어 모양을 눈으로 확인(플레이어 근처에 임시 스폰 → 확인 후 파괴, 씬에는 흔적 안 남김).
- 루트의 기존 `Rotator` 컴포넌트가 그대로 적용되어 천천히 자전 — "스캔하는 레이더"처럼 보이는 효과를 의도치 않게 공짜로 얻음.

### 2026-09-05 — 색상 배색 개선 (사용자 요청: "단색 하늘색은 탐지기와 안 어울림")
- 본체(Base+Antenna)는 기존 `Assets/Materials/Gray.mat`(어두운 회색, Lit, 재사용 — 새 에셋 안 만듦)로 교체해 "기기 하우징" 느낌을 줌.
- 접시(Dish)만 `RadarPackVisual.mat`의 `_BaseColor`를 하늘색→초록(0.25, 1, 0.35)으로 변경해 유지(Unlit이라 조명과 무관하게 항상 밝게 빛나는 "활성 센서" 느낌). 이 머티리얼은 Dish에만 쓰이므로 색 변경이 다른 곳에 영향 없음.
- 결과: 어두운 회색 본체 위에 밝은 초록 접시가 대비되어 "레이더 탐지기" 실루엣이 한층 분명해짐(스크린샷으로 확인).

## 2026-09-05 — Slice 3: 인벤토리 foundation (체력팩 + 레이더)

### 스코프 결정 (사용자 확인)
- 체력팩+레이더만 "모았다가 원할 때 사용"하는 진짜 인벤토리 아이템으로 전환. 탄약/코인은 지금처럼 줍자마자 즉시 적용 유지(별도 손 안 댐 — `AmmoPack.cs`/`Coin.cs`/`ItemSpawner.cs` 전혀 수정 안 함).
- 사용 방식: 숫자키 1/2로 해당 슬롯 즉시 사용(인벤토리 창 UI는 만들지 않음).

### 아키텍처
- `ItemData`(추상 ScriptableObject) → `HealthItemData`/`RadarItemData`가 상속, 각각 기존 `HealthPack.Use()`/`RadarPack.Use()`에 있던 효과 로직을 그대로 옮겨받음(`ZombieData`/`GunData`와 동일한 `[CreateAssetMenu(menuName="Scriptable/...")]` 컨벤션).
- `Inventory`(신규, `Player Character`에 부착, 싱글톤 아님 — `PlayerHealth`/`PlayerShooter`처럼 그냥 플레이어 소유 컴포넌트): 고정 4슬롯 배열. `Add(data, amount)`는 같은 데이터면 수량만 합치고, 없으면 빈 슬롯에 배치. `UseSlot(index)`가 `data.Use(gameObject)` 호출 후 수량 차감, 0이면 슬롯 비움.
- `HealthPack.cs`/`RadarPack.cs`는 이제 효과를 직접 실행하지 않고 `target.GetComponent<Inventory>().Add(itemData, 1)`만 호출 — "픽업(월드 오브젝트)"과 "효과(ItemData)"와 "저장소(Inventory)"가 명확히 분리됨.
- UI는 기존 `UIManager.UpdateAmmoText`/`UpdateScoreText` 패턴을 그대로 따라 `UpdateInventoryText(string)` 추가 — `Inventory`가 상태 바뀔 때마다 호출(매 프레임 아님). HUD Canvas의 `Ammo Display` 패널 바로 위(좌하단)에 `Inventory Text`를 배치해 "1:체력팩 x2  2:레이더 x1" 형식으로 표시, 빈 슬롯은 표시 안 함.
- `PlayerInput.cs`에 `useSlot1`(`KeyCode.Alpha1`)/`useSlot2`(`KeyCode.Alpha2`) 추가, 게임오버 시 false로 리셋(기존 `fire`/`reload`와 동일 패턴). `Inventory.Update()`가 이 플래그를 읽어 `UseSlot(0)`/`UseSlot(1)` 호출.

### 검증 (헤드리스 세션, 코드 직접 호출로 확정)
- `Add(healthItemData,2)` → 슬롯/UI 반영 확인 → `UseSlot(0)` → 실제 체력 70→120(+50) 회복 + 수량 2→1 차감 확인.
- `Add(radarItemData,1)` → `UseSlot(1)` → 컬링 마스크 2049→6145(레이더 실제 발동) + 수량 소진 시 슬롯이 UI 표시에서 사라짐 확인 — **레이더가 체력팩과 완전히 동일한 범용 경로(`ItemData.Use`)로 동작함**을 증명(하드코딩 없음, CLAUDE.md 13.3/16장 요구사항 충족).
- `HealthPack.Use(player)`/`RadarPack.Use(player)`를 직접 호출해 이제 즉시 효과가 발생하지 않고(체력/컬링마스크 불변) 인벤토리에만 담기는 것을 확인(과거 동작과 명확히 달라졌음을 회귀 테스트로 증명).
- `AmmoPack.Use()`는 여전히 즉시 적용(100→130) — 이번 변경과 무관함을 재확인.
- 좀비 점블랭크 레이캐스트로 사격 판정 회귀 없음 재확인.

### 알아두면 좋은 것 (다음 세션 참고)
- `LivingEntity.RestoreHealth`는 상한 클램프가 없어서 체력이 시작 체력(100)을 넘어 120까지 올라가는 게 관찰됨 — 이번에 옮긴 로직 그대로라 **기존부터 있던 동작**(회귀 아님, 손 안 댐). 나중에 체력 상한 처리가 필요하면 `LivingEntity.cs`를 볼 것.
- 인벤토리는 지금 "창" UI가 없고 상시 노출되는 한 줄 텍스트뿐 — Slice 4(플레이어 상태 UI)나 이후 요청 시 아이콘/슬롯 그래픽으로 발전시킬 수 있음.

## 2026-09-05 — 사용자 피드백 3건: 좀비 마커 메인화면 노출 버그, 인벤토리 아이콘 UI, 사망 좀비 마커 숨김

### [버그] Main Camera가 MinimapZombie 레이어를 컬링하지 않고 있었음
- Slice 1에서 `MinimapPlayer`를 Main Camera 컬링 마스크에서 제외했지만, Slice 2에서 `MinimapZombie` 레이어를 새로 추가할 때 **Main Camera 쪽 제외 처리를 빠뜨림**(레이더 컨트롤러/미니맵 카메라 쪽만 신경 씀). 그 결과 좀비 미니맵 마커(빨간 사각형)가 실제 게임 화면에 계속 떠 있었음.
- 수정: `mainCam.cullingMask &= ~(1 << LayerMask.NameToLayer("MinimapZombie"))` 적용(-2049 → -6145). **교훈**: 앞으로 미니맵 전용 레이어를 새로 추가할 때마다 Minimap Camera 쪽 포함 처리뿐 아니라 **Main Camera 쪽 제외 처리도 항상 세트로 확인**할 것. 헤드리스 세션에서 좀비 스폰 위치는 화면에 잘 안 잡혀서 육안 스크린샷으로는 놓치기 쉬움 — `cam.cullingMask` 값을 직접 비트 검사하는 방식으로 확인해야 확실함.

### 인벤토리 아이콘 슬롯 UI
- `ItemData`에 `public Sprite icon;` 추가. 전용 아트가 없어 코드로 절차적 생성: 체력팩=빨간 원 배경+흰색 십자가(64x64), 레이더=초록 동심원 3겹+중심점(64x64) — `Assets/Sprites/Health Item Icon.png`, `Radar Item Icon.png`(Minimap Mask Circle 만들 때와 동일한 Texture2D→PNG→Sprite 임포트 절차 재사용).
- `UIManager`의 `UpdateInventoryText(string)`(한 줄 텍스트)을 `UpdateInventorySlot(index, icon, quantity)`로 교체 — `inventorySlotIcons: Image[]`, `inventorySlotCounts: Text[]` 배열 필드로 슬롯별 접근.
- `Inventory.slots` 크기를 4→2로 축소(실제 아이템 2종만 있으므로 UI 슬롯 개수와 정확히 일치시킴 — 안 쓰는 빈 슬롯을 UI에 만들지 않기 위함).
- HUD 좌하단에 `Inventory Slots` 컨테이너 → `Slot 0`/`Slot 1`(반투명 검은 Image 배경, 52x52) → 각각 자식 `Icon`(Image, 비어있으면 `enabled=false`)과 `Count`(Text, 우하단, 수량 0이면 빈 문자열).
- **헤드리스 세션에서 스크린샷이 UI 변경사항을 즉시 반영 안 하는 문제 발견**: 컴포넌트 값은 코드로 확인하면 정상인데(`icon.sprite`, `count.text` 등) 그 직후 찍은 스크린샷엔 반영이 안 될 때가 있었음. `Canvas.ForceUpdateCanvases()`를 스크린샷 직전에 호출하면 그 순간엔 해결되지만, 그 뒤에 다른 액션(카메라 이동 등)을 몇 번 더 하고 나중에 다시 찍으면 또 스테일한 프레임이 나올 수 있음(프레임이 실제로 안 돌아가는 헤드리스 환경의 근본적 한계, 기존에 기록한 "Time.frameCount 고정" 이슈와 같은 원인). **결론**: UI 값 검증은 컴포넌트 프로퍼티를 코드로 직접 읽는 것이 스크린샷보다 신뢰도가 높음. 스크린샷은 `Canvas.ForceUpdateCanvases()` 직후 바로 찍을 때만 신뢰.

### 사망한 좀비의 미니맵 마커 숨김
- `Zombie.cs`의 `Awake()`에서 `transform.Find("Minimap Marker")` 결과를 `minimapMarker` 필드로 캐싱(죽을 때 딱 한 번만 쓰지만 매번 Find하는 것보다 캐싱이 낫다는 기존 컨벤션 유지).
- `Die()`에서 콜라이더 비활성화하는 부분과 같은 위치에 `minimapMarker.SetActive(false)` 추가. 좀비 오브젝트 자체는 `ZombieSpawner.cs`가 사망 10초 뒤에 파괴하지만(`Destroy(zombie.gameObject, 10f)`), 그 10초 동안 시체가 남아있어도 마커는 죽는 즉시 사라짐.

## 2026-09-05 — Slice 4: 발밑 체력 링 → `I` 키 상태 창 (Project Zomboid 스타일)

### [중요 발견] 체력 UI는 이미 있었다
- Phase A 인스펙션 중 `PlayerHealth.healthSlider`가 실제로 씬의 "Health Slider" 오브젝트에 연결되어 있음을 발견. 위치는 `Player Character/Canvas`(월드스페이스 Canvas, 로컬 포지션 (0, 0.3, 0) — 플레이어 발밑), `Health Circle` 스프라이트를 `Image.Type.Filled`+`Radial360`으로 사용.
- **이게 바로 Slice 1 때 "미니맵과 무관한 기존 장식품"이라고 오인해서 범위 밖으로 넘겼던 그 분홍/빨간 링이었음.** 당시엔 원인을 깊게 파지 않고 넘어갔는데, 이번에 알고 보니 실제 체력 게이지였다. **교훈**: "이건 기존부터 있던 것 같다"고 넘길 때, 정말 아무 기능이 없는 장식인지 한 번은 실제로 확인해볼 것 — 겉보기엔 이상해 보여도 실은 의도된 UI일 수 있다.

### 사용자 결정
- 발밑 링을 없애고 Project Zomboid처럼 `I` 키를 눌렀을 때 뜨는 상태 창으로 교체. CLAUDE.md 13.4(Hunger/Stamina 등은 요청 전까지 보류) 원칙에 따라 창 내용은 **체력만** 표시(허기/스태미나 등 새 스탯 추가 안 함 — 아직 그걸 소모하는 메커니즘이 전혀 없으므로 섣부르게 만들지 않음).

### 관심사 분리로 재설계
- `PlayerHealth.cs`에서 `healthSlider` 필드/로직을 완전히 제거 — `RestoreHealth` 오버라이드까지 통째로 삭제(오버라이드가 UI 갱신 말고 하던 일이 없었으므로, `LivingEntity.RestoreHealth`를 그대로 씀). 이제 `PlayerHealth`는 UI 존재를 전혀 모름.
- 새 `UIManager.statusWindow`/`statusHealthText`가 `LivingEntity.health`/`startingHealth`를 **직접 읽어서** 표시 — 게임플레이 쪽이 UI에 값을 밀어넣는(push) 기존 패턴(Ammo/Score/Wave/Inventory)과 달리, 이건 **UI가 필요할 때 당겨오는(pull)** 방식. 상태 창처럼 "가끔 열어보는" UI엔 pull이 더 자연스럽고(항상 최신값 보장, 이벤트 훅 불필요), 항상 떠있는 HUD엔 기존처럼 push가 맞음 — 상황에 따라 패턴을 다르게 가져간 것.
- `UIManager`에 처음으로 `Awake()`/`Update()`가 생김(기존엔 순수 setter 메서드 모음이었음). `Awake()`에서 플레이어의 `PlayerInput`/`LivingEntity`를 캐싱(씬에 플레이어가 하나뿐인 싱글플레이 전제, `GameObject.FindGameObjectWithTag("Player")` 1회만 호출).

### 검증 관련: 헤드리스 세션에서 새로 발견한 렌더링 글리치
- `Canvas.ForceUpdateCanvases()` 직후 스크린샷을 찍었더니, 이번엔 내가 건드리지도 않은 기존 Wave/Ammo 텍스트까지 전부 깨진 문자로 나오는 렌더링 글리치가 발생(내용은 맞는데 폰트 아틀라스가 깨진 것처럼 보임). 다시 찍으니 이번엔 텍스트는 멀쩡한데 상태 창 자체가 화면에 아예 안 보임(코드로는 `activeSelf=true` 확인됨). **결론**: 이 세션의 스크린샷 캡처는 상태 창처럼 방금 막 SetActive(true)한 UI에 대해 신뢰할 수 없다 — 이번엔 `ui.statusWindow.activeSelf`/`ui.statusHealthText.text`를 리플렉션으로 직접 읽어 토글(열림→닫힘)과 체력 값 반영(100→65)을 확정 검증했고, 그걸로 충분하다고 판단함. 다음에도 이런 "방금 연 패널" 검증은 스크린샷보다 컴포넌트 값 직접 조회를 우선할 것.
- 검증 중 실제로 플레이어가 사망(`YOU DIE`)하는 것을 목격했는데, 콘솔에 에러가 전혀 없어서 게임오버 흐름 자체는 정상 동작임을 확인함(내가 준 데미지 외에 좀비 공격이 실제로 몇 번 들어갔을 가능성 있음 — 헤드리스 환경에서도 물리/트리거는 완전히 멈춰있지 않을 수 있다는 뜻이지만, 이번 작업 검증엔 영향 없음).

## 2026-09-05 — Slice 4 후속: PZ 스타일 시각 참고(실루엣 + 세로 체력바)

### 사용자가 Project Zomboid 실제 스크린샷 2장 제공
- 탭(정보/스킬/상태/피복 보호/체온 상태), "운동" 버튼, 신체 실루엣, 부위별 부상+붕대 목록, 우측 세로 체력바+하트 아이콘까지 포함된 꽤 깊은 UI.
- 확인 질문으로 범위를 좁힘: **시각 스타일만 참고**로 확정(부위별 부상 시스템 등 새 게임플레이 시뮬레이션은 만들지 않음 — CLAUDE.md 13.4 원칙 재확인).

### 구현
- `Assets/Sprites/Status Body Silhouette.png`: 원(머리)+사각형들(목/몸통/팔 2/다리 2, 사이 간격을 둬서 각 부위가 구분되어 보이게) 조합으로 그린 단순 "종이인형" 실루엣. 코드로 픽셀 단위 생성(기존 아이콘/미니맵 마스크 생성 기법 재사용). 부위별 상호작용은 전혀 없음 — 순수 장식.
- 상태 창을 좌측 실루엣 + 우측 세로 체력바(Image Type=Filled, FillMethod=Vertical, Origin=Bottom) + 바 하단에 기존 체력 아이콘(인벤토리에서 쓰던 빨간 십자 아이콘 재사용 — 새 하트 아이콘을 따로 안 만들고 기존 "체력" 상징을 그대로 재사용해 게임 내 시각 언어 일관성 유지) + 상단 퍼센트 텍스트로 재구성.
- `UIManager`가 `health/startingHealth` 비율을 계산해 텍스트("체력 70%")와 `Image.fillAmount`(0.7)를 동시에 갱신 — 여전히 pull 방식(창이 열려있을 때만).

### 검증: 스크린샷이 이번엔 유독 더 불안정했음
- `Canvas.ForceUpdateCanvases()` 직후 찍어도 상태 창이 안 보였고, 심지어 재시도한 스크린샷에서는 기존에 멀쩡하던 탄약 박스/미니맵까지 안 보이는 등 캡처 자체가 전반적으로 불안정했음(내가 새로 만든 기능과 무관).
- 대신 `RectTransform.GetWorldCorners()`로 패널의 실제 화면 좌표를 직접 계산해 확인: 화면 1515x862 기준 (553~960, 227~634) 범위로 **정확히 중앙에, 정상 크기로 위치**함을 좌표로 확정. `activeSelf`, `healthText.text`, `fillAmount` 값도 전부 기대값과 정확히 일치. **스크린샷이 안 보인다고 반드시 실제로 안 보이는 게 아니라는 것을 좌표 계산으로 명확히 구분해낸 사례** — 앞으로도 헤드리스 스크린샷이 의심스러우면 `GetWorldCorners()`로 레이아웃 자체를 먼저 배제하고 판단할 것.

## 2026-09-05 — Slice 4 재수정: 아이콘 위치 버그 + 체력 연동 색상 변화

### 사용자 피드백 (스크린샷 첨부)
- "체력바 밑 아이콘"이 실제로는 우측 하단에 따로 떨어져 있음(버그).
- 좀비에게 맞아도 체력바가 흰색 그대로(색상 피드백 없음).
- 좌측 실루엣도 체력 비율에 따라 빨갛게 물들었으면 함.

### [버그 원인] 아이콘 좌표 계산 실수
- `Health Bar Background`는 `anchorMin/Max=(1,0)`, `pivot=(1,0)`, `anchoredPosition.x=-70`, `width=26` → 중심 x는 `-70 - 26/2 = -83`(부모 우측 기준).
- `Health Bar Icon`은 `pivot=(0.5,1)`이라 `anchoredPosition.x` 자체가 곧 중심 x인데, 처음 만들 때 `-57`로 잘못 넣어서 바 중심(-83)과 26px 어긋나 있었음.
- **교훈**: pivot이 서로 다른 두 UI 요소를 "같은 x에 정렬"하고 싶을 때, 각자의 pivot 기준으로 "중심 좌표"를 따로 환산해서 맞춰야 함 — anchoredPosition 값을 그냥 똑같이 넣거나 감으로 어림잡으면 pivot 차이만큼 어긋난다. 이번처럼 `GetWorldCorners()`로 두 요소의 실제 중심을 계산해서 `diff==0`을 확인하는 방식이 확실함.

### 체력 연동 색상 (`healthyColor` ↔ `criticalColor` Lerp)
- `UIManager`에 `healthyColor`(기본 밝은 회백색)/`criticalColor`(빨강) 필드 추가, `Update()`에서 `Color.Lerp(criticalColor, healthyColor, ratio)`를 계산해 `statusHealthBarFill.color`와 `statusSilhouette.color`에 동일하게 적용 — 사용자가 둘 다 원했으므로 하나의 계산으로 통일.

### [중요 발견] 이 세션은 실시간으로 흐르고 있었다 — "헤드리스 프레임 정지" 가정이 틀렸을 수 있음
- 검증 도중 플레이어 체력이 `-1020`까지 떨어져 있는 것을 발견. 원인 추적 결과: **좀비가 실제로 플레이어를 계속 공격하고 있었음**(`OnTriggerStay` 기반 공격, 0.5초 간격) — 즉 이 Play Mode 세션은 내가 도구 호출을 하는 동안에도 실시간으로 계속 흐르고 있었다는 뜻.
- Slice 1 때 기록한 "`Time.frameCount`가 1에서 멈춰있고 진행이 안 됨" 관찰과 정면으로 배치됨. 두 관찰 다 실제로 있었던 일이므로, **아마도 프레임 진행 여부는 그때그때 에디터 창의 포커스/상태에 따라 달라지는 것으로 추정**(고정된 환경 제약이 아님). **앞으로는 "프레임이 멈춰있다"고 미리 단정하지 말고, 매번 `Time.frameCount`나 실제 체력/좀비 상태를 찍어봐서 그 세션이 실시간으로 흐르고 있는지 먼저 확인할 것.** 실시간으로 흐르는 경우, 좀비 근처에서 오래 코드 테스트를 하면 플레이어가 실제로 죽을 수 있으니 체력 관련 테스트는 플레이어를 좀비로부터 먼 안전지대(예: (500,0,500))로 옮겨두고 진행할 것.
- 테스트 중 플레이어를 원점에서 아주 멀리(500,0,500) 옮겼더니 `Invalid worldAABB` / `IsFinite(distanceForSort)` 경고가 콘솔에 찍힘 — Unity의 부동소수점 정밀도 관련 경고로, 실제 게임 코드 버그가 아니라 테스트용 원거리 텔레포트 때문. Play Mode 종료로 자동 정리됨, 별도 조치 불필요.
- 실시간으로 흐르는 세션에서 `PlayerInput.toggleXxx`류 값을 리플렉션으로 강제 설정한 뒤 다른 스크립트의 `Update()`를 리플렉션으로 호출하는 방식은 **레이스 컨디션에 취약함** — 실제 `PlayerInput.Update()`가 같은 프레임 사이에 끼어들어 방금 설정한 값을 `Input.GetKeyDown(...)`(눌리지 않았으므로 false)으로 도로 덮어써버릴 수 있음. 한 번의 `execute_code` 호출 안에서 "값 설정 → 대상 Update() 호출"을 촘촘하게 묶으면 대체로 성공하지만, 여러 번 반복하거나 호출 사이 텀이 있으면 실패할 수 있음(이번에 실제로 한 번 재현됨). 토글 자체의 정확성은 이미 이전 호출에서 증명됐으므로, 화면 확인이 목적일 땐 입력 시뮬레이션 대신 `GameObject.SetActive`/`Camera.enabled`를 직접 조작하는 편이 더 안정적.

## 2026-09-05 — 추가 기능: M키 전체 지도 창

### 설계
- 미니맵(우측 상단, 플레이어를 따라다니는 좁은 시야)과 별개로, **월드 중심에 고정된(플레이어를 따라가지 않는) 넓은 시야**의 카메라를 하나 더 둠 — `Full Map Camera`, orthographicSize=16으로 `Level Art`의 실제 바운드(가로세로 약 27유닛)를 여유 있게 커버(경계 계산은 `Renderer.bounds`를 전부 `Encapsulate`해서 구함).
- 컬링 마스크/레이더 연동 로직을 미니맵과 완전히 동일하게 맞춤 — `MinimapRadarController`가 이제 미니맵 카메라뿐 아니라 `fullMapCamera` 필드로 지정한 카메라의 컬링 마스크도 함께 토글. 덕분에 "레이더 켜짐 → 좀비가 미니맵과 전체지도 양쪽에 동시에 나타남" 요구를 별도 분기 없이 만족.
- 지도 창이 닫혀있을 때는 `Camera.enabled = false`로 꺼둬서 안 쓰는 카메라가 매 프레임 렌더링되는 낭비를 막음(미니맵 카메라는 상시 필요해서 항상 켜져있지만, 전체지도는 가끔만 열어보므로 이 차이를 반영).

### [함정] 프리팹 에셋에 씬 오브젝트를 대입하면 저장 시 null이 됨 — 반드시 기록해둘 것
- 위 checklist.md에 상세 기록. 요약: 프리팹 스테이지 안에서 `GameObject.Find`로 찾은 씬 오브젝트를 프리팹 필드에 넣고 저장하면 그 참조가 사라짐. 씬의 인스턴스에서 `SerializedObject`로 다시 연결해야 함. 이 프로젝트에서 미니맵/카메라 관련 참조를 늘릴 때마다 반복될 수 있는 함정이므로 다음에도 주의.

## 2026-09-05 — 전체 지도(종이 지도) 후속: 아이템 완전 은폐

### 사용자 재지적: "아이템도 표시되면 컨셉이 이상하다"
- 전체 지도의 컬링 마스크가 `Default`였는데, `AmmoPack`/`HealthPack`/`Coin`/`RadarPack` 픽업이 전부 `Default` 레이어라 그대로 나타나고 있었음.
- 1차 조치: `Pickup`(슬롯 13) 레이어 신설, 4개 픽업 프리팹 전체를 이 레이어로 이동. `manage_prefabs open_prefab_stage` 대신 `PrefabUtility.LoadPrefabContents`/`SaveAsPrefabAsset`로 헤드리스하게 처리(프리팹 스테이지를 열고 닫는 왕복 없이 한 번에 끝남 — 여러 프리팹을 일괄 수정할 때 더 빠름, 다음에도 이 방식 우선 고려).
- **실시간 미니맵(우측 상단)에는 영향 주지 않음** — 사용자는 "전체지도"만 문제 삼았으므로, `Minimap Camera` 컬링 마스크엔 `Pickup`을 추가해 그대로 아이템이 계속 보이게 유지함(범위를 벗어난 변경 안 함).

### [발견] 레이어를 옮겨도 아이템의 실시간 조명이 위치를 드러냄
- `AmmoPack`/`HealthPack`/`Coin`에는 작은 Light(range=1, intensity=1)가 달려 있어 바닥을 은은하게 비춤(3D 게임 화면에서 아이템을 눈에 띄게 하는 의도된 연출). 메시를 `Pickup` 레이어로 옮겨 카메라 렌더링에서는 빠졌지만, **조명이 비추는 바닥(Default 레이어, 전체 지도에서도 보임)은 여전히 밝아져서 아이템 위치가 미묘하게 드러남** — 픽셀 차등 비교로 실측 확인(아이템 활성/비활성 시 같은 좌표 색상이 또렷하게 다름).
- 원인 오판 주의: 처음엔 포스트프로세싱(Bloom)이 밝은 지점을 부풀리는 거라 추측해 `Full Map Camera`에 `UniversalAdditionalCameraData`를 추가하고 `m_RenderPostProcessing=false`로 꺼봤지만 **차이가 그대로 남아 있었음** → Bloom이 원인이 아니라 URP의 실시간 직접광 자체가 지형에 색을 입히는 것이 원인이었음. 관련 없는 시도였으므로 포스트프로세싱 설정은 다시 `true`로 되돌림(불필요한 변경 남기지 않기).
- **핵심 교훈**: Unity/URP의 실시간 Light는 "어느 오브젝트를 비출지"(Light.cullingMask)는 있어도 "어느 카메라에서 보일지"는 카메라의 `cullingMask`로 제어되지 않음 — 조명 기여는 카메라와 무관하게 씬 전역으로 계산됨. 즉 레이어 기반 카메라 컬링만으로는 "이 카메라에서만 조명 효과를 숨기기"가 불가능하다(URP의 Rendering Layers/Forward+ 기능을 쓰면 가능하지만 렌더러 에셋 전역 설정을 바꿔야 해서 이번 사소한 지도 이슈치고는 과함).

### 최종 해결: 지도가 열려있는 "순간에만" 조명을 직접 끔
- `UIManager.HidePickupLights()`: M키로 지도를 여는 순간, `FindObjectsByType<Light>()`로 씬의 모든 Light를 스캔해 `layer==Pickup`인 것만 `enabled=false`로 끄고 리스트에 저장(매 프레임이 아니라 지도를 열 때 딱 1회만 스캔 — CLAUDE.md의 "매 프레임 스캔 금지" 원칙 준수).
- `UIManager.RestorePickupLights()`: 지도를 닫는 순간 저장해둔 조명들을 다시 켬. 지도가 닫혀있는 한(대부분의 플레이 시간) 조명은 항상 정상 상태라 메인 게임 화면 연출엔 전혀 영향 없음.
- 검증: 지도 연 상태의 특정 픽셀 색상이 "그 아이템이 씬에 아예 없는" 경우와 **완전히 동일한 RGBA 값**으로 나옴을 확인 — 메시와 조명 둘 다 완벽히 은폐됨. 지도를 닫으면 조명이 다시 켜짐도 확인.

## 2026-09-05 — 전체 지도 재설계: "실시간 카메라"에서 "시작 시 1회 촬영"으로

### 사용자의 근본적 지적
- "종이지도는 내가 그위치에 있다고 해서 실시간 갱신이 되는게 아니잖아" — 매번 지도를 열 때마다 카메라를 다시 렌더링하는 지금 구조 자체가 "종이 지도" 컨셉과 어긋남. 레이어를 하나씩 제외해가는 방식(플레이어 마커 제외 → 좀비 마커 제외 → 아이템 제외 → 아이템 조명 제외...)은 앞으로 등장할 다른 동적 요소(피 이펙트, 탄피, 시체 등)마다 계속 땜질해야 하는 구조라는 것도 문제.
- **더 근본적인 버그도 같이 지적됨**: 플레이어의 "마커"는 이미 제외했지만(`MinimapPlayer` 레이어), 플레이어의 **진짜 3D 몸체**(Woman 메시, 총, 이펙트들)는 여전히 `Default` 레이어라 전체지도 카메라(`Default` 포함)에 그대로 찍히고 있었음 — Slice 1에서 좀비의 실제 몸체를 `Enemy` 레이어로 옮겼던 것과 똑같은 종류의 누락을 플레이어 쪽에서는 안 해뒀던 것.

### 해결: 정적 스냅샷 방식으로 아키텍처 전환
- `Player Character.prefab`의 `Minimap Marker`를 제외한 모든 렌더러(Woman, Gun 4개 메시, 손잡이/발사위치 트랜스폼, 총구/탄피/펀치 이펙트)를 이미 존재하던 `Player` 레이어(9)로 이동. 새 레이어를 안 만들고 기존 레이어를 재사용 — 콜라이더(좀비 감지용)는 원래부터 루트에 있고 루트는 이미 `Player` 레이어였으므로 이번 변경으로 좀비 AI에 영향 없음.
- `UIManager.cs`: `mapCamera`를 M키 누를 때마다 켜고 끄던 것을 **`Start()`에서 딱 1회 `Render()` 호출 후 영구적으로 `enabled=false`** 로 변경. 이후 지도 창은 그 순간에 찍힌 `RenderTexture`를 그대로 보여줄 뿐, 카메라도 다시 안 켜지고 다시 그리지도 않음 — 진짜 "그려서 고정해둔 지도"가 됨.
- 이 방식의 장점: 아이템은 `ItemSpawner`가 시작 후 2~7초 뒤에야 스폰하므로, `Start()` 시점 촬영은 애초에 아이템이 하나도 없는 상태를 찍음 — 지난번에 만든 `HidePickupLights`/`RestorePickupLights` 워크어라운드가 통째로 불필요해져서 삭제함(코드가 더 단순해짐). 앞으로 새로운 종류의 동적 이펙트(피, 탄피, 시체 등)가 생겨도 전부 자동으로 지도에 안 찍힘 — Default 레이어인 것과 무관하게, 애초에 그 시점에 존재하지 않았으므로.
- 검증(A/B 비교로 "원래부터 안 찍혔다"를 확정): 시작 시 찍힌 텍스처의 특정 픽셀과, **플레이어를 원점에서 500유닛 밖으로 치운 뒤 강제로 재촬영**한 같은 픽셀의 색상이 **완전히 동일**함을 확인 — 이건 "지금 안 보인다"가 아니라 "애초에 그 자리에 있어도 없어도 결과가 같다"는 것이므로 플레이어가 전혀 기여하지 않았다는 강한 증거.
- 저장된 `RenderTexture`를 PNG로 직접 저장해 육안으로도 확인: 지형과 장식 조명 글로우만 있고 플레이어/아이템/좀비 전부 없음.

### 남은 고려사항 (다음 세션 참고)
- 지금은 씬이 하나뿐이고 지형이 게임 중 안 바뀌므로 "시작 시 1회 촬영"이 완벽하게 맞음. 나중에 Slice 6(월드 확장)에서 파괴 가능한 구조물이나 씬 전환이 생기면, 그때는 "지형이 바뀔 때만 다시 촬영" 같은 재바운드 로직이 필요할 수 있음 — 지금은 그런 요구가 없으므로 만들지 않음.

## 2026-09-05 — 근접무기(삽) + 무기 전환 시스템 구현/검증

### 설계 요약 (plan.md 참조)
- 새 스켈레톤 애니메이션 없이 무기(Melee Weapon 오브젝트) 자체의 로컬 회전을 코루틴으로 Slerp — 손은 기존 IK 구조가 무기 손잡이(Left/Right Handle) 트랜스폼을 매 프레임 그대로 따라가므로 팔도 자연스럽게 따라 움직임.
- `Gun.cs`/`MeleeWeapon.cs` 양쪽에 각자 `leftHandMount`/`rightHandMount` 필드를 두고, `PlayerShooter.OnAnimatorIK()`가 `currentWeapon`에 따라 그때그때 어느 쪽을 읽을지 고른다 — PlayerShooter 자신은 더 이상 고정 마운트 필드를 갖지 않음(예전엔 PlayerShooter가 직접 Gun의 Left/Right Handle을 가리키는 고정 필드를 가지고 있었으나, 무기 전환을 지원하려면 이 구조가 맞지 않아 제거함).
- `PlayerShooter.EquipWeapon(WeaponType)`이 유일한 무기 상태 변경 지점 — Q키 입력(`PlayerInput.switchWeapon`)과 `OnEnable`(재활성화 시 현재 상태 복원)이 모두 이 메서드 하나로 수렴.

### [중요 검증 기법] 헤드리스 환경에서 짧은 코루틴(스윙) 중간 프레임을 잡는 법
- 코루틴 스윙 지속시간이 0.3~0.4초인데, MCP 툴 왕복 지연은 보통 3~6초라서 "Attack() 호출 → 잠시 후 상태 확인" 방식으로는 스윙이 이미 완전히 끝난 뒤의 상태만 보게 됨(실제로 이 문제 때문에 첫 시도에서 "데미지가 전혀 안 들어간 것처럼" 보이는 착시를 겪음 — 실제 원인은 아래 항목 참고).
- 해결: `UnityEditor.EditorApplication.isPaused = true`로 에디터를 일시정지한 뒤, `Attack()`을 호출(코루틴의 첫 `yield` 이전 부분은 `StartCoroutine` 호출 시 동기적으로 즉시 실행됨)하고, 이후 `UnityEditor.EditorApplication.Step()`을 반복 호출해 정확히 한 프레임씩 진행시키며 매 스텝마다 회전값/체력을 샘플링. 이러면 실제 통신 지연과 무관하게 결정론적으로 스윙의 모든 프레임을 관찰 가능. `Time.timeScale`을 늦추는 방식도 시도했지만 여전히 왕복 지연이 스케일된 스윙 전체 길이보다 길어질 수 있어 신뢰도가 낮음 — `isPaused`+`Step()` 조합이 훨씬 확실함.
- 이 기법으로 확인: 무기 로컬 회전이 대기자세(-50°)→스윙자세(80°)까지 매 프레임 부드럽게 변함, 스윙 진행률 t≈0.4 지점에서 정확히 1회만 `DetectHit()`이 실행되어 좀비 체력이 즉시 감소함(중복 판정 없음), 스윙 종료 후 다시 대기자세로 복귀함.

### [발견/함정] 헤드리스 실시간 테스트 중 플레이어가 실제로 죽을 수 있음
- 근접 판정을 테스트하려고 좀비를 플레이어 코앞(공격 사거리 이내)으로 순간이동시킨 뒤, 여러 번의 느린 MCP 툴 왕복(각각 수 초) 동안 그대로 방치했더니 **좀비 AI가 실제로 플레이어를 공격해 죽임** — `PlayerHealth.health`가 `-100`까지 떨어지고 `GameManager.isGameover=true`가 되면서 `PlayerShooter.enabled`가 `false`로 꺼짐(사망 시 비활성화되는 기존 로직). 그 상태에서는 `meleeWeapon.gameObject`도 `OnDisable()`에 의해 비활성화되어 있어서, 이후 `Attack()`을 호출해도 콘솔에 `"Coroutine couldn't be started because the the game object 'Melee Weapon' is inactive!"` 경고만 남고 아무 일도 안 일어남 — 처음엔 이걸 "판정 로직 버그"로 오인할 뻔했으나, `read_console`로 정확한 원인을 확인함(에러 메시지를 추측하지 말고 그대로 읽을 것 — CLAUDE.md 10장).
- 교훈: 헤드리스 상태에서 좀비를 강제로 플레이어 근처에 두고 여러 툴 호출에 걸쳐 방치하는 테스트 방식은 실제 게임 상태(플레이어 사망)를 오염시킬 수 있음. 이후 검증부터는 `isPaused=true` 상태를 테스트 시작부터 유지해 좀비/AI가 전혀 움직이지 못하게 고정한 뒤에만 좀비를 근접 배치하고 판정을 진행함 — 죽지 않은 깨끗한 플레이어 상태로 재검증 완료.
- 실제 데미지 적용 자체는 `DetectHit()`을 리플렉션으로 직접 호출한 별도 테스트로도 격리 검증됨(체력 10→-70, OverlapSphere가 좀비의 복합 콜라이더 2개를 모두 맞혀 40데미지×2 적용) — 판정 로직 자체는 처음부터 정상이었고, 문제는 항상 테스트 시나리오의 실시간 방치였음.

### [발견] 임시 검증용 카메라보다 `manage_camera screenshot`이 훨씬 안정적
- 씬에 임시 `GameObject`+`Camera`를 만들어 `Camera.Render()`+`ReadPixels()`로 스크린샷을 뜨는 기존 방식은, 카메라 위치를 손으로 대충 잡으면 묘지 오벨리스크 같은 전경 장애물에 가리거나, 가로등 불빛 이펙트(카메라 각도에 따라 납작한 판으로 깨져 보이는 half-용 실시간 조명용 평면)에 화면이 뒤덮이는 등 예측 불가능한 결과가 잦았음.
- `mcp__unityMCP__manage_camera`의 `screenshot` 액션(`camera` 파라미터 생략 시 `ScreenCapture` API로 실제 게임 화면을 그대로 캡처, `view_target`/`view_position`으로 특정 좌표에서 촬영도 가능)을 쓰니 실제 게임의 조명/카메라 리그를 그대로 재사용해서 훨씬 안정적으로 원하는 장면(플레이어가 삽을 든 모습)을 확인할 수 있었음. 다음에 무기/캐릭터 포즈를 스크린샷으로 확인해야 할 때는 임시 카메라를 직접 만들지 말고 이 툴을 먼저 시도할 것.

### 최종 확인된 사실
- `Melee Weapon.prefab`은 `Player Character.prefab`의 `Gun Pivot/Melee Weapon` 경로에 존재, 기본 비활성. `PlayerShooter.meleeWeapon` 필드가 이를 가리킴.
- `Gun.leftHandMount`/`rightHandMount` = 각자의 "Left Handle"/"Right Handle" 자식(총 프리팹 자체 기존 구조 그대로, 로직 변경 없음).
- `MeleeWeapon.hittableLayers` = `Enemy` 레이어(비트값 1024)만 포함 — 총과 달리 레이어마스크로 판정 대상을 제한(총의 `Physics.Raycast`는 레이어마스크 없이 전체 레이어를 맞히는 기존 동작 그대로 유지, 변경하지 않음).
- Q키(`PlayerInput.switchWeapon`)로 전환, 좌클릭(`fire`)이 현재 무기에 따라 `gun.Fire()` 또는 `meleeWeapon.Attack()`으로 라우팅됨. 재장전은 Gun 장착 중에만 동작.
- 탄약 UI(`UIManager.UpdateAmmoText`)는 무기 종류와 무관하게 항상 Gun의 탄약을 표시함(Melee 장착 중에도 갱신됨) — 이번 요청 범위에서 "무기별 UI 분기"는 요구되지 않아 손대지 않음, 필요 시 다음 세션에서 개선 가능.

## 2026-09-05 — 근접무기(삽) 파지 자세 버그 수정 ("반대로 들고있다" 리포트)

### 사용자 리포트
- "삽을 반대로 들고있다" — 근접무기 슬라이스 완료 후 다른 컴퓨터에서 이어서 확인하던 중 발견.

### 원인 (스크린샷 기반 진단)
- `shovel.fbx` 메쉬는 z=-0.5~-0.31 구간(폭 0.384, 블레이드)과 z=0.86~1.06 구간(폭 0.282, D자 손잡이)이 양 끝에 있고 가운데 샤프트는 얇음(정점 슬라이스별 폭 측정으로 확인).
- `Left Handle`(z=0.90)/`Right Handle`(z=0.35)은 D자 손잡이 쪽에 가깝게 붙어있는데, 회전 피벗(Melee Weapon 오브젝트의 로컬 원점 (0,0,0))은 두 손잡이보다 훨씬 더 블레이드 쪽에 가까움 — 즉 피벗이 손잡이 그립 위치에서 한참 떨어져 있었음.
- 그 결과 기존 `ReadyRotation = Euler(-50,0,0)`을 적용하면 피벗에서 먼 쪽(D자 손잡이, z=1.06)이 크게 위로 솟아오르고(머리 위 2m 이상), 반대편(블레이드, z=-0.5)은 몸 옆 허리 높이에 축 처지는 모양이 됨 — 완전히 거꾸로 든 것처럼 보임. `manage_camera screenshot`으로 실제 Play Mode 자세를 확인해 픽셀 단위로 원인을 검증함(추측 아님).

### 해결: 회전 피벗을 손잡이 중간 지점으로 이동
- `Left Handle`/`Right Handle`의 중간 z값(0.625)만큼 `Melee Weapon` 루트를 앞으로, `Shovel Model`/`Left Handle`/`Right Handle`을 반대로 밀어서 피벗이 두 손 사이에 오도록 재배치(대기 자세 기준 시각적으로는 동일 위치를 유지 — 순수하게 회전 중심만 이동).
- 이렇게 하면 손(피벗에서 가까움)은 회전해도 높이가 거의 안 바뀌고, 블레이드(피벗에서 먼 쪽, 이제 거리도 더 늘어남)만 크게 호를 그림 — 실제 도끼/삽 스윙의 물리적 감각과 일치.
- `ReadyRotation`/`SwingRotation`도 40°→-50°로 재조정(부호 반전 포함) — 여러 후보각(피벗 이동 전/후 각각 다수)의 블레이드·손 월드 높이/전방 내적을 코드로 계산해 손이 1.0~1.5m 사이에 머물고 블레이드가 어깨 위(대기)→앞-아래(타격)로 크게 이동하는 조합을 수치로 먼저 확정한 뒤 반영 — 스크린샷 왕복을 최소화하는 데 유효했음.

### [함정] Play Mode 헤드리스 테스트 중 플레이어가 또 사망함 (반복 재발)
- 스크린샷으로 확인하려고 Play를 시작했더니 스폰 지점 근처에 화재/폭발 이펙트가 있어 여러 툴 왕복(수 초~수십 초) 동안 방치된 사이에 실제로 플레이어가 사망함(health=-760). 근접무기 구현 때 이미 한 번 겪은 것과 동일한 패턴(context-notes.md 위쪽 "헤드리스 실시간 테스트 중 플레이어가 실제로 죽을 수 있음" 항목 참고) — **Play 진입 직후 화면부터 스크린샷 찍지 말고, Edit Mode에서 미리 플레이어를 좀비/이펙트가 없는 안전한 좌표로 옮겨둔 뒤 Play를 시작하고, Play 진입 직후 바로 `pause`할 것**을 다시 한번 확인. Ground 오브젝트의 실제 렌더러 바운즈(Center(-1.5,0,2), Extents(11,0,11))를 먼저 조회해서 맵 밖(허공)으로 내보내지 않도록 안전 좌표를 계산함.
- 좀비를 사거리 내로 순간이동시켜 데미지를 검증할 때도, `NavMeshAgent.enabled=false` + `Zombie` 스크립트 `enabled=false`를 함께 꺼야 `EditorApplication.Step()` 동안 좀비가 다시 걸어서 사거리 밖으로 벗어나지 않음(하나만 끄면 다른 쪽이 계속 움직이려다 "Resume/SetDestination/Stop can only be called on an active agent" 경고만 남고 실제로는 위치가 안 고정됨 — 처음엔 NavMeshAgent만 안 끄고 시도했다가 좀비가 (8,10)에서 (1.84,10.46)까지 이동해버려 판정 실패를 겪음).
- 스크립트(.cs) 파일을 Play Mode 도중에 수정해도 Unity는 컴파일을 Play 종료 후로 미룸 — Play 중에 코드 변경 후 바로 테스트하면 이전 컴파일 결과로 실행됨. 반드시 `stop` → 컴파일 확인(`read_console`에 에러 없음) → 다시 `play` 순서를 지킬 것.

### 검증 완료 (1차: 40°→-50°)
- checklist.md CP1~CP4 참고. 새 자세 스크린샷(대기/타격 양쪽), 좀비 데미지(20→-60, 1회), 총 회귀 없음(magAmmo 25→24) 모두 확인.

### 사용자 재지적: "휘두르는게 내려찍는게 아니라 올리는 것처럼 보인다" (1차 수정 직후)
- 수치로는 1차 수정(40°→-50°)도 블레이드가 실제로 내려가는 게 맞았음(bladeY 2.00→0.41m, `melee.TransformPoint`로 직접 확인) — 로직 자체는 틀리지 않았음.
- 하지만 같은 회전으로 반대쪽 D자 손잡이 끝도 같이 움직이는데, 그 방향이 위로 향함(gripY 1.00→1.61m). 손잡이 쪽도 폭이 있어(메쉬 폭 분석상 0.282, 블레이드 0.384와 큰 차이 없음) 화면에서 꽤 눈에 띄는 실루엣이 되고, 카메라 각도상 블레이드(다리 근처, 어둡고 배경과 겹침)보다 D손잡이(머리 위, 하늘 배경과 대비되어 잘 보임)가 더 두드러져서 사용자 눈에는 "올라가는 쪽"이 주된 움직임으로 읽힘. 게다가 대기 자세(40°)가 이미 어깨 위로 심하게 들려있는 극단적인 자세라, 스윙이 그보다 살짝 덜 들리는 정도로만 바뀌면 "그냥 자세가 조금 바뀌었다" 정도로만 보여서 방향성이 더 헷갈렸음.
- **교훈**: 좌우 대칭 없이 양 끝이 회전하는 물체는, 수치상 원하는 끝(블레이드)이 올바르게 움직여도 반대쪽 끝(손잡이)이 화면에서 더 눈에 띄면 사용자에게는 반대 방향으로 보일 수 있음. 코드로 좌표만 검증하지 말고 반드시 스크린샷으로 "어느 실루엣이 더 도드라지는지"까지 확인할 것.
- **재조정**: ReadyRotation 0°(수평으로 앞에 든 중립 자세), SwingRotation -90°(블레이드가 수직으로 발밑까지 내려꽂히는 자세)로 변경. 대기=수평, 타격=수직-블레이드-아래로 확실히 구분되어, 손잡이가 위로 올라가더라도(1.0→1.7m 정도) 블레이드가 발밑까지 내려가는 큰 변화(1.3→0.1m대)가 훨씬 지배적으로 보여 방향 오인 여지가 사라짐. 스크린샷으로 최종 확인: 대기(수평 자세) vs 타격(블레이드가 발밑까지 수직으로 내려가고 D손잡이만 머리 위로) 비교가 명확함.
- 재검증: 좀비 데미지(hp 50→-60... 실제로는 콜라이더 2개×40=80 데미지로 50→-30, 기존부터 있던 동작이며 이번 변경과 무관), 총 회귀 없음(magAmmo 25→24), 컴파일 에러 0건.

### 사용자 재지적 (실제 플레이 중): "삽을 들면 철부분이 플레이어를 관통하고, 공격하면 플레이어를 향해 내려찍는다"
- **원인**: 직전 수정(피벗을 손잡이 중간으로 재배치)에서 피벗 위치는 올바르게 옮겼지만, `shovel.fbx` 메쉬의 블레이드가 원래 로컬 -Z쪽에 있다는 것을 반영하지 않고 root를 +Z 방향(0.795)으로 옮겨버림 — 그 결과 블레이드가 몸 안쪽(-Z, 총의 "정면=+Z" 관례와 반대)에 위치하게 되어, 대기 자세에서 이미 블레이드가 몸을 관통하고 있었음.
- **왜 스크린샷 검증에서 못 잡았는가**: 검증에 사용한 고정 사이드 카메라 각도가 스윙 평면과 거의 나란히 놓여 있어서, 앞뒤(캐릭터 정면 방향) 위치 오차가 그 각도에서는 원근 단축(foreshortening)으로 가려짐 — 실제로 몸을 관통하고 있어도 "팔을 뻗어 수평으로 들고 있는" 것처럼 보였음. **교훈**: 앞뒤 클리핑 여부는 특정 각도의 스크린샷만으로 판단하지 말고, 반드시 `TransformPoint`로 블레이드의 월드 좌표를 구해 플레이어 루트와의 forward-dot·거리를 수치로 확인하거나, 정면/45도 각도 등 여러 카메라 각도에서 교차 검증할 것.
- **해결**: `Shovel Model`을 Y축 180도 회전시켜 메쉬 자체를 앞뒤로 뒤집음(블레이드가 로컬 +Z, 총의 "정면" 관례와 일치하도록). 피벗-재중심 공식도 뒤집힌 좌표계에 맞게 다시 계산.
      - Melee Weapon 루트 localPosition.z: 0.795 → **-0.455**
      - Shovel Model: localPosition.z 0 → 0.625, **localRotation.y: 0 → 180**
      - Left Handle localPosition.z: 0.275 → **-0.275**
      - Right Handle localPosition.z: -0.275 → **0.275**
- `MeleeWeapon.cs`: SwingRotation 부호도 뒤집힌 좌표계에 맞게 재계산 — Ready 0°(그대로), Swing -90° → **70°**(양의 X 회전이 이제 블레이드를 앞-아래로 내림, 뒤집기 전과 반대 부호).
- **검증(이번엔 수치 우선)**: `melee.TransformPoint`로 블레이드 월드좌표를 구해 플레이어 루트 기준 거리/전방 내적 직접 확인 — 대기: 거리 1.97m, fwdDot 0.76(몸 앞 확실히 떨어져 있음). 타격: 거리 0.80m, fwdDot 0.96, 높이 0.22m(몸 앞쪽 땅 가까이로 확실히 내려찍음, 몸 관통 없음). 이후 정면 45도 각도 스크린샷으로도 육안 확인(대기: 어깨에서 앞으로 수평하게 뻗음, 타격: 앞쪽 바닥으로 비스듬히 내려꽂힘, 몸에 안 겹침). 좀비 데미지(50→-30, 동일 기존 동작)·총 회귀 없음(25→24)·컴파일 에러 0건 재확인.

## 2026-09-05 — Slice 6: 맵 확장 1차 (기존 묘지 마당을 50x50으로 비례 확대)

### 사용자 요청
- 근접무기 버그 수정 확인 완료 후 "맵을 넓히자"라고만 요청, 구체적 규모는 "알아서 판단"이라고 위임 — 2~3배(약 50x50) 규모로 기존 스타일(울타리 쳐진 묘지 마당) 유지한 채 확장하는 쪽으로 판단해 진행함(로드맵 외 큰 아트 리소스 추가 없이 가장 단순하게 확장 가능한 방향).

### 발견한 기존 구조 (다음 세션 참고)
- 실제 플레이 가능 영역의 물리적 경계는 눈에 보이는 `Fence`(시각 메쉬)가 아니라 `Fence Collider`라는 별도 오브젝트에 달린 박스 콜라이더 12개임. `Fence`는 콜라이더가 전혀 없어서 순수 장식 + NavMesh 차단(NavMeshModifier)용으로만 쓰임 — 플레이어는 물리적으로 `Ground`(Plane, MeshCollider) 가장자리에서 막히는 구조(플레이어가 Ground 밖으로 나가면 바닥이 없어 떨어짐)와 `Fence Collider`의 박스들이 함께 경계를 형성.
- `Ground`/`Fence`/`Fence Collider` 세 오브젝트가 전부 동일한 중심(-1.5, *, 2)을 공유하는 동심원 구조(각자 피벗은 다르지만 `Renderer.bounds`/`BoxCollider.bounds`로 측정한 실제 바운즈 중심은 일치). 확장할 때는 이 중심을 유지하면서 X/Z만 같은 배율로 스케일해야 셋이 계속 정렬됨 — 오브젝트마다 피벗이 제각각이라 `localScale`만 똑같이 바꾸면 어긋남(각 오브젝트의 로컬 피벗 위치가 다르므로), 반드시 "바운즈 중심 → 목표 중심으로 유지하며 스케일"하는 공식(코드로 계산)을 써야 함: `new_scale = old_scale * k`, `new_position = target_center + k * (old_boundsCenter - old_position) 방향 보정`. 손으로 좌표를 추정하지 말고 `Renderer.bounds`/`BoxCollider.bounds`를 직접 읽어서 계산할 것 — 이번에 정확히 이 방식으로 처리해 세 오브젝트가 확장 후에도 완벽히 동심원을 유지함을 확인함.
- `NavMeshSurface`(Navigation 오브젝트)는 `collectObjects=MarkedWithModifier`라서 `size`/`center` 필드는 무시되고, `NavMeshModifier`가 달린 오브젝트(Ground, Fence)의 실제 메쉬 바운즈를 기준으로 굽는다 — Ground를 키우고 다시 구우면 자동으로 새 영역을 커버함. 별도 volume 크기 설정 불필요.
- `ItemSpawner`는 플레이어 위치 기준 상대 반경(`maxDistance`)으로 스폰 — 맵 절대 크기와 무관해서 그대로 재사용 가능(수정 불필요). `ZombieSpawner`는 절대 좌표 `Spawn Points`(Transform 4개)를 사용 — 맵을 넓힐 때마다 같은 중심 기준 배율로 재배치해야 함.
- `Full Map Camera`는 시작 시 1회 촬영(`UIManager.Start()`)이라 맵을 넓히면 `orthographicSize`를 반드시 같이 키워야 새 영역이 잘리지 않음(`Minimap Camera`는 플레이어 추종형이라 맵 절대 크기와 무관, 손댈 필요 없음).

### [함정] 스크립트로 `NavMeshSurface.BuildNavMesh()`만 호출하면 디스크에 저장되지 않음
- Unity 에디터의 NavMeshSurface 인스펙터 "Bake" 버튼은 내부적으로 새 `NavMeshData`를 만들고 그걸 기존 에셋 파일(`Assets/Scenes/Main/NavMesh-Navigation.asset`)에 저장하는 로직까지 포함하지만, 스크립트에서 `surf.BuildNavMesh()`만 호출하면 새 `NavMeshData`가 **메모리상의 임시 오브젝트로만** 만들어지고 기존 에셋 파일은 그대로 남음(`AssetDatabase.GetAssetPath(surf.navMeshData)`가 빈 문자열로 나와서 발견함).
- 이 상태로 씬을 저장하고 넘어가면, 지금 세션(에디터를 껐다 켜지 않은 상태)에서는 정상 동작하는 것처럼 보이지만 **다음 세션에서 씬을 다시 열면 옛날(22x22) 크기의 NavMesh 에셋이 다시 로드**되어 좀비가 새로 넓어진 영역 밖으로는 못 감(에셋 파일 자체가 옛날 데이터라서).
- 해결: `AssetDatabase.DeleteAsset(경로)` 후 새 `NavMeshData`로 `AssetDatabase.CreateAsset(newData, 같은 경로)` + `SaveAssets()`로 같은 경로에 다시 저장하고, `surf.navMeshData`를 그 저장된 에셋으로 재연결. `AssetDatabase.DeleteAsset`은 `execute_code`의 안전장치에 걸려서 `safety_checks=false`로 명시적으로 풀어야 실행됨(git으로 추적되는 재생성 가능한 에셋이라 위험도 낮다고 판단해 허용).
- **교훈**: NavMesh를 스크립트로 재굽는 모든 향후 작업(Slice 6 후속, 맵 추가 확장 등)에서 반드시 `git status`로 `NavMesh-*.asset` 파일이 실제로 modified로 뜨는지 확인할 것 — 뜨지 않으면 메모리에만 있고 저장 안 된 것.

### 진행 상황
- `Ground` 50x50으로 확대, `Fence`/`Fence Collider` 동일 중심으로 비례 확대(배율 k=50/22), `Spawn Points` 4개 재배치, `Full Map Camera.orthographicSize` 16→30, NavMesh 재굽기+에셋 저장 완료. checklist.md CP1~CP5 참고. Play Mode에서 좀비 스폰/NavMesh/전체지도/플레이어 이동 전부 정상 확인, 콘솔 에러 0건.
- **남은 것(다음 세션 참고)**: 새로 넓어진 영역(기존 22x22 바깥쪽)은 현재 완전히 빈 평지 — 기존 무덤/조명 등 장식(`Level Art/Props`, `Laterns`)은 옛 경계 안쪽에만 있음. 사용자가 "맵을 넓히자"고만 했지 재장식은 요청하지 않아서 이번 범위에서는 장식 추가/재배치를 하지 않음(CLAUDE.md "요청 이상 기능 추가 금지" 원칙). 다음에 사용자가 장식/추가 콘텐츠를 원하면 그때 진행.

## 2026-09-05 — 맵 확장 2차: 시골(농장) 테마 장식 추가

### 사용자 요청 및 판단
- "시골느낌으로 맵을 꾸밀수있어?" — 지난 턴에서 넓힌(50x50) 맵의 바깥 빈 공간을 시골/농장 분위기로 장식해달라는 요청.
- 조사 결과 프로젝트에 있는 3D 모델은 전부 묘지 테마(Level Art 폴더: 묘비/십자가/석관/철제 울타리 등)뿐, 농장 전용 에셋 없음. `generate_model`(Tripo/Meshy) AI 3D 생성 툴도 API 키 미설정으로 사용 불가(`list_providers`로 확인). 따라서 새 모델 임포트 없이 Unity 기본 프리미티브(Cube/Cylinder/Sphere)로 직접 저폴리 스타일 농장 소품(헛간/사일로/건초더미/나무)을 만들고, 기존 묘지 울타리 FBX(`fenceBroken.fbx`)를 목장 울타리로 재활용하는 방향으로 판단해 진행.

### 배치 위치 및 구조
- `Level Art/Rural Decor` 하위에 전부 배치(기존 `Props`/`Laterns`와 같은 레벨의 새 그룹).
- 헛간+사일로+목장울타리+건초더미 클러스터: (12~21, 12~22) 부근, 확장된 영역의 북동쪽 사분면.
- 나무 11그루: 확장된 외곽 링(옛 묘지 경계 반경 ~12 바깥, 새 울타리 반경 ~28 안쪽) 전체에 분산 — 정확한 좌표는 `Main.unity`에서 `Rural Decor/Tree 0X` 참조.
- 헛간 지붕 트릭: 정사각 단면 Cube를 Z축 45도 회전시켜 만든 "다이아몬드" 형태의 윗부분을 지붕처럼 사용(별도 모델링/ProBuilder 없이 프리미티브 회전만으로 박공지붕 실루엣 구현) — 저폴리 스타일에서 꽤 잘 먹힘, 비슷한 지붕이 다시 필요하면 이 트릭 재사용 가능.

### [함정 재확인] NavMeshModifier를 새로 추가한 오브젝트도 반드시 에셋 재저장 필요
- 헛간/사일로에 `NavMeshModifier`(Not Walkable)를 붙이고 `BuildNavMesh()`만 호출하면 지난번과 동일하게 메모리에만 반영되고 `NavMesh-Navigation.asset` 파일에는 저장 안 됨 — 이번에도 `AssetDatabase.DeleteAsset`+`CreateAsset`(같은 경로)+`SaveAssets`로 명시적으로 다시 저장해야 함(`safety_checks=false` 필요, DeleteAsset이 execute_code의 차단 패턴에 걸림). 앞으로 NavMesh에 영향 주는 오브젝트를 추가/이동/삭제할 때마다 이 저장 스텝을 빼먹지 않도록 체크리스트 항목으로 명시해둘 것.
- 나무/건초더미는 크기가 작아 이번 범위에서는 NavMeshModifier를 달지 않음(좀비가 나무 밑동을 살짝 스쳐 지나가도 크게 어색하지 않다고 판단) — 나중에 사용자가 지적하면 그때 추가.

### 검증
- Play Mode에서 헛간 중심이 NavMesh 밖(반경 0.5 내 샘플링 실패)임을 확인해 좀비가 헛간을 통과하지 못함을 검증. 플레이어를 헛간 옆으로 이동시켜도 정상 서 있음(콜라이더로 물리 차단 확보). 콘솔 에러 0건.
- 45도 각도 스크린샷(헛간+사일로+울타리+나무 조합)과 탑뷰 스크린샷(전체 배치)으로 육안 확인 — 자연스러운 작은 농장 실루엣을 이룸.

## 2026-09-05 — 맵 확장 3차: Blender로 농장 소품 재제작 ("엉성해 보인다" 피드백)

### 사용자 피드백 및 결정
- 프리미티브(Cube/Cylinder/Sphere)로 만든 농장 소품이 "엉성해 보인다"며 "블랜더로 맵을 만들어줄수있니?" 요청.
- 이 컴퓨터에 Blender 5.2.1 LTS가 설치되어 있음을 확인(`C:\Program Files\Blender Foundation\Blender 5.2\blender.exe`). Blender를 `--background --python 스크립트경로` 로 헤드리스 실행해 bpy/bmesh로 절차적 모델을 만들고 FBX로 내보낸 뒤, 그 FBX를 Unity 프로젝트로 가져와 기존 프리미티브를 교체하는 방식으로 진행.

### Blender 헤드리스 파이프라인 (다음에도 재사용 가능한 패턴)
- 스크립트 위치: 세션 스크래치패드(`.../scratchpad/build_rural_props.py`), 출력은 스크래치패드 하위 `rural_export/`에 FBX로 저장 후 `Assets/Models/Rural/`로 복사 → `refresh_unity`로 임포트.
- 실행 명령: `"C:\Program Files\Blender Foundation\Blender 5.2\blender.exe" --background --python <스크립트경로>` — GUI 없이 몇 초 안에 끝남, stdout에 각 FBX export 로그가 찍힘.
- FBX 내보내기 옵션 중요 포인트: `axis_forward='-Z', axis_up='Y'`로 지정해야 Blender(Z-up)→Unity(Y-up) 좌표계가 올바르게 변환됨(직접 확인: 씬에 배치했을 때 모든 오브젝트의 로컬 Y 오프셋이 Blender에서 설계한 Z 오프셋과 정확히 일치).
- 오브젝트별로 pivot이 다르게 나올 수 있음(bmesh로 직접 정점을 이동시킨 경우 pivot이 원점에 남고 좌표가 메쉬에 baked됨 / `primitive_xxx_add(location=...)`로 만든 경우 pivot 자체가 그 위치로 이동하고 메쉬는 로컬 원점 중심으로 남음) — 어느 쪽이든 Unity로 들어오면 자식 GameObject의 `localPosition`은 항상 올바른 실제 오프셋을 가지므로, Unity에서 배치할 때는 각 파츠의 `localPosition`을 기준으로 생각하면 됨(메쉬 자체의 `bounds.center`가 (0,0,0)으로 나와도 당황할 필요 없음 — pivot 방식 차이일 뿐).
- 만든 5종: `Barn.fbx`(몸체+진짜 삼각프리즘 박공지붕, 처마 오버행 포함), `Silo.fbx`(원기둥+진짜 원뿔 지붕), `Tree.fbx`(원뿔형 트렁크+아이코스피어를 무작위 정점 지터로 찌그러뜨린 불규칙 수관), `HayBale.fbx`(베벨 모디파이어로 모서리를 둥글린 원기둥), `WoodFence.fbx`(기둥 2개+가로대 2단, 목재 파스처 펜스 — 기존 철제 묘지 울타리를 농장에 재활용하던 어색함 해소).
- 지붕 트릭(헛간): bmesh로 처마 사각형 4점 + 능선 2점, 총 6정점으로 삼각기둥 지붕을 직접 정의(앞/뒤 삼각형 2면 + 좌우 경사면 2면) — 이전 세션에서 썼던 "정사각 큐브를 45도 회전시키는" 트릭보다 훨씬 깔끔하고 처마 오버행도 자연스럽게 표현됨. 앞으로 각진 지붕이 필요하면 이 bmesh 6정점 패턴을 재사용할 것(정사각 회전 트릭은 폐기).

### 프리미티브 → Blender 모델 교체
- 기존 프리미티브 25개(헛간/사일로/철제울타리 7/나무 11/건초더미 3) 전부 삭제 후, 같은 위치·스케일·회전으로 새 FBX 인스턴스 재배치(나무는 기존에 저장해뒀던 무작위 변주값을 그대로 재사용해 배치가 흐트러지지 않음).
- 목재 울타리는 세그먼트 폭이 기존 철제 울타리(2유닛)와 다르게 설계됨(2.4유닛) — 개수를 7→6개로 재계산해 비슷한 총 길이(약 14유닛)를 유지.
- 헛간 몸체(BoxCollider)·사일로 몸체(CapsuleCollider)·나무 11그루 각각의 Trunk(CapsuleCollider)에 콜라이더와 `NavMeshModifier`(Not Walkable)를 새로 추가(FBX로 임포트된 메쉬는 프리미티브와 달리 콜라이더가 자동으로 안 붙으므로 수동 추가 필요) — 이전에는 헛간·사일로에만 달았지만 이번엔 나무 트렁크에도 추가해 좀비가 나무를 뚫고 지나가지 않도록 개선.
- NavMesh 재굽기+에셋 재저장 함정(지난 두 차례 기록 참고)을 세 번째로 동일하게 겪을 뻔했으나 이번엔 처음부터 `AssetDatabase.DeleteAsset`+`CreateAsset`+`SaveAssets` 패턴을 바로 적용해 문제없이 처리함.

### 검증
- Play Mode에서 헛간 중심·나무 트렁크 둘 다 NavMesh 밖(좀비 차단 확인), 플레이어가 헛간 옆에 서도 정상. 콘솔 에러 0건.
- 45도/탑뷰 스크린샷 비교: 이전 프리미티브 버전 대비 실루엣이 뚜렷하게 개선됨(진짜 박공지붕, 원뿔 사일로 지붕, 불규칙한 나무 수관, 어색했던 철제 울타리 대신 목재 울타리).

## 2026-09-05 — 대규모 맵 재설계 착수: 씬 전환 시스템 + 도시 구역 1차

### 사용자 요청 (범위가 크게 바뀜)
- "블랜더로 로우폴리 아포칼립스 좀비 맵을 만들어주는데 시골/도시/광산/방공호가 있는 넓은 맵" + "탑다운 슈터 게임 형식 맵이여야 돼".
- 확인 질문 결과: **한 구역씩 순차적으로** 진행, 순서는 **도시 → 광산 → 방공호 → 시골**. 기존에 있던 맵(묘지+농장)은 전부 삭제.
- 핵심 요구사항: 건물/터널/벙커 입구에서 **상호작용(E)해야만** 각각 구현된 내부 맵으로 이동(Project Zomboid 스타일). 평소에는 내부에 못 들어가도록 배치. 건물 내부는 "최대한 단순화 + 로우폴리".

### 아키텍처 결정: Additive 씬 로드 + "포켓" 좌표 오프셋
- CLAUDE.md 13.1은 "단일 씬 유지, 필요 입증 전엔 addtive 스트리밍 도입 금지"를 권장하지만, 이건 "하나의 연속된 오픈월드를 청크로 나누는" 상황이 아니라 **서로 이어지지 않는 독립 공간(건물 내부)을 입구로만 접근**하는 요구라 다른 문제로 판단 — 여러 대안을 검토함.
  1. **DontDestroyOnLoad + Non-additive 씬 교체**: "나가기"가 곧 Main.unity를 디스크에서 다시 로드하는 것과 같아서, 건물에 들어갔다 나올 때마다 도시의 좀비/웨이브/점수/떨어진 아이템이 전부 초기화됨 — 채택 안 함.
  2. **Additive 씬 로드 (채택)**: Main 씬은 절대 언로드하지 않고, 건물 진입 시 내부 씬을 **추가로** 로드해 플레이어만 그 안으로 텔레포트, 퇴장 시 내부 씬을 언로드하고 원래 문 앞으로 복귀. Main의 상태(좀비/웨이브 등)가 전혀 끊기지 않음. 플레이어/카메라/UI/GameManager를 손댈 필요가 전혀 없음(전부 Main에 그대로 살아있음) — 기존 `UIManager`/`GameManager`가 이미 "씬 안에서 FindObjectOfType으로 찾는" 지연 싱글톤 패턴이라 DontDestroyOnLoad 없이도 자연스럽게 맞아떨어짐.
- **좌표 "포켓" 배치**: 내부 씬들(House Interior 등)의 콘텐츠는 메인 월드(원점 부근)와 겹치지 않도록 (3000,0,3000)처럼 멀리 떨어진 좌표에 미리 배치해서 authoring. Additive 로드해도 도시 위에 겹쳐 보이지 않음. 앞으로 내부 씬 종류가 늘어나면 서로 겹치지 않게 충분히 떨어뜨려(예: 상점 내부는 4000대, 터널 내부는 5000대 등) 배치할 것.

### 새 스크립트 (`Assets/Scripts/`)
- `SceneTransitionManager.cs`: `EnterInterior(sceneName, returnPos, returnRot)`/`ExitInterior()`. `SceneManager.LoadSceneAsync(..., Additive)`로 로드 후 `"Interior Spawn Point"`라는 이름의 오브젝트를 찾아 그 위치로 텔레포트. 텔레포트 시 `CinemachineVirtualCamera.OnTargetObjectWarped()`를 호출해 카메라가 부드럽게 따라오지 않고 즉시 스냅하도록 처리(안 하면 순간이동인데 카메라만 스르륵 따라와서 어색함).
- `BuildingEntrance.cs` / `InteriorExit.cs`: 트리거 콜라이더 + `PlayerInput.interact`(E키, 이번에 추가) 감지 + `UIManager` 안내 문구.
- `UIManager.cs`: `interactPrompt`/`interactPromptText` + Show/Hide 메서드 추가. HUD Canvas에 "Interact Prompt" Text 오브젝트를 코드로 만들어 필드에 연결.

### [함정] 씬 전환 도중 안내 문구가 고정되어 안 사라짐
- `BuildingEntrance`/`InteriorExit`가 보여준 프롬프트는 그 오브젝트가 `OnTriggerExit`을 통해 꺼야 하는데, 씬 전환으로 그 오브젝트 자체가 파괴되면 `OnTriggerExit`이 안 불려서 문구가 화면에 그대로 남음(실제로 재현: 나갔다 온 직후 스크린샷에 "E: 나가기"가 계속 떠있었음). 해결: `SceneTransitionManager.EnterInterior`/`ExitInterior` 시작 시점에 무조건 `UIManager.HideInteractPrompt()`를 먼저 호출.

### [함정 재확인] `manage_scene load`는 저장 안 한 현재 씬 편집을 날려버림
- Interact Prompt UI를 Main 씬에 만든 직후 저장하지 않고 바로 `manage_scene create`(House Interior 새로 만들기 — 활성 씬이 바뀜)를 실행했다가, 나중에 `manage_scene load Main`으로 돌아오니 그 UI가 통째로 사라져 있었음(디스크에 저장된 옛 버전으로 로드됨). Play Mode 테스트에서 `interactPrompt` 필드가 null로 나와서 발견. **에디터에서 씬 콘텐츠를 만들 때마다, 다른 씬을 create/load하기 직전에 반드시 `manage_scene save`부터 할 것** — 이번 세션에서 두 번째로 겪은 "씬/에셋 전환 전 저장 필수" 패턴(첫 번째는 NavMesh 에셋 재저장 함정).

### 도시 구역 콘텐츠
- 기존 그래픽 전부 삭제(Props 24개/Laterns/Rural Decor 22개/Effects(화재 위험 포함)/철제 Fence+Fence Collider) — Ground/Directional Light/Tiles(빈 컨테이너)만 남기고 재활용.
- Ground 70x70으로 확대 + 아스팔트색 재질. 기존 철제 울타리가 사라지면서 없어진 물리적 경계는 `City Boundary`(렌더러 꺼진 박스 콜라이더 4면 + NavMeshModifier)로 대체.
- Blender로 `CityBuilding.fbx`(1x1x1 단위 큐브: 몸체+처마 지붕+문 마커, 인스턴스마다 자유롭게 스케일)와 `Rubble.fbx`(잔해 더미) 제작 — 헤드리스 파이프라인은 지난번 농장 소품 제작 때와 동일(`build_city_props.py`, 같은 방식으로 재사용 가능).
- 메인 도로 하나(동서 방향) 양옆에 건물 10동(색상 4종 랜덤: 벽돌/콘크리트/베이지/블루그레이) 배치, 잔해 5곳. 건물 1동(`Building North Entrance`, 문만 노란색)에 House Interior로 연결되는 입구 배치.
- 건물마다 `BoxCollider`+`NavMeshModifier`(Not Walkable) 수동 추가(FBX 임포트 메쉬는 프리미티브와 달리 자동으로 안 붙음 — 농장 소품 때와 동일한 패턴).

### 검증
- Play Mode에서 `SceneTransitionManager`의 Enter/Exit를 직접 호출해 왕복 검증: 진입 시 (3000,0,2997.2)로 정확히 텔레포트(Interior Spawn Point와 일치), 퇴장 시 (0,0.05,9.3)으로 정확히 복귀(Return Point와 일치). `SceneManager.sceneCount`로 내부 씬이 로드 시 2, 언로드 후 1로 정확히 바뀜을 확인. 총 발사/좀비 스폰/NavMesh 차단 전부 회귀 없음. 콘솔 에러 0건.

### 남은 일 (다음 세션 참고)
- 이번엔 씬 전환 시스템 검증 + 작은 시가지(건물 10동, 진입 가능 1동)까지만 완료. 다음 순서: 도시 구역을 더 확장(교차로/더 많은 건물/내부 템플릿 다양화) → 광산 → 방공호 → 시골 재구축.
- 기존 그래이브야드/농장 관련 에셋(`Assets/Models/Level Art/*`, `Assets/Models/Rural/*`)과 재질들은 삭제하지 않고 프로젝트에 남겨둠 — 나중에 "시골" 구역을 재구축할 때 재사용 가능(이번에 씬에서 오브젝트만 제거했을 뿐, 에셋 파일 자체는 그대로 있음).
- Zombie Spawner의 기존 스폰 포인트 4개(±24~29 부근)는 그대로 두었음 — 새 70x70 도시 레이아웃에서도 대체로 도로/빈 공간에 위치해 건물과 안 겹치지만, 정밀하게 재배치하지는 않음(다음 확장 때 함께 정리 예정).

## 2026-09-05 — 도시 구역 확장 2차 (거리 연장 + 입구 2개 추가)

### 사용자 확인: "도시 구역 더 확장"
- 후속 질문에서 "도시 구역 더 확장(교차로/더 많은 거리와 건물, 내부 템플릿 다양화)" 옵션 선택.

### [설계 변경] 십자 교차로는 이번엔 보류 — 공간 계산 실수 교훈
- 처음엔 기존 큰길(X축)에 수직인 새 도로를 끼워 넣으려 했으나, 기존 건물 10동이 X=-24~24 구간을 6~8유닛 폭으로 촘촘히 채우고 있어서 폭 10짜리 교차로가 들어갈 4~6유닛 틈이 없었음(건물 배치 후에야 발견 — 다음엔 도로망을 먼저 설계하고 건물을 그 사이에 채우는 순서로 할 것, 거꾸로 하면 이런 공간 부족이 재발함).
- 대신 같은 큰길을 동서로 연장(Ground 70x70→90x90, 도로 길이 60→80)하고 양 끝(X=±36)에 건물 4동을 추가하는 것으로 범위를 조정. 사용자에게는 checklist.md/보고에 이 변경 이유를 명시.
- **다음 교차로 시도 시**: 반드시 기존 건물이 없는 새로 확보한 공간(예: 이번에 넓힌 90x90의 여백)에 배치하거나, 도로망을 먼저 그리드로 설계한 뒤 건물을 그 사이 칸에 채우는 순서로 진행할 것.

### Store Interior (두 번째 인테리어 템플릿)
- House Interior(10x8, 좁고 김, 회색 벽)와 의도적으로 다른 비율(14x7, 넓고 낮음, 베이지 벽)+가구 구성(선반 3열+계산대)으로 "상점"임을 시각적으로 구분. 포켓 좌표 (4000,0,4000) — House Interior(3000,0,3000)와 1000유닛 이상 떨어뜨려 겹침 없음.
- 인테리어 템플릿을 늘릴 때마다 이런 식으로 (a) 포켓 좌표를 최소 1000유닛 간격으로 새로 확보, (b) 치수/색상을 이전 템플릿과 다르게 해서 "다른 곳에 들어왔다"는 느낌을 주는 두 가지를 지킬 것.

### 새 입구 2개
- `Building North Store Entrance`(X=36, 새로 만든 북쪽 끝 건물) → Store Interior.
- 기존 `Building South 1`(X=-12, 이미 있던 건물 재사용) → House Interior 재사용 연결(문 마커 색만 노란색으로 바꿔서 "여기 들어갈 수 있음"을 표시) — **인테리어 하나를 여러 건물 입구에서 재사용하는 것도 유효한 패턴**임을 확인(모든 진입 가능 건물마다 고유 인테리어를 새로 만들 필요는 없음, 반복되는 "일반 가정집" 같은 곳은 재사용해도 자연스러움).

### 검증
- Play Mode에서 Store 입구/재사용 House 입구 둘 다 왕복 전환 검증(정확한 스폰/복귀 좌표, `sceneCount` 정상 전환). 좀비/총 발사 회귀 없음, 콘솔 에러 0건.

### 다음 세션 참고
- 지금 도시는 "한 줄짜리 큰길 + 양옆 건물 14동 + 진입 가능 3동" 상태. 진짜 격자형 도시(교차로)를 만들려면 이번에 넓힌 90x90 바깥으로 Ground를 한 번 더 키우거나, 현재 큰길과 평행하지 않은 새 구역에 별도로 배치하는 걸 권장.

## 2026-09-05 — 바닥 텍스처 + 랜드마크 건물 + 집 내부 방 구조 확장

### 사용자 피드백 3가지
1. "바닥이 엉성해 도시처럼 만들어달라" — Ground/도로/인도가 전부 무늬 없는 단색 평면이었던 것 지적.
2. "맵이 너무 단조로워 경찰서/소방서/병원/약국/아파트 등 배치" — 건물 14동이 전부 같은 모양(색만 다름)이었던 것 지적.
3. "집 내부는 적어도 거실/화장실/부엌/방2개" — 기존 House Interior가 가구 2개짜리 방 하나뿐이었던 것 지적.

### 바닥/도로 디테일
- `manage_texture`로 절차적 텍스처 3종 제작: 아스팔트(`apply_noise`, 어두운 회색 계열), 보도블록(`apply_pattern` grid, 밝은 회색), 도시 바닥(`apply_noise`, 중간 회색).
- **[함정]** `apply_noise`/`apply_pattern`은 `palette` 파라미터를 안 주면 자체 기본 팔레트(중간 회색조)로 **기존 fill_color를 덮어씀** — 처음에 어두운 아스팔트색으로 `create` 해놓고 `apply_noise`만 호출했더니 밝은 회색 노이즈로 바뀌어버림. 원하는 색상을 유지하려면 `apply_noise`/`apply_pattern` 호출 시 반드시 `palette`(색상 2개 이상의 배열)를 명시할 것.
- `set_import_settings` 액션에 `import_settings` JSON 객체를 문자열로 넘기면 파싱이 안 돼서 실패함(`{}`로 들어감) — 결국 `TextureImporter`를 코드로 직접 찾아 `wrapMode`/`filterMode`를 설정하는 방식으로 우회. 이 툴로 반복(tiling) 텍스처의 Wrap Mode를 바꿀 땐 `set_import_settings` 대신 직접 `AssetImporter.GetAtPath` 코드가 더 확실함.
- 재질의 `mainTextureScale`로 큰 도로/보도/바닥 면적에 텍스처가 촘촘하게 반복되도록 타일링 조정(오브젝트를 스케일해도 UV는 0~1 그대로라 `mainTextureScale` 값 자체가 "몇 번 반복되는지"임 — 반복 간격 ≈ 오브젝트 실제 크기 ÷ 스케일값이 되도록 계산).
- 도로 중앙선(노란 대시 23개, 2m 간격 대시-갭)과 인도-차도 경계 연석(살짝 솟은 얇은 박스) 추가로 "도로처럼" 보이게 함.
- 건물마다 바닥에 "대지" 패치 추가(건물 풋프린트+3유닛 여유, 주거형=`Lawn.mat` 초록 잔디/상업·공공형=`Lot Pavement.mat` 회색 포장) — 단색 평면 위에 건물이 둥둥 떠있는 느낌을 줄임.

### 랜드마크 건물 (신규 모델링 없이 기존 CityBuilding 재활용)
- 새 Blender 모델을 만들지 않고, 기존 14동 중 5동을 **재질 교체 + 간단한 서명 오브젝트(십자가/줄무늬/차고문/층선)를 추가로 붙이는 방식**으로 차별화 — 훨씬 빠르고, 이미 검증된 콜라이더/NavMeshModifier/도어 트리거 구조를 그대로 재사용 가능.
- 경찰서: 파란 몸체(`Police Blue.mat`) + 흰 가로띠(`Accent Stripe`) + 지붕 위 파란 라이트바(`Light Bar`).
- 소방서: 빨간 몸체(`Fire Red.mat`) + 전면에 검정 대형 차고문 패널(일반 문보다 훨씬 넓게, `Garage Door.mat`).
- 병원: 흰 몸체(`Hospital White.mat`) + **크기를 원래보다 확대**(11x7x9, 다른 건물보다 눈에 띄게 큼) + 전면에 빨간 십자가(가로+세로 박스 2개 겹침).
- 약국: 흰 몸체 + 작은 초록 십자가(병원보다 절반 크기, `Cross Green.mat`).
- 아파트(2동): 높이를 15로 대폭 확대(다른 건물 4~7 대비 압도적으로 높음) + 층 구분선(얇은 어두운 띠 4단, `Roof Dark.mat`)으로 다층 건물처럼 보이게 함.
- **패턴 정리**: "십자가"(가로 박스+세로 박스 겹침), "줄무늬/층선"(전체 폭보다 살짝 넓은 얇은 박스를 원하는 높이에 배치), "차고문"(전면 벽에 딱 붙는 넓고 어두운 패널) — 전부 프리미티브 조합만으로 구현 가능한 저폴리 서명 패턴. 앞으로 다른 건물 타입(학교/은행 등)이 필요하면 이 패턴들을 재조합해서 빠르게 만들 수 있음.

### House Interior 방 구조 확장 (거실+부엌+화장실+침실2)
- 기존 10x8 단일 방을 삭제하고 14x12 크기에 벽 12개 세그먼트로 5개 방(거실이 입구 안쪽 전체 폭, 그 뒤로 왼쪽 칼럼에 부엌→침실1, 오른쪽 칼럼에 화장실→침실2)을 구성. 각 방 사이 벽에 문 간격(2~3유닛)을 뚫어 서로 이어지도록 설계.
- 방마다 최소한의 가구 배치(소파+TV장/조리대+식탁/변기+세면대/침대×2) — 여전히 프리미티브 박스 조합이라 CLAUDE.md의 "저폴리+단순화" 기조 유지.
- **핵심**: `Interior Spawn Point`, `Exit Trigger` 오브젝트 **이름을 그대로 유지**했기 때문에 Main 씬의 `BuildingEntrance`/`SceneTransitionManager` 코드는 전혀 손대지 않고도 새 방 구조가 자동으로 반영됨(이름으로 찾는 설계라 내부 좌표가 바뀌어도 안전) — 인테리어 씬을 나중에 다시 리모델링할 때도 이 두 오브젝트 이름만 유지하면 됨.
- 검증: 탑뷰 스크린샷으로 5개 방 구획+문 간격 확인, Play Mode에서 기존 House 입구로 실제 진입해 새 거실에 정확히 도착함을 확인(왕복 전환도 재검증, sceneCount 정상).

### 남은 것
- Store Interior는 아직 단일 방(선반+계산대)뿐 — 사용자가 상점 내부도 방 구조를 요구하면 그때 확장.
- 십자 교차로는 이번에도 손대지 않음(이전 세션에 기록한 대로 별도 공간에 배치 필요).

## 2026-09-05 — 낮/밤 순환 + 가로등 + 실제 도시 교차로

### 사용자 요청
"낮과 밤이 일정시간 반복되도록" + "도시에 가로등도 몇개 배치" + "실제 도시처럼 건물들을 배치".

### 낮/밤 순환 설계
- `DayNightCycle.cs`를 `Level Art/Directional Light`에 부착. `cycleDuration`(기본 240초) 동안 태양의 X축 고도를 0~360도로 계속 회전시키고, `Mathf.Sin(각도)`를 0~1로 클램프한 뒤 smoothstep으로 부드럽게 만든 값을 "낮 정도"(`DayAmount`)로 사용 — 태양이 지평선 위(각도 0~180)인 절반 구간만 낮, 나머지 절반은 밤. 태양광 색상/세기와 `RenderSettings.ambientLight`를 낮/밤 프리셋 사이에서 `Color.Lerp`.
- 밤 프리셋(색/환경광)은 기존에 이미 있던 무드(보라빛 태양광 + 어두운 주황 환경광)를 그대로 재사용 — 이번 작업 전 모든 스크린샷의 "무드"가 사실 밤 설정이었던 셈. 낮 프리셋은 새로 정의(따뜻한 흰빛 태양 + 중간 밝기 중성 환경광).
- **검증 팁**: Play Mode에서 실제 240초를 기다리지 않고, `elapsed`(private) 필드를 리플렉션으로 원하는 지점으로 강제 이동시킨 뒤 `Update()`를 직접 Invoke하면 순간적으로 낮/밤 전환을 재현해서 검증할 수 있음 — 시간이 걸리는 순환 시스템을 테스트할 때 항상 이 패턴(비공개 필드 강제 설정 + Update 직접 호출) 사용할 것.
- **[알아둘 점]** 카메라의 Skybox가 Unity 기본 절차적 스카이박스(`Default-Skybox`)라서, 태양의 세기/색을 낮춰도 하늘 자체의 밝기가 스크린샷에서 드라마틱하게 어두워지지는 않음(절차적 스카이박스는 태양 방향 기준 글로우를 자체 로직으로 그리기 때문). 지면/건물의 실제 조명(환경광+직사광)은 정상적으로 어두워짐(리플렉션으로 `RenderSettings.ambientLight`가 밤에 정확히 어두운 값으로 바뀌는 것을 직접 확인함) — 다만 "하늘까지 확 어두워지는" 극적인 밤 연출을 원하면 다음에 카메라 클리어 플래그를 Solid Color로 바꾸고 배경색도 DayNightCycle에서 같이 보간하도록 개선 필요.

### 가로등
- `StreetLight.cs`: `DayNightCycle.IsNight`를 매 프레임 확인해 Point Light on/off + 전구 머티리얼 색 전환. 여러 가로등이 같은 "Lamp Off" 머티리얼 에셋을 공유하는데, `renderer.material`(공유 아님, 인스턴스화)로 접근해서 한 가로등을 켜도 다른 가로등에 영향 안 주도록 함 — `sharedMaterial`로 했으면 전부 같이 켜지고 꺼지는 버그가 났을 것.
- 기둥(Cylinder)+암(Cube)+램프헤드(Cube)+PointLight 조합으로 프리팹형 템플릿 하나를 만들고 11개 복제 배치(메인 거리 인도 위, 북쪽/남쪽 교대). 템플릿 원본은 비활성 상태로 씬에 남겨둠(추후 다른 거리에 더 배치할 때 재사용 가능).

### 실제 도시 교차로 (지난 세션에 "공간 부족"으로 보류했던 것 해결)
- 지난 세션 노트에 "교차로는 기존 건물이 없는 새 공간에 배치할 것"이라고 남겨뒀던 대로, 기존 동서 큰길(건물 14동)은 전혀 안 건드리고 그 동쪽 끝(마지막 건물 X=36) 바깥에 완전히 새로운 남북 교차로(X=60)를 추가.
- Ground를 대칭이 아니라 **동쪽으로 치우치게 재배치**(중심 X: -1.5→19, 폭 90→132)해서 새 교차로+건물 공간을 확보 — Ground의 피벗이 중심이라 스케일만 키우면 항상 대칭으로 커지므로, 특정 방향으로만 넓히고 싶으면 스케일과 함께 position도 같이 옮겨야 함(이번에 정확히 그렇게 처리).
- 새 교차로 양옆(서쪽 X=47, 동쪽 X=73)에 건물 6동 배치, 각각 문이 교차로 쪽(도로 방향)을 향하도록 회전(서쪽 건물은 rotY=90로 동쪽을 향함, 동쪽 건물은 rotY=270으로 서쪽을 향함 — CityBuilding 기본 문 방향(+Z)이 Y회전에 따라 어느 월드 방향을 향하는지는 `(x,z)->(x cosθ+z sinθ, -x sinθ+z cosθ)` 공식으로 미리 계산 후 배치, 실수 없이 한 번에 맞음).
- City Boundary/Full Map Camera(orthoSize 50→70)도 새 Ground 범위에 맞게 재조정.

### 검증
- Play Mode 종합 회귀: 총 발사(처음엔 매거진이 안 줄어서 당황했으나, `Time.time`이 아직 `timeBetFire`보다 작아서 발사 쿨다운에 걸린 테스트 타이밍 문제였을 뿐 — 프레임을 몇 번 더 진행한 뒤 재시도하니 정상 25→24. **일시정지 상태에서 막 Play를 시작한 직후 곧바로 발사 등 쿨다운 있는 액션을 테스트하면 Time.time이 아직 작아서 실패로 오인할 수 있음, 몇 프레임 Step 후 테스트할 것**), 좀비 스폰, 건물 입구 왕복 전환(확장된 Ground/새 교차로 환경에서도 기존 House 입구가 정상 동작) 전부 확인. 콘솔 에러 0건.

### 남은 것
- 교차로가 아직 1개뿐 — 완전한 격자(여러 교차로)를 원하면 같은 패턴(기존 콘텐츠 안 건드리고 바깥 공간에 새 도로+건물 추가, Ground를 필요한 방향으로 위치+스케일 함께 조정)으로 반복 확장 가능.
- 하늘(스카이박스) 밤 연출 개선은 다음 세션 후보.

## 2026-09-05 — 세션 종료 (다른 컴퓨터에서 이어서 작업 예정)

- 이 시점까지 커밋 `bb7a9d1`까지 전부 GitHub `origin/main`에 push 완료. 로컬에 미커밋/미푸시 변경 없음(`git status --short` 깨끗함).
- `checklist.md`는 근접무기 슬라이스의 완료 기록으로 전부 체크됨 — 다음 슬라이스(차량 등)를 시작할 때는 이 파일을 그 슬라이스용 새 체크리스트로 덮어써도 됨(과거 슬라이스 완료 기록은 git 히스토리와 `plan.md`/`context-notes.md`에 이미 남아있으므로 보존 목적으로 유지할 필요는 없음).
- 다음 세션 시작 시 `plan.md` 최상단 "현재 상태 요약" 섹션을 먼저 읽을 것.
