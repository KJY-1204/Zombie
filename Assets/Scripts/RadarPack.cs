// 사용 시 일정 시간 동안 미니맵에 좀비를 드러내는 레이더 아이템
using UnityEngine;

public class RadarPack : MonoBehaviour, IItem {
    public void Use(GameObject target) {
        // 레이더 컨트롤러에게 좀비 노출을 요청
        if (MinimapRadarController.instance != null)
        {
            MinimapRadarController.instance.ActivateRadar();
        }

        // 사용되었으므로, 자신을 파괴
        Destroy(gameObject);
    }
}
