# Checklist — Slice 4: 플레이어 상태 창 (Project Zomboid 스타일)

## 배경 (사용자 확인)
- Phase A 인스펙션 중 발견: 체력 UI는 이미 존재했음(플레이어 발밑의 방사형 링, `Health Circle` 스프라이트를 Radial360 Fill로 사용). Slice 1 때 "미니맵과 무관한 기존 장식"으로 오인해 넘겼던 분홍 링이 사실 이것이었음.
- 사용자 요청: 기존 발밑 링을 제거하고, Project Zomboid처럼 `I` 키를 누르면 뜨는 상태 창으로 교체.

## CP1. 기존 발밑 체력 링 제거
- [x] `PlayerHealth.cs`에서 `healthSlider` 필드와 모든 사용처(OnEnable/RestoreHealth-오버라이드 삭제/OnDamage/Die) 제거. 체력 표시 책임을 완전히 UI 쪽(StatusWindow)으로 이관.
- [x] `Player Character.prefab`에서 월드스페이스 `Canvas`(`Health Slider` 포함) 자식 오브젝트 삭제.
      Verify: 컴파일 에러 0건, 스크린샷에서 발밑 링이 더 이상 안 보임 확인.

## CP2. 상태 창 UI + 토글 입력
- [x] `PlayerInput.cs`에 `toggleStatus`(`KeyCode.I`, `GetKeyDown`) 추가, 게임오버 시 false로 리셋.
- [x] `UIManager.cs`에 `statusWindow`(GameObject)/`statusHealthText`(Text) 필드 + `Awake()`(플레이어 PlayerInput/LivingEntity 캐싱, 창 기본 비활성화) + `Update()`(토글 입력 시 SetActive 반전, 열려있는 동안만 체력 텍스트 갱신) 추가.
- [x] `HUD Canvas.prefab`에 `Status Window` 패널(반투명 검은 배경, 제목 "상태", 체력 텍스트, "[I] 닫기" 안내) 추가, 기본 비활성.
      Verify: 리플렉션으로 `toggleStatus`를 두 번 시뮬레이션해 창이 열림→닫힘 정상 토글 확인. 체력을 100→65로 깎은 뒤 열었을 때 텍스트가 "체력  65 / 100"으로 정확히 갱신됨을 코드로 직접 확인(스크린샷은 헤드리스 환경의 캔버스 지연/글리치로 신뢰 불가 — 컴포넌트 값 직접 조회로 대체 검증).

## CP3. 회귀 확인
- [x] Unity 컴파일 에러 0건.
- [x] `healthSlider` 참조가 코드 전체에 남아있지 않음(grep 확인).
- [x] 플레이 중 콘솔 에러 없음(테스트 중 실제 사망 이벤트 발생했으나 예외 없이 정상 처리됨 — GameManager 게임오버 흐름 회귀 없음).

## CP4. PZ 스타일 시각적 참고 반영 (사용자가 예시 이미지 제공)
- 사용자가 Project Zomboid의 실제 상태창 스크린샷 2장을 공유. 부위별 부상/붕대/체온/운동 탭 등 깊은 시뮬레이션 포함 — 확인 결과 **시각적 스타일만 참고**하기로 확정(부위별 부상 시스템 등은 만들지 않음).
- [x] `Assets/Sprites/Status Body Silhouette.png` 생성 (원+사각형 조합으로 그린 단순 사람 실루엣, 120x224, 코드로 절차적 생성).
- [x] 상태 창 레이아웃을 실루엣(좌측) + 세로 체력바(우측, Image Filled/Vertical) + 체력바 하단 아이콘(기존 인벤토리 체력 아이콘 재사용) + 상단 퍼센트 텍스트로 재구성. 패널 크기 340x340으로 확대.
- [x] `UIManager.cs`에 `statusHealthBarFill`(Image) 필드 추가, `Update()`에서 `fillAmount = health/startingHealth`로 갱신(텍스트도 "체력 70%" 형식으로 변경).
      Verify: 코드로 데미지 30 적용 후 상태창 열어 `healthText="체력 70%"`, `fillAmount=0.7` 정확히 일치 확인. `RectTransform.GetWorldCorners()`로 패널이 화면(1515x862) 안쪽 중앙(553~960, 227~634)에 정확히 위치함을 좌표로 확인(스크린샷이 반복적으로 안 보였는데, 좌표 직접 검증으로 레이아웃 자체는 문제없음을 확정 — 헤드리스 캡처 렌더링 지연 문제로 결론).

