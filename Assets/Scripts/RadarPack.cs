// 인벤토리에 담기는 레이더 아이템
using UnityEngine;

public class RadarPack : MonoBehaviour, IItem {
    public ItemData itemData; // 인벤토리에 추가할 아이템 데이터

    public void Use(GameObject target) {
        // 전달받은 게임 오브젝트의 인벤토리에 아이템 추가
        Inventory inventory = target.GetComponent<Inventory>();
        if (inventory != null)
        {
            inventory.Add(itemData, 1);
        }

        // 인벤토리에 담겼으므로, 월드에 있던 자신을 파괴
        Destroy(gameObject);
    }
}
