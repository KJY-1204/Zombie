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

## CP17. 도시 구역 확장 (2차: 거리 연장 + 입구 2개 추가)
- [x] 사용자 확인: "도시 구역 더 확장" 선택.
- [x] 교차로 배치를 시도했으나 기존 건물 10동이 이미 도로 X축 대부분을 촘촘히 채우고 있어 십자 교차로용 여유 공간이 부족 — 대신 Ground를 90x90으로 확대하고 같은 큰길을 동서로 연장, 양 끝에 건물 4동(북 2/남 2) 추가하는 방향으로 조정(교차로는 다음 확장 때 별도 공간에 배치 예정, `context-notes.md`에 기록).
- [x] `Assets/Scenes/Store Interior.unity` 신규 — House Interior와 다른 형태(더 넓고 낮은 방, 선반 3개+계산대)로 시각적 차별화, (4000,0,4000) 포켓 좌표에 배치, Build Settings 등록.
- [x] 새 입구 2개 배치: `Building North Store Entrance`(X=36) → Store Interior 신규 연결, 기존 `Building South 1`(X=-12) → House Interior 재사용 연결(문 마커를 노란색으로 변경해 입구 표시).
- [x] `City Boundary`/`Full Map Camera`(orthoSize 40→50)를 90x90 크기에 맞게 재조정, NavMesh 재굽기+에셋 재저장.
      Verify: Play Mode에서 신규 Store 입구, 재사용 House 입구 둘 다 왕복 전환 검증(각각 정확한 스폰/복귀 좌표로 텔레포트, `sceneCount` 2→1 정상 전환). 좀비 스폰/총 발사 회귀 없음. 콘솔 에러 0건.

## CP18. 바닥 텍스처 + 랜드마크 건물 다양화 + 집 내부 방 구조 확장
사용자 피드백: "바닥이 엉성해 도시처럼 만들어달라" + "경찰서/소방서/병원/약국/아파트 등 배치해 단조로움 해소" + "집 내부는 거실/화장실/부엌/방2개는 되어야".

- [x] `manage_texture`로 아스팔트(노이즈)/보도블록(그리드)/도시 바닥(노이즈) 절차적 텍스처 3종 제작해 도로/인도/Ground 재질에 적용(타일링 스케일 지정으로 반복).
- [x] 도로 중앙선(노란 대시 23개) + 인도-차도 경계 연석(북/남) 추가.
- [x] 건물마다 바닥에 "대지" 패치 추가(주거형=잔디 Lawn.mat, 상업/공공형=포장 Lot Pavement.mat) 총 13곳.
- [x] 기존 건물 5동을 랜드마크로 재스킨(신규 모델링 없이 기존 CityBuilding 형태에 재질/서명 요소만 추가):
      - **경찰서**(Building North 0): 파란 몸체 + 흰 띠 + 지붕 위 파란 라이트바.
      - **소방서**(Building South 0): 빨간 몸체 + 전면 대형 차고문(검정 패널).
      - **병원**(Building North 3): 흰 몸체로 재도색 + 크기 확대(11x7x9) + 전면 빨간 십자가.
      - **약국**(Building South 2): 흰 몸체 + 작은 초록 십자가.
      - **아파트**(Building North 4, South 3 2동): 높이 15로 대폭 확대 + 층 구분선 4단.
      Verify: 45도 각도 스크린샷으로 파란 경찰서/빨간 소방서(차고문)/흰 병원(빨간 십자)/타워형 아파트(층선)가 한눈에 구분됨을 확인.
- [x] `House Interior.unity` 전면 재설계: 기존 단일 방(10x8) 삭제, 14x12 크기에 벽 12개 세그먼트로 거실(입구 바로 안쪽, 전체 폭)+부엌+화장실+침실 2개(총 5개 방, 문 간격으로 서로 연결)로 재구성. 방마다 간단한 가구(소파/TV장/부엌 조리대+식탁/변기+세면대/침대 2개) 배치.
      Verify: 탑뷰 스크린샷으로 5개 방 구획과 문 간격이 올바르게 뚫려있음을 확인(거실→부엌/화장실→각 침실로 이어지는 동선 성립). Play Mode에서 기존 House 입구를 통해 실제로 진입해 새 거실에 도착함을 확인(스폰 포인트가 자동으로 새 좌표에 맞게 반영됨 — 오브젝트 이름만 유지하면 Main 씬의 입구 코드 수정 불필요).