## CP5. 사용자 재검수 피드백 3건
- [x] **[버그] 체력바 아이콘이 바 중심이 아니라 오른쪽 아래에 치우쳐 있었음**: `Health Bar Icon`의 `anchoredPosition.x`가 `Health Bar Background`의 실제 중심 x좌표와 다른 값으로 잘못 계산돼 있었음. 두 오브젝트의 anchor/pivot 기준으로 중심 x를 다시 계산해 정확히 일치시킴. `GetWorldCorners()`로 두 중심 좌표가 완전히 동일함(diff=0)을 확인.
- [x] **체력바/실루엣 색상**: 체력 비율에 따라 `healthyColor`(밝은 회백색) ↔ `criticalColor`(빨강) 사이를 `Color.Lerp`로 보간해 체력바 채움과 실루엣 양쪽에 동일하게 적용. 100%=원래색, 20%=붉게 물듦을 코드로 확인.
      Verify: 안전지대로 플레이어를 옮겨 좀비 공격 없이 깨끗한 상태에서 체력 100%→20% 변화 시 텍스트/fillAmount/두 이미지 색상이 전부 기대값과 정확히 일치함을 확인.

## Slice 4 완료 (핵심 상태 = 체력만, Hunger/Stamina 등은 요청 없어 추가 안 함)

# Checklist — 추가 기능: M키 전체 지도 창

## CP1. 전체 지도 카메라
- [x] `Assets/Textures/FullMapRenderTexture.renderTexture` 생성 (1024x1024).
- [x] `Full Map Camera` 씬 오브젝트 생성: Orthographic size=16(레벨 전체 바운드 27x27을 여유있게 포함), 레벨 중심(-1.5, 15, 2)에 고정(플레이어 추적 안 함 — 전체 지도는 월드 고정), 컬링 마스크는 미니맵과 동일(Default+MinimapPlayer, 레이더 활성 시 MinimapZombie 추가), 기본 `enabled=false`(창 닫혀있을 때 렌더링 비용 없음).
- [x] `MinimapRadarController.cs`에 `fullMapCamera` 필드 추가, `ShowZombies()`/`HideZombies()`가 미니맵 카메라와 전체 지도 카메라 양쪽의 컬링 마스크를 함께 토글하도록 수정.
      Verify: 레이더 비활성 시 컬링 마스크=2049(좀비 제외), `ActivateRadar()` 호출 후 6145(좀비 포함)로 두 카메라 모두 동일하게 바뀜을 확인. 카메라 직접 캡처로 레벨 전체가 여백과 함께 프레임 안에 들어옴을 확인, 레이더 활성화 후 빨간 좀비 마커 2개가 실제로 나타남을 스크린샷으로 확인.

## CP2. 지도 창 UI + 토글 입력
- [x] `PlayerInput.cs`에 `toggleMap`(`KeyCode.M`) 추가, 게임오버 시 리셋.
- [x] `UIManager.cs`에 `mapWindow`/`mapCamera` 필드 + `Update()`에서 M 입력 시 `mapWindow.SetActive` 반전과 `mapCamera.enabled`를 함께 토글하는 로직 추가.
- [x] `HUD Canvas.prefab`에 `Map Window` 패널(제목 "지도", 정사각형 RawImage로 전체 지도 표시, 미니맵과 동일한 N/S/E/W 라벨, "[M] 닫기" 안내) 추가, 기본 비활성.
      Verify: 리플렉션으로 토글 2회(열림→닫힘) 시 `mapWindow.activeSelf`와 `mapCamera.enabled`가 함께 정확히 반전됨을 확인.

## [함정] 씬 오브젝트를 프리팹 에셋 필드에 대입하면 저장 시 null이 됨
- `HUD Canvas.prefab`을 프리팹 스테이지에서 편집하며 `uiManager.mapCamera = GameObject.Find("Full Map Camera")...`처럼 **씬에만 존재하는 오브젝트**를 프리팹 에셋의 필드에 대입하면, 저장 시 조용히 null로 초기화됨(프리팹은 여러 씬에서 재사용 가능해야 하므로 특정 씬 오브젝트를 직접 참조할 수 없음 — Unity의 정상 동작).
- **해결**: 프리팹 스테이지가 아니라 **씬에 배치된 인스턴스**에 직접 대입해야 하며, 대입 후 `EditorUtility.SetDirty` + 씬 저장만으로는 부족할 때가 있었음 — `SerializedObject`/`SerializedProperty.objectReferenceValue`로 명시적으로 설정하고 `ApplyModifiedProperties()`를 호출해야 프리팹 인스턴스 오버라이드로 확실히 기록됨.
- **교훈**: 다음에 프리팹 안의 스크립트가 "특정 씬에만 있는 오브젝트"(예: 이번처럼 미니맵/지도 전용 카메라)를 참조해야 한다면, 반드시 프리팹 스테이지가 아니라 씬의 인스턴스에서, 가급적 `SerializedObject` API로 연결할 것.
