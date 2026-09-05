# Checklist — Slice 3: 인벤토리 foundation (체력팩 + 레이더)

## CP1. ItemData 및 하위 클래스
- [x] `Assets/Scripts/ItemData.cs` 신규 (추상 ScriptableObject, `displayName` + `abstract Use(GameObject target)`).
- [x] `Assets/Scripts/HealthItemData.cs` 신규 (`health` 필드, `HealthPack.Use()`의 회복 로직 이전).
- [x] `Assets/Scripts/RadarItemData.cs` 신규 (`RadarPack.Use()`의 레이더 발동 로직 이전).
- [x] `Assets/ScriptableData/Health Item Data.asset`, `Radar Item Data.asset` 생성.
      Verify: 컴파일 에러 0건.

## CP2. Inventory 컴포넌트
- [x] `Assets/Scripts/Inventory.cs` 신규, 슬롯 4칸, `Add`/`UseSlot` 구현.
- [x] `PlayerInput.cs`에 `useSlot1`(Alpha1)/`useSlot2`(Alpha2) 추가.
- [x] `Player Character.prefab`에 `Inventory` 컴포넌트 부착.
      Verify: `Add(healthItemData,2)` → 슬롯 반영("1:체력팩 x2") 확인. `UseSlot(0)` → 체력 70→120(+50) 회복 + 수량 2→1 차감 확인. `Add(radarItemData,1)`+`UseSlot(1)` → 컬링 마스크 2049→6145(레이더 활성화) + 수량 1→0(슬롯 소진, UI에서 사라짐) 확인.

## CP3. 픽업 스크립트 연동
- [x] `HealthPack.cs` 수정: `itemData` 필드 추가, `Use()`가 `Inventory.Add()` 호출로 변경(직접 회복 제거).
- [x] `RadarPack.cs` 수정: `itemData` 필드 추가, `Use()`가 `Inventory.Add()` 호출로 변경(직접 레이더 발동 제거).
- [x] `HealthPack.prefab`/`RadarPack.prefab`에 각각 itemData 에셋 연결.
      Verify: `HealthPack.Use(player)` 호출 시 체력 변화 없이(120→120) 인벤토리에만 추가됨 확인(회귀: 더 이상 즉시 회복 안 함). `RadarPack.Use(player)`도 컬링 마스크 변화 없이(2049→2049) 인벤토리에만 추가됨 확인.

## CP4. UI
- [x] `UIManager.cs`에 `inventoryText` 필드 + `UpdateInventoryText(string)` 추가.
- [x] `HUD Canvas.prefab`에 `Inventory Text` 오브젝트 추가(Ammo Display 위, 좌하단).
      Verify: `Add`/`UseSlot` 호출마다 `UIManager.inventoryText.text`가 실제로 갱신되고, 빈 슬롯은 표시에서 빠짐을 확인.

## CP5. 통합 검증
- [x] Unity 컴파일 에러 0건.
- [x] `AmmoPack.Use()` 회귀 확인: 탄약 100→130 그대로 즉시 적용됨(변경 없음).
- [x] 좀비 점블랭크 레이캐스트: `layer=10(Enemy)`, `IDamageable` 정상 — 회귀 없음.

## Slice 3 완료
위 CP1~CP5 전부 검증 완료. 다음은 `plan.md`의 Slice 4(플레이어 상태 UI)로 진행.
