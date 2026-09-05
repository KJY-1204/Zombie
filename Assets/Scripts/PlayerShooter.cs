using UnityEngine;

// 현재 장착된 무기(총 또는 근접무기)를 사용하고 무기 전환을 관리
// 알맞은 애니메이션을 재생하고 IK를 사용해 캐릭터 양손이 현재 무기의 손잡이에 위치하도록 조정
public class PlayerShooter : MonoBehaviour {
    // 장착 가능한 무기 종류
    public enum WeaponType {
        Gun,
        Melee
    }

    public Gun gun; // 사용할 총
    public MeleeWeapon meleeWeapon; // 사용할 근접무기
    public Transform gunPivot; // 무기 배치의 기준점

    public WeaponType currentWeapon { get; private set; } = WeaponType.Gun; // 현재 장착된 무기

    private PlayerInput playerInput; // 플레이어의 입력
    private Animator playerAnimator; // 애니메이터 컴포넌트

    private void Start() {
        // 사용할 컴포넌트들을 가져오기
        playerInput = GetComponent<PlayerInput>();
        playerAnimator = GetComponent<Animator>();
    }

    private void OnEnable() {
        // 슈터가 활성화될 때 현재 장착된 무기만 활성화
        EquipWeapon(currentWeapon);
    }

    private void OnDisable() {
        // 슈터가 비활성화될 때 두 무기 모두 비활성화
        gun.gameObject.SetActive(false);
        meleeWeapon.gameObject.SetActive(false);
    }

    private void Update() {
        // 무기 전환 입력 감지
        if (playerInput.switchWeapon)
        {
            EquipWeapon(currentWeapon == WeaponType.Gun ? WeaponType.Melee : WeaponType.Gun);
        }

        // 입력을 감지하고 현재 장착된 무기로 발사/공격
        if (playerInput.fire)
        {
            if (currentWeapon == WeaponType.Gun)
            {
                gun.Fire();
            }
            else
            {
                meleeWeapon.Attack();
            }
        }
        else if (playerInput.reload && currentWeapon == WeaponType.Gun)
        {
            // 재장전 입력 감지시 재장전 (총 장착 중에만 가능)
            if (gun.Reload())
            {
                // 재장전 성공시에만 재장전 애니메이션 재생
                playerAnimator.SetTrigger("Reload");
            }
        }

        // 남은 탄약 UI를 갱신
        UpdateUI();
    }

    // 무기를 전환하고 각 무기 GameObject의 활성 상태를 갱신
    private void EquipWeapon(WeaponType weapon) {
        currentWeapon = weapon;
        gun.gameObject.SetActive(weapon == WeaponType.Gun);
        meleeWeapon.gameObject.SetActive(weapon == WeaponType.Melee);
    }

    // 탄약 UI 갱신
    private void UpdateUI() {
        if (gun != null && UIManager.instance != null)
        {
            // UI 매니저의 탄약 텍스트에 탄창의 탄약과 남은 전체 탄약을 표시
            UIManager.instance.UpdateAmmoText(gun.magAmmo, gun.ammoRemain);
        }
    }

    // 애니메이터의 IK 갱신
    private void OnAnimatorIK(int layerIndex) {
        // 무기의 기준점 gunPivot을 3D 모델의 오른쪽 팔꿈치 위치로 이동
        gunPivot.position =
            playerAnimator.GetIKHintPosition(AvatarIKHint.RightElbow);

        // 현재 장착된 무기의 손잡이 트랜스폼을 가져온다
        Transform leftHandMount = currentWeapon == WeaponType.Gun ? gun.leftHandMount : meleeWeapon.leftHandMount;
        Transform rightHandMount = currentWeapon == WeaponType.Gun ? gun.rightHandMount : meleeWeapon.rightHandMount;

        // IK를 사용하여 왼손의 위치와 회전을 현재 무기의 왼쪽 손잡이에 맞춘다
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);

        playerAnimator.SetIKPosition(AvatarIKGoal.LeftHand,
            leftHandMount.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.LeftHand,
            leftHandMount.rotation);

        // IK를 사용하여 오른손의 위치와 회전을 현재 무기의 오른쪽 손잡이에 맞춘다
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);

        playerAnimator.SetIKPosition(AvatarIKGoal.RightHand,
            rightHandMount.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.RightHand,
            rightHandMount.rotation);
    }
}
