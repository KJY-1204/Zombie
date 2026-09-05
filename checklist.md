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
위 CP1~CP5 전부 검증 완료.

## CP6. 사용자 피드백 3건 수정
- [x] **[버그] 좀비 미니맵 마커가 메인 화면에 노출됨**: `Main Camera` 컬링 마스크가 `MinimapPlayer`만 제외하고 `MinimapZombie`(Slice 2에서 추가된 레이어)는 빠뜨렸던 것을 발견해 수정(-2049 → -6145). 실제 Main Camera를 좀비 근처로 옮겨 스크린샷 확인 — 더 이상 빨간 사각형이 안 보임.
- [x] **인벤토리 슬롯을 아이콘 UI로 교체**: `ItemData`에 `icon`(Sprite) 필드 추가, 체력팩(빨간 원+흰 십자)/레이더(초록 동심원) 아이콘을 코드로 생성해 연결. `UIManager`의 텍스트 한 줄 방식을 `inventorySlotIcons[]`/`inventorySlotCounts[]` + `UpdateInventorySlot(index, icon, quantity)`로 교체. HUD 좌하단에 반투명 검은 배경의 네모 슬롯 2칸(Slot 0/Slot 1) 생성, 각각 아이콘+우하단 수량 텍스트. `Inventory.slots`도 4→2칸으로 정리(실제 사용하는 슬롯 수와 UI를 일치시킴).
      Verify: `Add`/스크린샷으로 슬롯 안에 아이콘+숫자가 정상 표시됨을 픽셀로 확인.
- [x] **레이더 활성 중 사망한 좀비가 미니맵에서 안 보이게**: `Zombie.cs`가 `Awake()`에서 `Minimap Marker` 자식을 캐싱해두고, `Die()`에서 `SetActive(false)`로 즉시 숨김(콜라이더 비활성화와 동일한 타이밍). 강제 사망 테스트로 마커가 `activeSelf: True→False`로 바뀜을 확인.
      Verify: 좀비 강제 사망 후 마커 비활성화 확인, 살아있는 좀비 마커는 정상 동작 유지.

## Slice 3 + 후속 수정 완료. 다음은 `plan.md`의 Slice 4(플레이어 상태 UI)로 진행.
