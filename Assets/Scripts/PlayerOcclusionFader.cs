// 카메라와 플레이어 사이를 가리는 건물을 감지해 반투명하게 만드는 컴포넌트
using System.Collections.Generic;
using UnityEngine;

public class PlayerOcclusionFader : MonoBehaviour {
    public Transform player; // 감시할 대상(플레이어)
    public Camera cam; // 기준이 될 카메라 (비워두면 Camera.main)
    public LayerMask buildingLayerMask; // 건물 콜라이더가 속한 레이어
    [Range(0f, 1f)] public float fadedAlpha = 0.25f; // 가려졌을 때의 알파값
    public float fadeSpeed = 6f; // 페이드 속도(초당 알파 변화량)

    private class FadeState {
        public List<Material> materials = new List<Material>();
        public float currentAlpha = 1f;
    }

    private readonly Dictionary<GameObject, FadeState> states = new Dictionary<GameObject, FadeState>();
    private readonly HashSet<GameObject> occludingThisFrame = new HashSet<GameObject>();
    private readonly List<GameObject> pendingRemoval = new List<GameObject>();
    private RaycastHit[] hitBuffer = new RaycastHit[8];

    private void Start() {
        if (cam == null) cam = Camera.main;
    }

    private void Update() {
        if (cam == null || player == null) return;

        occludingThisFrame.Clear();

        Vector3 origin = cam.transform.position;
        Vector3 targetPos = player.position + Vector3.up * 1f;
        Vector3 toTarget = targetPos - origin;
        float dist = toTarget.magnitude;

        if (dist > 0.01f) {
            int hitCount = Physics.RaycastNonAlloc(origin, toTarget / dist, hitBuffer, dist, buildingLayerMask);
            for (int i = 0; i < hitCount; i++) {
                GameObject root = hitBuffer[i].collider.transform.parent != null
                    ? hitBuffer[i].collider.transform.parent.gameObject
                    : hitBuffer[i].collider.gameObject;
                occludingThisFrame.Add(root);
                if (!states.ContainsKey(root)) {
                    states[root] = BuildFadeState(root);
                }
            }
        }

        pendingRemoval.Clear();
        foreach (var kvp in states) {
            GameObject root = kvp.Key;
            FadeState state = kvp.Value;
            bool occluding = occludingThisFrame.Contains(root);
            float target = occluding ? fadedAlpha : 1f;
            state.currentAlpha = Mathf.MoveTowards(state.currentAlpha, target, fadeSpeed * Time.deltaTime);
            ApplyAlpha(state);

            if (!occluding && Mathf.Approximately(state.currentAlpha, 1f)) {
                pendingRemoval.Add(root);
            }
        }
        for (int i = 0; i < pendingRemoval.Count; i++) {
            states.Remove(pendingRemoval[i]);
        }
    }

    // 건물 루트 하위의 모든 렌더러 머티리얼을 인스턴스화하고 투명 렌더링 모드로 전환
    private FadeState BuildFadeState(GameObject root) {
        FadeState state = new FadeState();
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++) {
            Material mat = renderers[i].material; // 공유 에셋이 아닌 인스턴스를 생성/반환
            SetTransparent(mat);
            state.materials.Add(mat);
        }
        state.currentAlpha = 1f;
        return state;
    }

    private void ApplyAlpha(FadeState state) {
        for (int i = 0; i < state.materials.Count; i++) {
            Material mat = state.materials[i];
            Color c = mat.GetColor("_BaseColor");
            c.a = state.currentAlpha;
            mat.SetColor("_BaseColor", c);
        }
    }

    // URP Simple Lit/Lit 계열 머티리얼을 런타임에 Opaque -> Transparent 렌더 모드로 전환
    private static void SetTransparent(Material mat) {
        mat.SetFloat("_Surface", 1f);
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }
}
