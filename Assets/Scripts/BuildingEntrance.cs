// 건물/터널/벙커 등 입구에 부착: 플레이어가 범위 안에서 상호작용하면 지정된 내부 씬으로 이동시킴
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BuildingEntrance : MonoBehaviour {
    public string interiorSceneName; // 진입할 내부 씬 이름 (Build Settings에 등록되어 있어야 함)
    public Transform returnPoint; // 나갈 때 되돌아올 위치/방향 (비워두면 이 오브젝트 자신의 위치 사용)
    public string promptText = "E: 들어가기"; // 범위 안에 있을 때 표시할 안내 문구

    private bool playerInRange;
    private PlayerInput playerInput;

    private void Start() {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            playerInput = player.GetComponent<PlayerInput>();
        }
    }

    private void Update() {
        if (!playerInRange || playerInput == null) return;

        if (playerInput.interact) {
            Transform rp = returnPoint != null ? returnPoint : transform;
            SceneTransitionManager.instance.EnterInterior(interiorSceneName, rp.position, rp.rotation);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        if (UIManager.instance != null) UIManager.instance.ShowInteractPrompt(promptText);
    }

    private void OnTriggerExit(Collider other) {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        if (UIManager.instance != null) UIManager.instance.HideInteractPrompt();
    }
}
