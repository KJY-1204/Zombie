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

## Slice 4 완료 (핵심 상태 = 체력만, Hunger/Stamina 등은 요청 없어 추가 안 함)
