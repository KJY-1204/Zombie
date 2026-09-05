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
