using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

#if UNITY_EDITOR
public class DebugStatsOverlay : MonoBehaviour
{
    private float deltaTime;
    private GUIStyle style;

    void Awake()
    {
        style = new GUIStyle();
        style.fontSize = 10;                                  // 작은 폰트
        style.normal.textColor = new Color(0f, 1f, 0.2f);     // 초록색
        style.richText = false;
    }

    void OnGUI()
    {
        // FPS 계산
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1f / deltaTime;

        // CPU/GPU 타이밍
        FrameTimingManager.CaptureFrameTimings();
        FrameTiming[] frames = new FrameTiming[1];
        FrameTimingManager.GetLatestTimings(1, frames);

        float cpu = (float)(frames.Length > 0 ? frames[0].cpuFrameTime : 0f);
        float gpu = (float)(frames.Length > 0 ? frames[0].gpuFrameTime : 0f);

        // 세로 정렬 시작
        GUILayout.BeginArea(new Rect(10, 10, 300, 400));

        GUILayout.Label($"FPS: {fps:F1}", style);
        GUILayout.Space(-2);

        GUILayout.Label($"CPU: {cpu:F2}ms", style);
        GUILayout.Space(-2);

        GUILayout.Label($"GPU: {gpu:F2}ms", style);
        GUILayout.Space(-2);

        GUILayout.Label($"Draw: {UnityStats.drawCalls}", style);
        GUILayout.Space(-2);

        GUILayout.Label($"Batch: {UnityStats.batches}", style);
        GUILayout.Space(-2);

        GUILayout.Label($"Pass: {UnityStats.setPassCalls}", style);
        GUILayout.Space(-2);

        GUILayout.Label($"Tris: {UnityStats.triangles}", style);
        GUILayout.Space(-2);

        GUILayout.Label($"Verts: {UnityStats.vertices}", style);
        GUILayout.Space(-2);

        GUILayout.Label($"Alloc: {Profiler.GetTotalAllocatedMemoryLong() / 1048576f:F1}MB", style);
        GUILayout.Space(-2);

        GUILayout.Label($"Mono: {Profiler.GetMonoHeapSizeLong() / 1048576f:F1}MB", style);

        GUILayout.EndArea();
    }
}
#endif