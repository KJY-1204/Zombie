# Checklist — Slice 2: 레이더로 좀비 임시 표시

## CP1. 레이어 + 좀비 미니맵 마커
- [x] `MinimapZombie` 레이어 추가 (슬롯 12).
- [x] `Zombie.prefab`에 미니맵 전용 마커(Quad, 로컬 회전 (90,0,0) — Slice 1에서 검증된 방향, 콜라이더 없음, layer=MinimapZombie, scale 0.7) 자식 추가.
- [x] `Assets/Materials/MinimapZombieMarker.mat` 생성 (URP Unlit, 빨간색 RGBA(1, 0.15, 0.1, 1)).
      Verify: 컴파일 에러 0건.

## CP2. MinimapRadarController
- [x] `Assets/Scripts/MinimapRadarController.cs` 신규 생성, `Minimap Camera`에 부착.
- [x] 싱글톤 패턴(GameManager와 동일), `ActivateRadar()`(타이머 갱신 + 컬링 마스크 토글), `Update()`에서 만료 체크(`Time.time` 기준, timeScale 영향 없음).
      Verify: 코드로 `ActivateRadar()` 호출 → 컬링 마스크 2049→6145(MinimapZombie 비트 포함) 확인. `Camera.Render()`+`ReadPixels`로 좀비 마커 위치 픽셀이 정확히 마커 색상(RGBA 1,0.149,0.102,1)으로 렌더링됨을 픽셀 단위로 확인. `revealEndTime`을 과거로 강제 설정 후 `Update()` 리플렉션 호출 → 마스크 6145→2049(제외) 확인. 활성 중 재사용 시 `revealEndTime`이 항상 `Time.time+revealDuration`으로 재설정됨(리셋) 확인.

## CP3. RadarPack 아이템
- [x] `Assets/Scripts/RadarPack.cs` 신규 생성 (IItem, AmmoPack과 동일 패턴).
- [x] `Assets/Materials/RadarPackVisual.mat` 생성 (URP Unlit, 시안색 RGBA(0.15, 0.95, 1, 1)).
- [x] `Assets/Prefabs/RadarPack.prefab` 생성 (SphereCollider trigger radius 0.4 + Rotator + RadarPack + Sphere 비주얼 자식 scale 0.4, 자식엔 콜라이더 없음).
- [x] 씬의 `Item Spawner`의 `items` 배열에 RadarPack 추가 (AmmoPack/HealthPack/Coin과 함께 무작위 스폰 풀에 편입).
      Verify: `RadarPack.Use(player)` 직접 호출 → `MinimapRadarController.instance` 통해 컬링 마스크가 정상적으로 좀비 포함 상태로 바뀜 확인(Destroy는 프레임 종료 시 처리되는 Unity 정상 동작).

## CP4. 통합 검증
- [x] Unity 컴파일 에러 0건.
- [x] 좀비 점블랭크 레이캐스트로 `layer=10(Enemy)`, `IDamageable` 정상, `NavMeshAgent` 정상 확인 — 마커 추가로 인한 회귀 없음.
- [x] `AmmoPack.Use()` 회귀 테스트: 탄약 100→130 정상 증가 확인.

## Slice 2 완료
위 CP1~CP4 전부 검증 완료. 다음은 `plan.md`의 Slice 3(인벤토리 foundation)으로 진행 — 지금은 아이템을 주우면 즉시 발동되는 임시 패턴이고, Slice 3에서 실제 인벤토리에 저장했다가 원할 때 사용하는 방식으로 교체 예정.
