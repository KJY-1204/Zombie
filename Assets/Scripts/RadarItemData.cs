// 사용 시 미니맵에 좀비를 임시로 드러내는 레이더 아이템 데이터
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable/RadarItemData", fileName = "Radar Item Data")]
public class RadarItemData : ItemData {
    public override void Use(GameObject target) {
        // 레이더 컨트롤러에게 좀비 노출을 요청
        if (MinimapRadarController.instance != null)
        {
            MinimapRadarController.instance.ActivateRadar();
        }
    }
}
