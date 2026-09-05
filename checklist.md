# Checklist — 대규모 맵 재설계 1차: 씬 전환 시스템 + 도시 구역 (시골/농장 콘텐츠는 삭제됨)

## 사용자 요청 요약
- 기존 맵(묘지+농장) 전부 삭제, 도시 → 광산 → 방공호 → 시골 순서로 재구축.
- 건물/터널/벙커 입구에서 상호작용(E)하면 각각 구현된 내부 맵으로 이동(Project Zomboid 스타일). 내부는 평소엔 못 들어가고 입구에서만 진입 가능.
- 건물 내부는 "최대한 단순화 + 로우폴리".
- 진행 방식은 사용자가 "한 구역씩 순차적으로"를 선택 → 이번 체크포인트는 **도시 구역 1차 슬라이스**(씬 전환 시스템 검증 + 작은 시가지).

## CP13. 씬 전환 아키텍처 결정
- [x] 옵션 검토: (a) DontDestroyOnLoad로 플레이어/UI 유지 + Non-additive 씬 교체, (b) Additive 씬 로드. **(b) 채택** — Main 씬을 절대 언로드하지 않아 도시의 좀비/웨이브/점수 상태가 건물 출입으로 리셋되지 않음. 내부 씬은 메인 월드와 멀리 떨어진 좌표(3000,0,3000 등 "포켓" 영역)에 배치해 좌표 겹침 없이 addtive 로드/언로드.
- [x] `PlayerInput.cs`: `interact`(E키) 입력 추가.
- [x] `SceneTransitionManager.cs` 신규: `EnterInterior(sceneName, returnPos, returnRot)` / `ExitInterior()` — 코루틴으로 씬 additive 로드/언로드, 텔레포트, 시네머신 `OnTargetObjectWarped`로 카메라 즉시 스냅(글라이드 없음).
- [x] `BuildingEntrance.cs` / `InteriorExit.cs` 신규: 트리거 범위 안에서 상호작용 시 각각 진입/퇴장 호출, `UIManager`에 안내 문구 표시.
- [x] `UIManager.cs`: `interactPrompt`/`interactPromptText` 필드 + `ShowInteractPrompt`/`HideInteractPrompt` 메서드 추가.
      Verify: Play Mode에서 `SceneTransitionManager.EnterInterior`/`ExitInterior`를 직접 호출해 라운드트립 검증 — 진입 시 플레이어가 정확히 내부 씬의 Interior Spawn Point로 순간이동, 퇴장 시 정확히 원래 문 앞 Return Point로 복귀. `SceneManager.sceneCount`로 내부 씬이 로드/언로드 시점에 정확히 추가/제거됨을 확인.
- [x] **[함정]** 씬 전환(로드/언로드) 도중 안내 문구를 보여준 트리거 오브젝트가 파괴되면 `OnTriggerExit`이 호출되지 않아 문구가 화면에 그대로 남음 → `EnterInterior`/`ExitInterior` 시작 시점에 `UIManager.HideInteractPrompt()`를 명시적으로 호출하도록 수정.
- [x] **[함정 재확인]** `manage_scene load`로 다른 씬을 불러오면 저장 안 된 현재 씬의 편집 내용이 사라짐 — Interact Prompt UI를 만들고 바로 저장하지 않은 채 House Interior 씬을 만들고 Main으로 돌아왔더니 그 UI가 통째로 사라졌던 적 있음. **씬을 전환하기 전에는 반드시 `manage_scene save`부터 할 것.**

## CP14. House Interior 씬 (건물 내부 템플릿 1종)
- [x] `Assets/Scenes/House Interior.unity` 신규 — 좌표 (3000,0,3000) 부근에 단순한 방 하나(바닥+벽 4면(문 간격 포함)+가구 2개+조명), Interior Spawn Point, Exit Trigger(`InteriorExit`) 배치.
- [x] Build Settings에 `Main.unity`, `House Interior.unity` 등록.
      Verify: 45도 스크린샷으로 방 형태 확인(문 간격 뚫린 벽, 낮은 폴리곤 가구) — 요청한 "최대한 단순화" 기준 충족.

## CP15. 도시 구역 1차 슬라이스 (Blender 저폴리 건물)
- [x] 기존 그래픽 콘텐츠 삭제: `Level Art`의 `Props`(24개 묘지 소품)/`Laterns`/`Rural Decor`(농장 22개)/`Effects`(화재 위험 효과 포함)/`Fence`(철제 묘지 울타리)/`Fence Collider` 전부 제거.
- [x] `Ground`를 70x70으로 재조정, `City Ground.mat`(아스팔트 회색)로 재질 교체.
- [x] Blender 헤드리스로 `CityBuilding.fbx`(몸체+지붕 처마+문 마커, 1x1x1 단위 큐브라 인스턴스별 스케일로 자유 변형) + `Rubble.fbx`(잔해 더미) 제작 후 Unity로 임포트.
- [x] 메인 도로(아스팔트) + 양옆 인도 배치, 도로 양쪽에 저폴리 건물 10동(색상 4종 랜덤 배치) 배치, 잔해 더미 5곳 배치.
- [x] 건물마다 `BoxCollider`(물리 차단) + `NavMeshModifier`(Not Walkable) 추가.
- [x] 도시 외곽에 보이지 않는 경계 콜라이더 4면(`City Boundary`) 추가(기존 철제 울타리 삭제로 없어진 물리적 경계 대체) — 렌더러 비활성화, NavMeshModifier로 좀비도 차단.
- [x] `Full Map Camera.orthographicSize` 30→40(새 70x70 도시 전체가 잘리지 않도록).
- [x] 건물 10동 중 도로 근처 1동(`Building North Entrance`, 문 색상만 노란색으로 구분)에 `Door Trigger`+`BuildingEntrance`(→House Interior) 배치.
      Verify: 45도/탑뷰 스크린샷으로 시가지 실루엣(플랫 지붕 저폴리 건물, 도로/인도, 색상 구분되는 진입 가능 문) 확인.

## CP16. NavMesh 재굽기 + 회귀 확인
- [x] `NavMeshSurface.BuildNavMesh()` + 에셋 재저장(기존 함정 재발 방지 위해 처음부터 `AssetDatabase.DeleteAsset`+`CreateAsset`+`SaveAssets` 패턴 적용).
- [x] Play Mode 종합 검증: 건물 입구 트리거 진입→`House Interior`로 순간이동→퇴장→정확한 문 앞 위치로 복귀(왕복 전부 확인), 총 발사 회귀 없음(magAmmo 25→24), 좀비 2마리 정상 스폰+NavMesh 위치(`isOnNavMesh=true`), 진입 가능 건물이 NavMesh를 정상 차단, 콘솔 에러 0건.
- [x] `manage_scene save`(Main, House Interior), `git status`로 변경 파일이 두 씬/새 스크립트 3개/신규 재질들/`Assets/Models/City`/`EditorBuildSettings.asset`로만 국한됨을 확인.

## 다음 단계 (사용자에게 보고 후 순서대로 진행 예정)
- 도시 구역 확장: 교차로/더 많은 거리·건물, 추가 건물 내부(상점/사무실 등 인테리어 템플릿 다양화).
- 광산(채석장/노천광 형태, 터널 입구→터널 내부 씬 연결) 구역.
- 지하 방공호(콘크리트 벙커 입구→벙커 내부 씬 연결) 구역.
- 시골 구역 재구축(이번에 삭제된 농장 컨셉을 새 대형 맵의 한 구역으로 재배치).
