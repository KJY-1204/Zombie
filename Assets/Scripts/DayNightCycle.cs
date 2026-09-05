// 태양(Directional Light)을 회전시키며 낮/밤을 일정 주기로 반복시키는 컴포넌트
using UnityEngine;

public class DayNightCycle : MonoBehaviour {
    public float cycleDuration = 240f; // 낮과 밤 한 바퀴가 도는 데 걸리는 시간(초)
    public Light sun; // 회전시킬 태양 역할의 디렉셔널 라이트

    public Color dayLightColor = new Color(1f, 0.95f, 0.85f); // 낮의 태양광 색
    public float dayLightIntensity = 1.1f; // 낮의 태양광 세기
    public Color nightLightColor = new Color(0.23f, 0.277f, 0.774f); // 밤의 태양광 색(기존 무드 유지)
    public float nightLightIntensity = 0.25f; // 밤의 태양광 세기

    public Color dayAmbient = new Color(0.4f, 0.4f, 0.42f); // 낮의 환경광
    public Color nightAmbient = new Color(0.255f, 0.09f, 0.047f); // 밤의 환경광(기존 색 유지)

    // 다른 스크립트(가로등 등)가 현재 밤 정도를 읽을 수 있도록 공개
    public float DayAmount { get; private set; }
    public bool IsNight { get { return DayAmount < 0.35f; } }

    private float elapsed;

    private void Start() {
        // 해질녘 부근에서 시작해 바로 낮/밤 변화를 체감할 수 있도록 함
        elapsed = cycleDuration * 0.15f;
    }

    private void Update() {
        elapsed += Time.deltaTime;
        float t = (elapsed % cycleDuration) / cycleDuration;
        float angle = t * 360f;

        // 기존 조명의 방위각(Y=330)은 유지하고, 태양 고도(X)만 하루 주기로 순환
        sun.transform.rotation = Quaternion.Euler(angle - 90f, 330f, 0f);

        // 태양이 지평선 위에 있는 절반 구간만 "낮"으로 취급, 부드럽게 보간
        float rawDay = Mathf.Clamp01(Mathf.Sin(angle * Mathf.Deg2Rad));
        DayAmount = rawDay * rawDay * (3f - 2f * rawDay); // smoothstep

        sun.color = Color.Lerp(nightLightColor, dayLightColor, DayAmount);
        sun.intensity = Mathf.Lerp(nightLightIntensity, dayLightIntensity, DayAmount);
        RenderSettings.ambientLight = Color.Lerp(nightAmbient, dayAmbient, DayAmount);
    }
}
