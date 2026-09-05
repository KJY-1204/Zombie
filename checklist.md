# Checklist — Slice 6: 맵 확장 (1차, 기존 묘지 마당을 비례 확대)

## CP1. 기존 구조 조사
- [x] `Ground`(Plane, 22x22, MeshCollider+NavMeshModifier), `Fence`(시각용 메쉬, 콜라이더 없음), `Fence Collider`(박스 콜라이더 12개, 실제 충돌 담당) 3개 오브젝트가 전부 같은 중심(-1.5, *, 2)을 공유하는 동심원 구조임을 확인.
- [x] `Navigation` 오브젝트의 `NavMeshSurface`가 `collectObjects=MarkedWithModifier` 방식이라 Ground/Fence의 NavMeshModifier만 있으면 재굽기(Bake) 시 자동으로 새 크기를 반영함을 확인.
- [x] `ItemSpawner`는 플레이어 위치 기준 상대 반경(maxDistance)으로 스폰 — 맵 크기와 무관, 수정 불필요.
- [x] `ZombieSpawner`는 `Spawn Points` 하위 4개 Transform 사용 — 기존 경계(22x22)에 맞춰 배치되어 있어 재배치 필요.
- [x] `Full Map Camera`(orthoSize=16, 1024x1024 텍스처)는 시작 시 1회 촬영이라 지도 크기를 바꾸면 orthoSize 재조정 필요. `Minimap Camera`는 플레이어 추종형이라 맵 크기와 무관, 수정 불필요.

## CP2. 지형/충돌 비례 확대 (배율 k = 50/22 ≈ 2.27, 목표 50x50)
- [x] `Ground` localScale (2.2,1,2.2) → (5.0,1,5.0) — 순수 스케일만 변경(피벗이 이미 중심과 일치).
- [x] `Fence`, `Fence Collider`: 각각의 월드 바운즈 중심을 구해 Ground와 동일한 중심(-1.5,*,2)을 유지하도록 스케일(X/Z만 k배, Y 높이는 그대로)과 포지션을 함께 재계산해 적용(코드로 계산 후 적용 — 손 계산 대신 `Renderer.bounds`/`BoxCollider.bounds`로 직접 측정).
      Verify: 적용 후 세 오브젝트의 바운즈 중심이 모두 (-1.5, *, 2)로 정확히 일치함을 코드로 재확인. 정상 위 스크린샷(45도 위)으로 울타리가 새 Ground 전체를 대칭으로 둘러싸는 것을 육안 확인.

## CP3. 스폰/카메라 갱신
- [x] `Spawn Points`(4개)를 동일한 중심 기준 배율 k로 재배치(기존 경계 근처였던 위치가 새 경계 근처로 이동).
- [x] `Full Map Camera.orthographicSize` 16 → 30(새 울타리 바운즈(~28.4) + 여유 마진을 포함하도록).
      Verify: Play Mode에서 지도 창(M)을 열어 새로 확장된 전체 구역이 잘리지 않고 여유 있게 프레임 안에 들어오는 것을 스크린샷으로 확인.

## CP4. NavMesh 재굽기 + 에셋 저장
- [x] `NavMeshSurface.BuildNavMesh()` 재실행.
- [x] **[함정]** 스크립트로 `BuildNavMesh()`만 호출하면 새로 만들어진 `NavMeshData`가 기존 `Assets/Scenes/Main/NavMesh-Navigation.asset`에 저장되지 않고 메모리상의 임시 오브젝트로만 남음(`AssetDatabase.GetAssetPath`가 빈 문자열) — 다음 세션에서 씬을 다시 열면 옛날(22x22) NavMesh로 되돌아가는 문제. 기존 에셋을 지우고 새 데이터로 `AssetDatabase.CreateAsset`+`SaveAssets`로 같은 경로에 다시 저장한 뒤 `NavMeshSurface.navMeshData`를 재연결해서 해결.
      Verify: `git status`에 `NavMesh-Navigation.asset`이 실제로 modified로 표시됨을 확인(수정 전에는 반영 안 됨을 먼저 확인한 뒤 대조).
