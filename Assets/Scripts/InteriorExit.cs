// 내부 씬(건물/터널/벙커)의 출구에 부착: 상호작용하면 원래 있던 외부 위치로 되돌아감
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteriorExit : MonoBehaviour {
    public string promptText = "E: 나가기"; // 범위 안에 있을 때 표시할 안내 문구

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
            SceneTransitionManager.instance.ExitInterior();
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
