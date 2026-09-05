using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 좀비 게임 오브젝트를 주기적으로 생성
public class ZombieSpawner : MonoBehaviour {
    public Zombie zombiePrefab; // 생성할 좀비 원본 프리팹

    public ZombieData[] zombieDatas; // 사용할 좀비 셋업 데이터들
    public Transform[] spawnPoints; // 좀비 AI를 소환할 위치들

    public int hordeBurstSize = 50; // 큰 소리 한 번에 스폰되는 좀비 무리 최소 규모
    public int maxZombiesAlive = 200; // 동시에 존재할 수 있는 좀비 최대 수
    public int hordeSpawnPerFrame = 12; // 한 프레임에 생성할 좀비 수(한번에 다 만들면 순간 버벅임이 생길 수 있어 나눠서 생성)
    public float offscreenMargin = 0.08f; // 화면 밖으로 판정할 뷰포트 여유값

    private List<Zombie> zombies = new List<Zombie>(); // 생성된 좀비들을 담는 리스트
    private int wave; // 현재 웨이브

    private DayNightCycle dayNightCycle; // 낮/밤 상태를 확인할 대상
    private bool wasNight; // 직전 프레임의 밤 여부 (낮->밤/밤->낮 전환 감지용)

    private Transform player; // 호드가 몰려갈 대상
    private LivingEntity playerLivingEntity; // Zombie.ForceTarget에 넘길 대상
    private Camera mainCamera; // 화면 밖 스폰 위치 계산용

    private void Start() {
        dayNightCycle = FindObjectOfType<DayNightCycle>();
        wasNight = IsNight();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerLivingEntity = playerObj.GetComponent<LivingEntity>();
        }

        mainCamera = Camera.main;
    }

    private void OnEnable() {
        // 총소리 등 큰 소리가 나면 화면 밖에서 좀비 무리를 스폰
        NoiseManager.OnNoiseEmitted += OnNoiseHeard;
    }

    private void OnDisable() {
        NoiseManager.OnNoiseEmitted -= OnNoiseHeard;
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

    // 큰 소리(총소리 등)를 들었을 때 화면 밖에서 좀비 무리를 몰려오게 함
    private void OnNoiseHeard(Vector3 position, float radius) {
        // 낮이거나 게임오버 상태면 무리를 소환하지 않음
        if (!IsNight()) return;
        if (GameManager.instance != null && GameManager.instance.isGameover) return;
        if (player == null || mainCamera == null) return;

        // 이미 최대치에 도달했다면 더 소환하지 않음, 아니라면 남은 여유만큼만 소환(최소 hordeBurstSize, 상한 maxZombiesAlive)
        int remainingCapacity = maxZombiesAlive - zombies.Count;
        if (remainingCapacity <= 0) return;

        int burst = Mathf.Min(hordeBurstSize, remainingCapacity);
        StartCoroutine(SpawnHordeRoutine(burst));
    }

    // 좀비 무리를 화면 밖 위치에 원형으로 분산 배치하며, 한 프레임에 너무 많이 만들지 않도록 나눠서 생성
    private IEnumerator SpawnHordeRoutine(int count) {
        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep + Random.Range(-angleStep * 0.35f, angleStep * 0.35f);
            Vector3 spawnPos = GetOffscreenSpawnPosition(angle);
            CreateHordeZombieAt(spawnPos);

            if ((i + 1) % hordeSpawnPerFrame == 0)
            {
                yield return null;
            }
        }
    }

    // 플레이어를 중심으로 angle 방향, 현재 카메라 화면 밖에 걸리지 않는 가장 가까운 지점을 찾음
    private Vector3 GetOffscreenSpawnPosition(float angleDeg) {
        Vector3 center = player.position;
        Vector3 dir = Quaternion.Euler(0f, angleDeg, 0f) * Vector3.forward;

        float dist = 12f;
        Vector3 candidate = center + dir * dist;

        for (int i = 0; i < 24; i++)
        {
            candidate = center + dir * dist;
            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(candidate + Vector3.up);
            bool offscreen = viewportPoint.z < 0f
                || viewportPoint.x < -offscreenMargin || viewportPoint.x > 1f + offscreenMargin
                || viewportPoint.y < -offscreenMargin || viewportPoint.y > 1f + offscreenMargin;

            if (offscreen) break;
            dist += 3f;
        }

        // 내비메시 위의 가장 가까운 유효 지점으로 보정 (건물 안/밖 지형 등에 스폰되지 않도록)
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(candidate, out hit, 15f, UnityEngine.AI.NavMesh.AllAreas))
        {
            return hit.position;
        }
        return candidate;
    }

    // 호드용 좀비 생성: 곧바로 플레이어를 추적 대상으로 지정해 화면 밖에서부터 바로 달려오게 함
    private void CreateHordeZombieAt(Vector3 position) {
        Zombie zombie = CreateZombieAt(position, Quaternion.identity);
        if (zombie != null && playerLivingEntity != null)
        {
            zombie.ForceTarget(playerLivingEntity);
        }
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

    // 좀비를 생성하고 생성한 좀비에게 추적할 대상을 할당 (기존 스폰 포인트 웨이브용)
    private void CreateZombie() {
        // 사용할 좀비 데이터 랜덤으로 결정
        ZombieData zombieData = zombieDatas[Random.Range(0, zombieDatas.Length)];

        // 생성할 위치를 랜덤으로 결정
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        CreateZombieAt(spawnPoint.position, spawnPoint.rotation, zombieData);
    }

    // 좀비를 지정된 위치에 생성하고 공통 후처리(리스트 등록, 사망 이벤트 연결)를 수행
    private Zombie CreateZombieAt(Vector3 position, Quaternion rotation, ZombieData zombieDataOverride = null) {
        ZombieData zombieData = zombieDataOverride != null
            ? zombieDataOverride
            : zombieDatas[Random.Range(0, zombieDatas.Length)];

        // 좀비 프리팹으로부터 좀비 생성
        Zombie zombie = Instantiate(zombiePrefab, position, rotation);

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

        return zombie;
    }
}
