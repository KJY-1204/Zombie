// 밤에는 켜지고 낮에는 꺼지는 가로등 램프
using UnityEngine;

public class StreetLight : MonoBehaviour {
    public Light lamp; // 가로등의 실제 광원
    public MeshRenderer bulbRenderer; // 램프 헤드(전구) 렌더러 - 켜졌을 때 발광 색으로 바뀜
    public Color onEmission = new Color(1f, 0.85f, 0.5f);
    public Color offColor = new Color(0.15f, 0.14f, 0.12f);

    private DayNightCycle dayNight;
    private bool isOn;

    private void Start() {
        dayNight = FindObjectOfType<DayNightCycle>();
        SetOn(false);
    }

    private void Update() {
        if (dayNight == null) return;

        bool shouldBeOn = dayNight.IsNight;
        if (shouldBeOn != isOn) {
            SetOn(shouldBeOn);
        }
    }

    private void SetOn(bool on) {
        isOn = on;
        if (lamp != null) lamp.enabled = on;
        if (bulbRenderer != null) {
            // material(공유 아님)로 접근해 인스턴스화된 개별 머티리얼만 바꿈 — 다른 가로등에 영향 없음
            bulbRenderer.material.SetColor("_BaseColor", on ? onEmission : offColor);
        }
    }
}
