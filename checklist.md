# Checklist — 근접무기(삽) + 무기 전환 시스템

## CP1. 데이터/스크립트 기반
- [x] `Assets/Scripts/MeleeWeaponData.cs` 신규 (ScriptableObject, damage/attackRange/attackRadius/swingDuration/timeBetAttack).
- [x] `Assets/ScriptableData/Shovel Data.asset` 생성.
- [x] `Assets/Scripts/MeleeWeapon.cs` 신규 (leftHandMount/rightHandMount/meleeData, Attack()→SwingRoutine() 코루틴, OverlapSphere 판정).
- [x] `Assets/Scripts/Gun.cs`에 `leftHandMount`/`rightHandMount` 필드 추가(로직 변경 없음, PlayerShooter가 참조할 수 있게).
      Verify: 컴파일 에러 0건 (read_console 확인 완료).

## CP2. Melee Weapon 프리팹
- [x] `Assets/Prefabs/Melee Weapon.prefab` 생성: shovel.fbx 모델 + Left Handle/Right Handle 트랜스폼 + AudioSource + MeleeWeapon 컴포넌트.
- [x] `Player Character.prefab`의 `Gun Pivot` 아래 `Gun`의 형제로 배치, 기본 비활성.
      Verify: Main Camera 게임 뷰 스크린샷으로 확인 — 삽을 어깨 위로 비스듬히 들어올린 자연스러운 대기 자세, 총과 동일한 IK 손 마운트 컨벤션으로 부착됨을 눈으로 확인. 완벽한 손맛은 헤드리스 한계로 사용자 직접 플레이 권장.

## CP3. 무기 전환 로직
- [x] `PlayerInput.cs`에 `switchWeapon`(Q키) 추가.
- [x] `PlayerShooter.cs` 리팩터링: currentWeapon(Gun/Melee) 상태, Q 입력 시 두 무기 GameObject SetActive 반전 + IK 마운트 참조 교체, fire 입력을 현재 무기에 라우팅, reload는 Gun일 때만.
      Verify: 리플렉션으로 `EquipWeapon(Melee)` 직접 호출 → gunActive=False/meleeActive=True로 정확히 반전 확인. `OnAnimatorIK`가 실제로 갱신하는 애니메이터 IK 위치가 현재 무기(Melee)의 leftHandMount/rightHandMount 월드 좌표와 정확히 일치함을 확인(Gun으로 되돌린 뒤에도 동일하게 일치 재확인).

## CP4. 판정 검증
- [x] 좀비 근처에서 `meleeWeapon.Attack()` 직접 호출 → 스윙 진행 중 판정 시점에 좀비 체력이 실제로 감소하는지 확인.
- [x] 스윙 중 무기 로컬 회전이 프레임별로 변하는지 확인.
- [x] Gun 회귀: 다시 총으로 전환 후 발사/재장전 정상 동작 확인.
      Verify: 에디터를 일시정지(`isPaused=true`)한 뒤 `EditorApplication.Step()`으로 프레임을 한 장씩 진행하며 확인 — 무기 로컬 회전이 310°(대기)→318.7°→...→80°(스윙 정점)까지 프레임마다 매끄럽게 변하고, t≈0.4 지점(step 6)에서 좀비 체력이 20→-60으로 정확히 1회 감소(중복 판정 없음, hitApplied 플래그 정상 동작), 스윙 종료 후 대기 자세(310°)로 복귀함을 확인. Gun으로 전환 후 `Fire()` 호출 시 magAmmo 25→24 정상 감소, `Reload()` 호출 시 state가 Reloading으로 정상 전환됨을 확인.

## CP5. 통합 검증
- [x] Unity 컴파일 에러 0건.
- [x] 좀비 관련 기존 스크립트(Zombie.cs, LivingEntity.cs) 회귀 없음 — 기존 `IDamageable.OnDamage()` 인터페이스를 그대로 재사용했으며 수정하지 않음. 좀비 AI/콜라이더/레이어는 변경 없이 정상 동작(테스트 중 좀비가 실제로 플레이어를 인식하고 공격하는 것도 확인됨).
