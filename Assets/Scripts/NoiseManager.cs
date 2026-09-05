// 총소리 등 큰 소리가 발생했을 때 이를 좀비들에게 알리는 정적 이벤트 허브
using System;
using UnityEngine;

public static class NoiseManager {
    // 소리 발생 시 (발생 위치, 들리는 반경)을 전달하는 이벤트
    public static event Action<Vector3, float> OnNoiseEmitted;

    public static void EmitNoise(Vector3 position, float radius) {
        OnNoiseEmitted?.Invoke(position, radius);
    }
}
