// 근접무기를 스크립트로 회전시켜 휘두르고, 판정 범위 안의 대상에게 데미지를 주는 컴포넌트
using System.Collections;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour {
    public Transform leftHandMount; // 왼손이 위치할 손잡이 트랜스폼
    public Transform rightHandMount; // 오른손이 위치할 손잡이 트랜스폼

    public MeleeWeaponData meleeData; // 근접무기의 현재 데이터
    public LayerMask hittableLayers; // 판정 대상이 될 레이어 (기본: Enemy)

    private AudioSource weaponAudioPlayer; // 무기 소리 재생기
    private Coroutine swingRoutine; // 진행 중인 스윙 코루틴
    private float lastAttackTime; // 마지막으로 공격한 시점

    // 뒤로 젖힌 대기 자세와 내려찍는 자세 (로컬 회전)
    private static readonly Quaternion ReadyRotation = Quaternion.Euler(-50f, 0f, 0f);
    private static readonly Quaternion SwingRotation = Quaternion.Euler(80f, 0f, 0f);

    private void Awake() {
        weaponAudioPlayer = GetComponent<AudioSource>();
    }

    private void OnEnable() {
        // 장착될 때마다 대기 자세로 초기화
        transform.localRotation = ReadyRotation;
        lastAttackTime = 0f;
    }

    // 현재 공격이 가능한 상태인지 여부
    public bool CanAttack {
        get { return swingRoutine == null && Time.time >= lastAttackTime + meleeData.timeBetAttack; }
    }

    // 공격 시도
    public void Attack() {
        if (!CanAttack) return;

        lastAttackTime = Time.time;
        swingRoutine = StartCoroutine(SwingRoutine());
    }

    // 실제 휘두르는 처리: 대기 자세 -> 내려찍는 자세로 회전, 중간 지점에서 판정 1회 실행
    private IEnumerator SwingRoutine() {
        if (weaponAudioPlayer != null && meleeData.swingClip != null)
        {
            weaponAudioPlayer.PlayOneShot(meleeData.swingClip);
        }

        bool hitApplied = false;
        float elapsed = 0f;

        while (elapsed < meleeData.swingDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / meleeData.swingDuration);
            transform.localRotation = Quaternion.Slerp(ReadyRotation, SwingRotation, t);

            // 스윙이 40% 진행된 시점에 판정을 1회만 실행
            if (!hitApplied && t >= 0.4f)
            {
                hitApplied = true;
                DetectHit();
            }

            yield return null;
        }

        // 내려찍은 자세에서 잠시 멈췄다가 대기 자세로 복귀
        yield return new WaitForSeconds(0.1f);
        transform.localRotation = ReadyRotation;

        swingRoutine = null;
    }

    // 플레이어 정면의 판정 지점에서 겹치는 대상에게 데미지 적용
    private void DetectHit() {
        Transform playerTransform = transform.root;
        Vector3 origin = playerTransform.position + Vector3.up
            + playerTransform.forward * meleeData.attackRange;

        Collider[] hits = Physics.OverlapSphere(origin, meleeData.attackRadius, hittableLayers);
        bool anyHit = false;

        foreach (Collider hit in hits)
        {
            IDamageable target = hit.GetComponent<IDamageable>();
            if (target == null) continue;

            anyHit = true;
            Vector3 hitPoint = hit.ClosestPoint(origin);
            Vector3 hitDirection = (hit.transform.position - playerTransform.position).normalized;
            target.OnDamage(meleeData.damage, hitPoint, hitDirection);
        }

        if (anyHit && weaponAudioPlayer != null && meleeData.hitClip != null)
        {
            weaponAudioPlayer.PlayOneShot(meleeData.hitClip);
        }
    }
}