- [x] NavMesh 재굽기+에셋 재저장, Play Mode 왕복 전환/좀비 스폰/총 발사 회귀 확인, 콘솔 에러 0건.

## CP19. 낮/밤 순환 + 가로등 + 실제 도시 격자 교차로
사용자 요청: "낮과 밤이 일정시간 반복" + "가로등 몇 개 배치" + "실제 도시처럼 건물 배치".

- [x] `DayNightCycle.cs` 신규: Directional Light를 240초(기본값) 주기로 회전시켜 낮/밤 반복. 태양 고도(sin 기반)로 낮 정도(DayAmount 0~1)를 계산해 태양광 색상/세기와 환경광(RenderSettings.ambientLight)을 부드럽게 보간. 다른 스크립트가 낮/밤 여부를 읽을 수 있도록 `DayAmount`/`IsNight` 공개.
      Verify: 리플렉션으로 `elapsed`를 각 구간(0/0.15/0.25/0.5/0.75/0.9)으로 강제 이동시키며 `DayAmount`가 0→0.9→1→0→0→0으로 정확히 사인 곡선을 그림을 확인(0.25=한낮 정오, 0.5/0.75=밤).
- [x] `StreetLight.cs` 신규: `DayNightCycle.IsNight`를 매 프레임 확인해 밤에만 Point Light를 켜고 전구 머티리얼을 발광 색으로 바꿈(낮엔 꺼짐). 재질은 `renderer.material`(인스턴스)로 접근해 다른 가로등에 영향 없이 개별 제어.
      Verify: 리플렉션으로 낮/밤 강제 전환 후 `lamp.enabled`가 각각 False/True로 정확히 바뀜을 확인.
- [x] 가로등 프리팹형 구조(기둥+암+램프헤드+PointLight) 제작 후 메인 거리 인도(북/남)에 11개 배치(비활성 템플릿 1개 보존, 실제 배치본은 별도 복제).
- [x] **실제 도시 격자**: 기존 동서 큰길 동쪽 끝(X=36 건물 이후) 바깥에 새 남북 교차로(Cross Road, X=60)를 추가하고 그 교차로 양옆(서쪽 X=47/동쪽 X=73)에 건물 6동을 새로 배치해 진짜 "+" 교차로를 형성(이전 세션엔 공간 부족으로 보류했던 것을 이번에 별도 확장 공간에 구현). Ground를 (19,0,2) 중심 132x90 크기로 재배치/확대, City Boundary·Full Map Camera(orthoSize 50→70)도 함께 재조정.
      Verify: 탑뷰 스크린샷으로 실제 "+"자 교차로와 그 사방에 늘어선 건물 블록 구조를 확인 — 이전의 "한 줄짜리 거리"에서 "교차로가 있는 도시 블록"으로 개선됨.
- [x] NavMesh 재굽기+에셋 재저장, Play Mode 종합 회귀(총 발사, 좀비 스폰, 건물 입구 왕복 전환 — 이번엔 확장된 Ground/교차로에서도 정상 동작) 확인, 콘솔 에러 0건.

## CP20. Blender로 건물/연석 재제작 (기존 저폴리 버전이 "너무 로우폴리"라는 피드백)
사용자 요청: "블렌더로 도시 맵의 땅을 만들어주고 건물들도 다시 만들어줘 너무 로우폴리 였어".

