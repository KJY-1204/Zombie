// 레이더 사용 중 좀비 미니맵 표시 시간을 관리하는 컴포넌트
using UnityEngine;

public class MinimapRadarController : MonoBehaviour {
    // 싱글톤 접근용 프로퍼티
    public static MinimapRadarController instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<MinimapRadarController>();
            }

            return m_instance;
        }
    }

    private static MinimapRadarController m_instance; // 싱글톤이 할당될 static 변수

    public float revealDuration = 10f; // 레이더 사용 시 좀비가 노출되는 시간(초)
    // 전체 지도 창은 "종이 지도" 컨셉으로 플레이어/좀비 위치를 표시하지 않으므로
    // 레이더는 미니맵에만 영향을 준다

    private Camera minimapCamera; // 좀비 레이어를 토글할 미니맵 카메라
    private int minimapZombieLayerMask; // MinimapZombie 레이어에 해당하는 비트마스크
    private bool revealing; // 현재 좀비가 노출 중인지 여부
    private float revealEndTime; // 노출이 종료되는 시각(Time.time 기준)

    private void Awake() {
        // 씬에 싱글톤이 된 다른 컨트롤러가 있다면 자신을 파괴
        if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        minimapCamera = GetComponent<Camera>();
        minimapZombieLayerMask = 1 << LayerMask.NameToLayer("MinimapZombie");
    }

    // 매프레임 노출 시간이 끝났는지 확인 (Time.timeScale에 영향받지 않도록 Time.time 사용)
    private void Update() {
        if (revealing && Time.time >= revealEndTime)
        {
            HideZombies();
        }
    }

    // 레이더를 발동시켜 좀비를 미니맵에 노출
    public void ActivateRadar() {
        // 재사용 시 처음부터 다시 카운트되도록 종료 시각만 갱신
        revealEndTime = Time.time + revealDuration;

        if (!revealing)
        {
            ShowZombies();
        }
    }

    // 미니맵 카메라 컬링 마스크에 MinimapZombie 레이어를 포함시켜 좀비를 노출
    private void ShowZombies() {
        revealing = true;
        minimapCamera.cullingMask |= minimapZombieLayerMask;
    }

    // 미니맵 카메라 컬링 마스크에서 MinimapZombie 레이어를 제외해 좀비를 숨김
    private void HideZombies() {
        revealing = false;
        minimapCamera.cullingMask &= ~minimapZombieLayerMask;
    }
}
