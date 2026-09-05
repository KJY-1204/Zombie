// 사용 시 체력을 회복시키는 아이템 데이터
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable/HealthItemData", fileName = "Health Item Data")]
public class HealthItemData : ItemData {
    public float health = 50f; // 회복할 체력 수치

    public override void Use(GameObject target) {
        // 전달받은 게임 오브젝트로부터 LivingEntity 컴포넌트 가져오기 시도
        LivingEntity life = target.GetComponent<LivingEntity>();

        // LivingEntity 컴포넌트가 있다면 체력 회복 실행
        if (life != null)
        {
            life.RestoreHealth(health);
        }
    }
}
