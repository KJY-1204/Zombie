// 미니맵 카메라가 플레이어 위치를 따라가도록 하는 컴포넌트
using UnityEngine;

public class MinimapFollow : MonoBehaviour {
    public float height = 15f; // 플레이어 머리 위 미니맵 카메라 높이

    private Transform target; // 따라갈 플레이어 트랜스폼

    private void Start() {
        // 태그로 플레이어를 한 번만 찾아 캐싱 (Update에서 매번 검색하지 않음)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            target = player.transform;
        }
    }

    // LateUpdate는 플레이어 이동 처리 이후에 실행되어 떨림 없이 따라감
    private void LateUpdate() {
        if (target == null) return;

        // 정북 고정(north-up) — 플레이어 위치만 따라가고 회전은 고정
        transform.position = new Vector3(target.position.x, target.position.y + height, target.position.z);
    }
}
