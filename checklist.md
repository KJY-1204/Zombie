# Checklist — Slice 1: 원형 미니맵 (좀비 미표시)

## CP1. 레이어 준비
- [x] `Enemy` 레이어 추가 (슬롯 10).
- [x] `MinimapPlayer` 레이어 추가 (슬롯 11).
- [x] `Zombie.prefab`의 비주얼(루트, `Zombie_Cylinder` SkinnedMeshRenderer, `BloodSprayEffect`/`BloodGlobs` 파티클)을 `Enemy`로 변경.
      Verify: 컴파일 에러 0건. Play Mode에서 스폰된 좀비 콜라이더에 점블랭크 레이캐스트 → `layer=10`, `IDamageable` 정상 인식 확인(총기 명중 회귀 없음).

## CP2. 미니맵 카메라
- [x] `Assets/Textures/MinimapRenderTexture.renderTexture` 생성 (512x512).
- [x] 씬에 `Minimap Camera` GameObject 추가: Orthographic, Target Texture 연결, 컬링 마스크 = Default + MinimapPlayer(2049), X축 -90도 고정 회전(정북 고정), Depth -1.
- [x] `Assets/Scripts/MinimapFollow.cs` 신규 생성 및 부착 (플레이어 태그로 타겟 1회 캐싱, XZ 위치만 추적, 회전 고정).
      Verify: `manage_camera screenshot camera="Minimap Camera"`로 직접 캡처 → 지형(Level Art)은 보이고 좀비는 전혀 보이지 않음 확인. **주의**: 헤드리스 자동화 세션에서는 Unity Editor가 OS 포커스를 못 받아 Time.frameCount가 진행되지 않아(EditorApplication.QueuePlayerLoopUpdate 시도해도 동일) 실시간 추적 동작을 프레임 단위로 직접 관찰하지 못함. 스크립트 로직 자체는 단순하고 검증됨(LateUpdate에서 위치만 갱신) — 실제 에디터에서 포커스를 두고 플레이할 때 정상 동작 예상. 사용자가 직접 에디터에서 플레이하며 최종 확인 권장.

## CP3. 원형 미니맵 UI
- [x] `HUD Canvas.prefab`에 `Minimap` 계층 추가 (Mask + RawImage), 우측 상단 앵커(160x160).
- [x] ~~`Health Circle` 스프라이트~~ → **변경**: `Health Circle.png`을 픽셀 알파 검사한 결과 링(도넛) 모양으로 확인되어(중심부 alpha=0) 마스크로 부적합. 대신 `Assets/Sprites/Minimap Mask Circle.png`(채워진 원, 코드로 생성)를 새로 만들어 사용.
      Verify: Play Mode 게임 화면 우측 상단에 원형 미니맵이 정상 렌더링됨(스크린샷 확인).

## CP4. 플레이어 마커
- [x] `Player Character.prefab`에 `Minimap Marker` 자식 추가 (Quad, MeshCollider 제거, layer MinimapPlayer, 노란색 URP Unlit 머티리얼 `MinimapPlayerMarker.mat`, scale 0.9).
- [x] 메인 카메라 컬링 마스크에서 `MinimapPlayer` 레이어 제외 (cullingMask: -1 → -2049).
      Verify: `Camera.Render()` 강제 호출 + `ReadPixels`로 마커 위치 픽셀을 직접 샘플링해 노란색이 실제로 렌더링됨을 픽셀 단위로 확인(스크린샷 육안 확인의 한계를 보완). 마커를 강제로 비활성화해도 메인 게임 화면에 변화 없음 확인(메인 카메라엔 애초에 안 그려짐).
      **[수정된 버그]** 최초 구현 시 회전을 `(-90,0,0)`으로 넣어 Quad 뒷면이 미니맵 카메라를 향해 실제로는 전혀 렌더링되지 않고 있었음(회색 기본 머티리얼이라 눈치채지 못함). 노란색으로 바꾼 뒤 픽셀 검사로 발견, 올바른 회전 `(90,0,0)`으로 수정 완료 — 자세한 진단 과정은 context-notes.md 참조.

## CP6. 방위 라벨 (사용자 추가 요청)
- [x] `HUD Canvas.prefab`의 `Minimap` 아래 `Label North/South/East/West` 4개 추가.
- [x] 원형 마스크 바깥의 `Minimap`(마스크 아님) 오브젝트에 자식으로 추가해 원형 클리핑에 잘리지 않게 함, 각각 상/하/좌/우 앵커에 고정(정북 고정 미니맵이라 방위가 회전하지 않음).
- [x] **가독성 개선(사용자 요청)**: fontSize 12→22로 확대, 폰트를 `Kenney Future Narrow`(장식체, 작은 크기에서 흐릿함)에서 `Assets/TextMesh Pro/Fonts/LiberationSans.ttf`(레거시 `UI.Text`로 사용, 이미 프로젝트에 포함되어 있던 에셋이라 신규 임포트 불필요)로 교체해 또렷하게 표시. N만 빨간색(`RGBA(1, 0.15, 0.15, 1)`)으로 강조, 나머지는 흰색 유지.
      Verify: 스크린샷 크롭 확대(3배)로 N(빨강, 위)/S/E/W(흰색) 전부 또렷하게 읽힘 확인.

## CP5. 통합 검증
- [x] Unity 컴파일 에러 0건.
- [x] 좀비 스폰 후에도 미니맵에 좀비가 전혀 안 보임 (레이어 격리로 보장).
- [x] 총기 발사가 좀비(Enemy 레이어)에게 정상 명중 (레이캐스트 회귀 테스트 통과).
- [ ] 실제 에디터에서 사람이 직접 플레이하며 "미니맵이 이동을 따라오는지" 최종 확인 — 헤드리스 세션 한계로 자동 검증 못함(위 CP2 비고 참조).

## 알려진 이슈 (범위 밖, 손대지 않음)
- 게임 화면에서 플레이어 발밑에 분홍색 링 이펙트가 보임 — Minimap Marker를 비활성화해도 동일하게 보여 **기존부터 있던 요소**로 확인됨(이번 작업과 무관). 원인 파악은 별도 작업으로 분리 필요.
