// 인벤토리에 저장되는 아이템의 불변 데이터를 표현하는 추상 스크립터블 오브젝트
using UnityEngine;

public abstract class ItemData : ScriptableObject {
    public string displayName; // 인벤토리 UI에 표시될 이름

    // 아이템을 사용했을 때의 효과. target은 효과가 적용될 대상(주로 플레이어)
    public abstract void Use(GameObject target);
}
