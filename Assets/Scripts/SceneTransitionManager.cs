// 건물/터널/벙커 입구에서 내부 씬을 추가 로드하고, 나갈 때 원래 위치로 되돌리는 씬 전환 관리자
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class SceneTransitionManager : MonoBehaviour {
    // 싱글톤 접근용 프로퍼티 (다른 매니저들과 동일한 패턴)
    public static SceneTransitionManager instance {
        get {
            if (m_instance == null) {
                m_instance = FindObjectOfType<SceneTransitionManager>();
            }
            return m_instance;
        }
    }

    private static SceneTransitionManager m_instance;

    public string InteriorSpawnPointName = "Interior Spawn Point"; // 내부 씬에서 플레이어를 배치할 지점의 오브젝트 이름

    private string currentInteriorScene; // 현재 추가 로드되어 있는 내부 씬 이름 (없으면 null)
    private Vector3 returnPosition; // 나갈 때 되돌아갈 월드 위치 (입구 앞)
    private Quaternion returnRotation; // 나갈 때 되돌아갈 회전
    private bool isTransitioning; // 로딩/언로딩 도중 중복 입력 방지

    private Transform player;
    private Rigidbody playerRigidbody;
    private CinemachineVirtualCamera followCam;

    public bool IsInInterior { get { return currentInteriorScene != null; } }

    private void Awake() {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) {
            player = playerObj.transform;
            playerRigidbody = playerObj.GetComponent<Rigidbody>();
        }

        GameObject camObj = GameObject.Find("Follow Cam");
        if (camObj != null) {
            followCam = camObj.GetComponent<CinemachineVirtualCamera>();
        }
    }

    // 건물 입구 등에서 호출: interiorSceneName 내부 씬을 추가 로드하고 플레이어를 그 안으로 이동
    public void EnterInterior(string interiorSceneName, Vector3 doorReturnPosition, Quaternion doorReturnRotation) {
        if (isTransitioning || IsInInterior) return;

        // 전환 시작 시점에 안내 문구를 즉시 숨김 — 트리거를 보여준 오브젝트가 씬 전환으로
        // 파괴되면 OnTriggerExit이 호출되지 않아 문구가 그대로 남는 문제를 방지
        if (UIManager.instance != null) UIManager.instance.HideInteractPrompt();

        returnPosition = doorReturnPosition;
        returnRotation = doorReturnRotation;
        currentInteriorScene = interiorSceneName;
        StartCoroutine(LoadInteriorRoutine(interiorSceneName));
    }

    // 내부 씬의 출구 오브젝트에서 호출: 내부 씬을 언로드하고 플레이어를 입구 앞으로 복귀
    public void ExitInterior() {
        if (isTransitioning || !IsInInterior) return;

        if (UIManager.instance != null) UIManager.instance.HideInteractPrompt();

        string sceneName = currentInteriorScene;
        currentInteriorScene = null;
        StartCoroutine(UnloadInteriorRoutine(sceneName));
    }

    private IEnumerator LoadInteriorRoutine(string sceneName) {
        isTransitioning = true;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        yield return op;

        GameObject spawn = GameObject.Find(InteriorSpawnPointName);
        Vector3 spawnPos = spawn != null ? spawn.transform.position : player.position;
        Quaternion spawnRot = spawn != null ? spawn.transform.rotation : player.rotation;
        TeleportPlayer(spawnPos, spawnRot);

        isTransitioning = false;
    }

    private IEnumerator UnloadInteriorRoutine(string sceneName) {
        isTransitioning = true;

        // 언로드 전에 먼저 텔레포트: 언로드 도중 파괴되는 오브젝트 위에 플레이어가 남지 않도록 함
        TeleportPlayer(returnPosition, returnRotation);

        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid()) {
            AsyncOperation op = SceneManager.UnloadSceneAsync(scene);
            yield return op;
        }

        isTransitioning = false;
    }

    // 플레이어를 즉시 이동시키고, 시네머신 카메라가 부드럽게 따라오지 않고 즉시 전환되도록 알림
    private void TeleportPlayer(Vector3 pos, Quaternion rot) {
        Vector3 delta = pos - player.position;

        if (playerRigidbody != null) {
            playerRigidbody.position = pos;
            playerRigidbody.rotation = rot;
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        } else {
            player.SetPositionAndRotation(pos, rot);
        }

        if (followCam != null) {
            followCam.OnTargetObjectWarped(player, delta);
        }
    }
}