- [x] Play Mode 재검증: 좀비 2마리가 새 스폰 포인트 위치(24.64,0.5,2 / -1.5,0.5,-25.27)에서 정상 스폰되고 `NavMeshAgent.isOnNavMesh=true`. 플레이어를 옛 경계 밖(x=20)로 텔레포트해도 바닥이 꺼지지 않고 정상 서 있음. 컴파일 에러 0건, 콘솔 에러 0건.

## CP5. 씬 저장 및 회귀 확인
- [x] `manage_scene save`로 `Main.unity` 저장(2회 — NavMesh 에셋 재연결 전/후).
- [x] `git status`로 변경 파일이 `Main.unity`, `Main/NavMesh-Navigation.asset`으로만 국한됨을 확인(Packages/ProjectSettings는 기존부터 있던 무관한 변경).

# Checklist — 맵 확장 2차: 시골(농장) 테마 장식 추가

## CP6. 조사
- [x] `Assets/Models/Level Art`에 있는 기존 모델은 전부 묘지 테마(묘비/십자가/석관/철제 울타리 등)뿐, 농장/시골 전용 에셋은 없음을 확인. `generate_model`(Tripo/Meshy) API 키 미설정 확인(`list_providers`) — AI 3D 생성 불가.
- [x] 결정: 새 3D 에셋 임포트 없이 기존 Unity 프리미티브(Cube/Cylinder/Sphere) + 기존 묘지 울타리 FBX(`fenceBroken.fbx`, 목재/철제 느낌 재활용) 조합으로 저폴리 스타일에 맞는 농장 소품을 직접 구성.

## CP7. 농장 클러스터 제작 (`Level Art/Rural Decor`)
- [x] 헛간(Barn): Cube 몸체(6x4x8, 빨강) + Cube를 45도 회전시켜 만든 박공지붕(회색) — 스크린샷으로 실루엣 확인.
- [x] 사일로(Silo): Cylinder 몸체 + 납작한 Cylinder 지붕(회색), 헛간 옆에 배치.
- [x] 목장 울타리: 기존 `fenceBroken.fbx` 7개를 헛간 앞에 일렬 배치.
- [x] 건초더미(Hay Bale) 3개: Cylinder, 황토색, 헛간 옆에 삼각 더미로 쌓음.
- [x] 나무(Tree) 11그루: Cylinder(줄기, 갈색) + Sphere(수관, 초록) 조합을 확장된 외곽 영역 전체에 위치/크기/회전을 무작위로 살짝 변주해 분산 배치(기존 묘지 핵심부 반경 안쪽은 피함).
- [x] 새 재질 5종 생성(`Assets/Models/Materials/`): Barn Red, Roof Gray, Silo Metal, Foliage Green, Hay Tan, Bark Brown.
      Verify: 45도/탑뷰 스크린샷으로 전체 배치가 자연스러운 농장+수목 실루엣을 이루는지 육안 확인.

## CP8. 충돌/NavMesh 반영
- [x] 헛간/사일로는 프리미티브 생성 시 자동으로 붙는 콜라이더(BoxCollider/CapsuleCollider)로 플레이어 물리 차단 확보.
- [x] 헛간·사일로 몸체에 `NavMeshModifier`(area=Not Walkable) 추가 — 좀비가 벽을 뚫고 다니지 않도록 함(나무/건초더미는 크기가 작아 이번 범위에서는 생략, 필요시 추후 추가).
- [x] `NavMeshSurface.BuildNavMesh()` 재실행 + 이전에 발견한 함정(에셋 재저장 필요)대로 `Assets/Scenes/Main/NavMesh-Navigation.asset`에 다시 저장·재연결.
      Verify: `NavMesh.SamplePosition`으로 헛간 중심(18,0,18)이 반경 0.5 안에서 NavMesh 위에 없음을 확인(좀비가 헛간을 통과하지 못함). Play Mode에서 플레이어를 헛간 옆(14.5,0,18)으로 이동해도 정상 서 있음, 콘솔 에러 0건.
- [x] `manage_scene save`, `git status`로 변경 파일이 `Main.unity`/`NavMesh-Navigation.asset`/신규 재질 5종으로만 국한됨을 확인.
