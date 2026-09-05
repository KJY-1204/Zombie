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
