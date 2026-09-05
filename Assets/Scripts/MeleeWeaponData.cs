// 근접무기의 공격력/사거리/스윙 속도 등 불변 데이터를 담는 스크립터블 오브젝트
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable/MeleeWeaponData", fileName = "Melee Weapon Data")]
public class MeleeWeaponData : ScriptableObject {
    public float damage = 40f; // 한 번 스윙에 적용할 데미지
    public float attackRange = 1.2f; // 플레이어 정면 기준 판정 지점까지의 거리
    public float attackRadius = 0.9f; // 판정 구체의 반지름
    public float swingDuration = 0.3f; // 휘두르는 동작의 전체 소요 시간(초)
    public float timeBetAttack = 0.6f; // 공격 간격(연타 방지)

    public AudioClip swingClip; // 휘두를 때 재생할 소리 (없으면 재생 안 함)
    public AudioClip hitClip; // 명중 시 재생할 소리 (없으면 재생 안 함)
}