- [x] Blender 헤드리스로 상세 건물(`BuildingDetailed.fbx`) 제작: 기초(Plinth)+몸체+처마(파라펫)+문(프레임+패널+계단)+전면 창문 6개(각각 프레임+유리, 총 18파츠). 기존 `CityBuilding.fbx`와 동일한 1x1x1 단위 규격+동일 좌표 부호 규칙(문이 로컬 Z+ 방향)을 유지해 기존 20개 건물의 위치/회전/스케일을 그대로 재사용 가능하게 설계.
      **[함정]** 처음에 "문 방향이 반대인 것 같다"고 판단해 블렌더 좌표 부호를 무심코 뒤집었다가 실제로는 원래(음수 Y)가 맞았음(FBX axis_forward='-Z' 변환은 Blender -Y → Unity +Z로 부호가 뒤집힘, 지난 세션에 이미 실측 확인해둔 사실인데 재확인 없이 "감"으로 고쳤다가 틀림) — 반드시 임포트 후 실제 좌표를 찍어서(`localPosition`) 확인하고 판단할 것, 유사한 부호 反전 실수가 이미 한 번 기록되어 있었는데도 재발함.
- [x] Blender로 연석(`Curb.fbx`, L자 단면 2단 박스) 제작, 기존 평평한 박스형 연석을 교체하고 교차로 쪽에는 새로 42개 세그먼트(8유닛 단위)로 타일링해 추가(기존엔 메인 거리에만 있었고 교차로엔 연석이 아예 없었음).
- [x] 보도블록(`SidewalkPanel.fbx`, 줄눈 홈 포함)도 제작은 했으나 이번 체크포인트에서는 아직 씬에 적용하지 않음(타일링 배치 작업량 문제로 다음으로 미룸 — 현재 보도는 기존 텍스처 적용된 평면 그대로 유지).
- [x] 기존 건물 20개(경찰서/소방서/병원/약국/아파트 2동 포함 랜드마크 전부)를 전부 삭제 후 새 상세 모델로 교체 — 각 건물의 기존 위치/회전/스케일/몸체 재질/입구 여부(노란 문)를 미리 기록해뒀다가 그대로 복원, 랜드마크 서명 오브젝트(경찰서 줄무늬/라이트바, 소방서 차고문, 병원·약국 십자가, 아파트 층선)는 별도 월드좌표 오브젝트라 건물 교체와 무관하게 그대로 유지됨.
      Verify: 45도 각도 스크린샷으로 건물 파사드에 실제 창틀+유리 격자, 기초 턱, 처마 라인이 뚜렷이 보임을 확인 — 기존 "박스+문양+지붕만" 버전과 확연히 다른 디테일 수준.
- [x] 새 건물 콜라이더+NavMeshModifier 재적용, NavMesh 재굽기+에셋 재저장.
- [x] Play Mode 종합 회귀: 건물 입구 왕복 전환(House Interior), 좀비 스폰, 총 발사 전부 정상. 콘솔 에러 0건.

## CP21. 보도블록 배치
- [x] 기존 평면 인도(북/남/교차로 서/동) 4개 삭제, `SidewalkPanel.fbx`(줄눈 홈)를 6유닛 세그먼트로 타일링해 58개 배치(메인 거리 28개 + 교차로 30개).
      Verify: 탑뷰/각도 스크린샷으로 줄눈이 반복되는 실제 보도블록 패턴 확인, 세그먼트 이음매에 틈/겹침 없음. Play Mode 진입해 플레이어 정상 확인, 콘솔 에러 0건. 패널은 콜라이더 없음(장식용, 보행 차단 없음) — NavMesh에 영향 없어 재굽기 불필요.

## 다음 단계 (사용자에게 보고 후 순서대로 진행 예정)
- 도로 자체(아스팔트)도 원한다면 미세한 크라운/포트홀 등 지오메트리 디테일 추가 검토.
- 도시 구역: 교차로를 더 늘려 완전한 격자로 확장, 인테리어 템플릿 추가 다양화(Store Interior도 방 구조 보강 검토).
- 야간 하늘(스카이박스)이 절차적 스카이박스라 태양 세기를 낮춰도 하늘 자체 밝기 변화가 크지 않음 — 필요시 카메라 클리어 플래그를 Solid Color로 바꾸고 낮/밤에 따라 배경색도 함께 보간하는 개선 고려(다음 세션 참고용, `context-notes.md` 기록).
- 광산(채석장/노천광 형태, 터널 입구→터널 내부 씬 연결) 구역.
- 지하 방공호(콘크리트 벙커 입구→벙커 내부 씬 연결) 구역.
- 시골 구역 재구축(이번에 삭제된 농장 컨셉을 새 대형 맵의 한 구역으로 재배치).
