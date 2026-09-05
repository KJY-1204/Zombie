using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리자 관련 코드
using UnityEngine.UI; // UI 관련 코드

// 필요한 UI에 즉시 접근하고 변경할 수 있도록 허용하는 UI 매니저
public class UIManager : MonoBehaviour {
    // 싱글톤 접근용 프로퍼티
    public static UIManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<UIManager>();
            }

            return m_instance;
        }
    }

    private static UIManager m_instance; // 싱글톤이 할당될 변수

    public Text ammoText; // 탄약 표시용 텍스트
    public Text scoreText; // 점수 표시용 텍스트
    public Text waveText; // 적 웨이브 표시용 텍스트
    public Image[] inventorySlotIcons; // 인벤토리 슬롯별 아이콘 이미지
    public Text[] inventorySlotCounts; // 인벤토리 슬롯별 수량 텍스트
    public GameObject gameoverUI; // 게임 오버시 활성화할 UI

    public GameObject statusWindow; // I 키로 여닫는 상태 창
    public Text statusHealthText; // 상태 창의 체력 퍼센트 텍스트
    public Image statusHealthBarFill; // 상태 창의 세로 체력바 채움 이미지
    public Image statusSilhouette; // 상태 창의 신체 실루엣 이미지

    public Color healthyColor = new Color(0.85f, 0.85f, 0.9f, 1f); // 체력 100%일 때 색
    public Color criticalColor = new Color(0.9f, 0.15f, 0.15f, 1f); // 체력 0%일 때 색

    private PlayerInput playerInput; // 상태 창 토글 입력을 읽어올 컴포넌트
    private LivingEntity playerLivingEntity; // 체력을 조회할 대상

    private void Awake() {
        // 플레이어의 입력/체력 컴포넌트를 찾아 캐싱
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInput>();
            playerLivingEntity = player.GetComponent<LivingEntity>();
        }

        // 상태 창은 기본적으로 닫혀있음
        statusWindow.SetActive(false);
    }

    private void Update() {
        // I 키 입력 시 상태 창 열기/닫기 토글
        if (playerInput != null && playerInput.toggleStatus)
        {
            statusWindow.SetActive(!statusWindow.activeSelf);
        }

        // 상태 창이 열려있는 동안에만 내용을 갱신
        if (statusWindow.activeSelf && playerLivingEntity != null)
        {
            float ratio = playerLivingEntity.startingHealth > 0
                ? playerLivingEntity.health / playerLivingEntity.startingHealth : 0f;
            ratio = Mathf.Clamp01(ratio);

            statusHealthText.text = "체력 " + Mathf.RoundToInt(ratio * 100f) + "%";
            statusHealthBarFill.fillAmount = ratio;

            // 체력이 낮을수록 체력바와 실루엣을 붉게 물들임
            Color tint = Color.Lerp(criticalColor, healthyColor, ratio);
            statusHealthBarFill.color = tint;
            statusSilhouette.color = tint;
        }
    }

    // 탄약 텍스트 갱신
    public void UpdateAmmoText(int magAmmo, int remainAmmo) {
        ammoText.text = magAmmo + "/" + remainAmmo;
    }

    // 인벤토리 슬롯 하나의 아이콘/수량 갱신. icon이 없으면 빈 슬롯으로 표시
    public void UpdateInventorySlot(int index, Sprite icon, int quantity) {
        if (index < 0 || index >= inventorySlotIcons.Length) return;

        inventorySlotIcons[index].sprite = icon;
        inventorySlotIcons[index].enabled = icon != null;
        inventorySlotCounts[index].text = quantity > 0 ? quantity.ToString() : "";
    }

    // 점수 텍스트 갱신
    public void UpdateScoreText(int newScore) {
        scoreText.text = "Score : " + newScore;
    }

    // 적 웨이브 텍스트 갱신
    public void UpdateWaveText(int waves, int count) {
        waveText.text = "Wave : " + waves + "\nEnemy Left : " + count;
    }

    // 게임 오버 UI 활성화
    public void SetActiveGameoverUI(bool active) {
        gameoverUI.SetActive(active);
    }

    // 게임 재시작
    public void GameRestart() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}