using System.Collections.Generic;
using UnityEngine;

// 좀비 게임 오브젝트를 주기적으로 생성
public class ZombieSpawner : MonoBehaviour {
    public Zombie zombiePrefab; // 생성할 좀비 원본 프리팹

    public ZombieData[] zombieDatas; // 사용할 좀비 셋업 데이터들
    public Transform[] spawnPoints; // 좀비 AI를 소환할 위치들

    private List<Zombie> zombies = new List<Zombie>(); // 생성된 좀비들을 담는 리스트
    private int wave; // 현재 웨이브

    private DayNightCycle dayNightCycle; // 낮/밤 상태를 확인할 대상
    private bool wasNight; // 직전 프레임의 밤 여부 (낮->밤/밤->낮 전환 감지용)

    private void Start() {
        dayNightCycle = FindObjectOfType<DayNightCycle>();
        wasNight = IsNight();
    }

    // 밤인지 여부. DayNightCycle을 찾을 수 없으면 기존 동작(항상 스폰)을 유지하기 위해 true로 취급
    private bool IsNight() {
        return dayNightCycle == null || dayNightCycle.IsNight;
    }

    private void Update() {
        // 게임 오버 상태일때는 생성하지 않음
        if (GameManager.instance != null && GameManager.instance.isGameover)
        {
            return;
        }

        bool isNight = IsNight();

        // 낮이 막 시작된 시점이라면 남아있는 좀비를 전부 정리 (아침에는 좀비가 없어야 함)
        if (wasNight && !isNight)
        {
            ClearAllZombies();
        }
        wasNight = isNight;

        // 밤에만 웨이브를 스폰
        if (isNight && zombies.Count <= 0)
        {
            SpawnWave();
        }

        // UI 갱신
        UpdateUI();
    }

    // 낮이 시작될 때 남아있는 좀비를 점수/이펙트 없이 즉시 제거
    private void ClearAllZombies() {
        for (int i = zombies.Count - 1; i >= 0; i--)
        {
            if (zombies[i] != null)
            {
                Destroy(zombies[i].gameObject);
            }
        }
        zombies.Clear();
    }

    // 웨이브 정보를 UI로 표시
    private void UpdateUI() {
        // 현재 웨이브와 남은 적 수 표시
        UIManager.instance.UpdateWaveText(wave, zombies.Count);
    }

    // 현재 웨이브에 맞춰 좀비들을 생성
    private void SpawnWave() {
        // 웨이브 1 증가
        wave++;

        // 현재 웨이브 * 1.5에 반올림 한 개수 만큼 좀비를 생성
        int spawnCount = Mathf.RoundToInt(wave * 1.5f);

        // spawnCount 만큼 좀비를 생성
        for (int i = 0; i < spawnCount; i++)
        {
            // 좀비 생성 처리 실행
            CreateZombie();
        }
    }

    // 좀비를 생성하고 생성한 좀비에게 추적할 대상을 할당
    private void CreateZombie() {
        // 사용할 좀비 데이터 랜덤으로 결정
        ZombieData zombieData = zombieDatas[Random.Range(0, zombieDatas.Length)];
        
        // 생성할 위치를 랜덤으로 결정
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // 좀비 프리팹으로부터 좀비 생성
        Zombie zombie = Instantiate(zombiePrefab, spawnPoint.position, spawnPoint.rotation);

        // 생성한 좀비의 능력치 설정
        zombie.Setup(zombieData);

        // 생성된 좀비를 리스트에 추가
        zombies.Add(zombie);

        // 좀비의 onDeath 이벤트에 익명 메서드 등록
        // 사망한 좀비를 리스트에서 제거
        zombie.onDeath += () => zombies.Remove(zombie);
        // 사망한 좀비를 10 초 뒤에 파괴
        zombie.onDeath += () => Destroy(zombie.gameObject, 10f);
        // 좀비 사망시 점수 상승
        zombie.onDeath += () => GameManager.instance.AddScore(100);
    }
}