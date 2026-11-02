using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;

// 최적화 전후 비교용 측정 도구.
// Play 중 지정한 프레임 수 동안 렌더 통계와 CPU 마커 시간을 모아 마크다운 표로 저장한다.
// 사용: PerfCapture.Begin("라벨", 300, "Docs/Perf/xx/result.md"); 끝나면 IsDone이 true가 된다.
public static class PerfCapture
{
    private class Metric
    {
        public string Label;
        public ProfilerCategory Category;
        public string Name;
        public bool IsTimeNs;
        public ProfilerRecorder Recorder;
        public List<double> Values = new List<double>();
    }

    private static readonly List<Metric> _metrics = new List<Metric>();
    private static readonly List<double> _frameTimesMs = new List<double>();

    private static readonly List<double> _gpuTimesMs = new List<double>();
    private static readonly FrameTiming[] _frameTimings = new FrameTiming[1];

    private static string _label;
    private static string _outputPath;
    private static int _targetFrames;
    private static int _lastFrame;
    private static int _warmupLeft;

    // 마지막 측정의 지표별 평균·중앙값 (A/B 요약 표에 쓴다)
    public static readonly Dictionary<string, double[]> LastStats = new Dictionary<string, double[]>();

    public static bool IsRunning { get; private set; }
    public static bool IsDone { get; private set; }
    public static string Summary { get; private set; } = "";

    public static void Begin(string label, int frames, string outputPath, int warmupFrames = 0)
    {
        if (IsRunning) Stop();

        _label = label;
        _targetFrames = frames;
        _outputPath = outputPath;
        _lastFrame = -1;
        _warmupLeft = warmupFrames;
        IsDone = false;
        Summary = "";
        _frameTimesMs.Clear();
        _gpuTimesMs.Clear();
        LastStats.Clear();
        _metrics.Clear();

        // 1. 렌더 통계 (개수)
        AddMetric("Draw Calls", ProfilerCategory.Render, "Draw Calls Count", false);
        AddMetric("Batches", ProfilerCategory.Render, "Batches Count", false);
        AddMetric("SetPass Calls", ProfilerCategory.Render, "SetPass Calls Count", false);
        AddMetric("Triangles", ProfilerCategory.Render, "Triangles Count", false);
        AddMetric("Vertices", ProfilerCategory.Render, "Vertices Count", false);
        AddMetric("Shadow Casters", ProfilerCategory.Render, "Shadow Casters Count", false);

        // 2. CPU 마커 (ns -> ms). 스크립트 개별 함수 마커는 Deep Profile이 있어야 해서 모아서 본다.
        AddMetric("메인 스레드", ProfilerCategory.Internal, "Main Thread", true);
        AddMetric("Update (모든 스크립트)", ProfilerCategory.Scripts, "Update.ScriptRunBehaviourUpdate", true);
        AddMetric("LateUpdate (모든 스크립트)", ProfilerCategory.Scripts, "PreLateUpdate.ScriptRunBehaviourLateUpdate", true);
        AddMetric("코루틴", ProfilerCategory.Scripts, "CoroutinesDelayedCalls", true);
        AddMetric("NavMesh 경로 탐색(Crowd.Pathfinding)", ProfilerCategory.Ai, "Crowd.Pathfinding", true);
        AddMetric("NavMesh 이동(Crowd.Movement)", ProfilerCategory.Ai, "Crowd.Movement", true);
        AddMetric("NavMesh 동기화(AIUpdatePostScript)", ProfilerCategory.Scripts, "PreLateUpdate.AIUpdatePostScript", true);
        AddMetric("애니메이터(Animators.Update)", ProfilerCategory.Animation, "Animators.Update", true);
        AddMetric("카메라 그리기(Camera.Render, 모든 카메라)", ProfilerCategory.Render, "Camera.Render", true);
        AddMetric("URP 그리기(DoRenderLoop_Internal)", ProfilerCategory.Render, "RenderPipelineManager.DoRenderLoop_Internal", true);
        AddMetric("GPU 대기(Gfx.WaitForPresentOnGfxThread)", ProfilerCategory.Render, "Gfx.WaitForPresentOnGfxThread", true);
        AddMetric("목표 FPS 대기(WaitForTargetFPS)", ProfilerCategory.Internal, "WaitForTargetFPS", true);
        AddMetric("UI 배치(Canvas.BuildBatch)", ProfilerCategory.Render, "Canvas.BuildBatch", true);

        // 3. 메모리
        AddMetric("GC 할당(바이트/프레임)", ProfilerCategory.Memory, "GC Allocated In Frame", false);

        IsRunning = true;
        EditorApplication.update += Tick;
    }

    public static void Stop()
    {
        EditorApplication.update -= Tick;
        foreach (Metric metric in _metrics)
        {
            if (metric.Recorder.Valid) metric.Recorder.Dispose();
        }

        IsRunning = false;
    }

    private static void AddMetric(string label, ProfilerCategory category, string name, bool isTimeNs)
    {
        // 마커 이름으로 실제 카테고리를 찾아 쓴다. (이름이 같아도 카테고리가 다르면 값이 안 잡힌다)
        category = FindCategory(name, category);
        Metric metric = new Metric { Label = label, Category = category, Name = name, IsTimeNs = isTimeNs };
        metric.Recorder = ProfilerRecorder.StartNew(category, name, 1);
        _metrics.Add(metric);
    }

    private static Dictionary<string, ProfilerCategory> _categoryByName;

    private static ProfilerCategory FindCategory(string name, ProfilerCategory fallback)
    {
        if (_categoryByName == null)
        {
            _categoryByName = new Dictionary<string, ProfilerCategory>();
            List<ProfilerRecorderHandle> handles = new List<ProfilerRecorderHandle>();
            ProfilerRecorderHandle.GetAvailable(handles);
            foreach (ProfilerRecorderHandle handle in handles)
            {
                ProfilerRecorderDescription description = ProfilerRecorderHandle.GetDescription(handle);
                if (!_categoryByName.ContainsKey(description.Name))
                {
                    _categoryByName[description.Name] = description.Category;
                }
            }
        }

        return _categoryByName.TryGetValue(name, out ProfilerCategory found) ? found : fallback;
    }

    private static void Tick()
    {
        if (!Application.isPlaying) { Finish("Play 모드가 끝나 중단됨"); return; }

        // 새 프레임일 때만 한 번 기록한다.
        int frame = Time.frameCount;
        if (frame == _lastFrame) return;
        _lastFrame = frame;

        // 설정을 바꾼 직후 몇 프레임은 값이 안정되지 않으므로 버린다.
        if (_warmupLeft > 0) { _warmupLeft--; return; }

        _frameTimesMs.Add(Time.unscaledDeltaTime * 1000.0);

        // GPU 프레임 시간 (지원하지 않으면 0)
        FrameTimingManager.CaptureFrameTimings();
        if (FrameTimingManager.GetLatestTimings(1, _frameTimings) > 0) _gpuTimesMs.Add(_frameTimings[0].gpuFrameTime);

        foreach (Metric metric in _metrics)
        {
            if (!metric.Recorder.Valid) continue;
            double value = metric.Recorder.LastValue;
            metric.Values.Add(metric.IsTimeNs ? value / 1_000_000.0 : value);
        }

        if (_frameTimesMs.Count >= _targetFrames) Finish("완료");
    }

    private static void Finish(string status)
    {
        Stop();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"## {_label}");
        sb.AppendLine();
        sb.AppendLine($"- 상태: {status}, 프레임 {_frameTimesMs.Count}개, 해상도 {Screen.width}x{Screen.height}, timeScale {Time.timeScale}");
        sb.AppendLine($"- 측정 시각: {DateTime.Now:yyyy-MM-dd HH:mm:ss}, Unity {Application.unityVersion}");
        sb.AppendLine();
        sb.AppendLine("| 지표 | 평균 | 중앙값 | 95% | 최대 |");
        sb.AppendLine("| --- | ---: | ---: | ---: | ---: |");
        AppendRow(sb, "프레임 시간(ms)", _frameTimesMs, true);
        if (_gpuTimesMs.Count > 0 && _gpuTimesMs.Exists(v => v > 0)) AppendRow(sb, "GPU 프레임 시간(ms, FrameTimingManager)", _gpuTimesMs, true);
        else sb.AppendLine("| GPU 프레임 시간(ms, FrameTimingManager) | 측정 불가 | | | |");
        foreach (Metric metric in _metrics)
        {
            string name = metric.IsTimeNs ? metric.Label + " (ms)" : metric.Label;
            if (!metric.Recorder.Valid && metric.Values.Count == 0)
            {
                sb.AppendLine($"| {name} | 측정 불가 | | | |");
                continue;
            }

            AppendRow(sb, name, metric.Values, metric.IsTimeNs);
        }

        Summary = sb.ToString();
        string fullPath = Path.GetFullPath(_outputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        File.WriteAllText(fullPath, Summary, new UTF8Encoding(false));
        IsDone = true;
    }

    // ---- A/B 실행기: 설정을 번갈아 바꾸며 같은 위치에서 여러 번 잰다 ----

    public class Step
    {
        public string Label;
        public int Frames = 100;
        public int Warmup = 8;
        public Action Setup;
    }

    private static List<Step> _queue;
    private static List<string> _queueLabels;
    private static List<Dictionary<string, double[]>> _queueStats;
    private static string _queueDir;
    private static string _queueTitle;
    private static int _queueIndex;
    private static bool _queueCollect;

    public static bool QueueDone { get; private set; } = true;

    public static void RunQueue(string title, List<Step> steps, string outputDir)
    {
        _queue = steps;
        _queueTitle = title;
        _queueDir = outputDir;
        _queueIndex = 0;
        _queueCollect = false;
        _queueLabels = new List<string>();
        _queueStats = new List<Dictionary<string, double[]>>();
        QueueDone = false;
        EditorApplication.update += QueueTick;
    }

    private static void QueueTick()
    {
        if (!Application.isPlaying) { QueueDone = true; EditorApplication.update -= QueueTick; return; }
        if (IsRunning) return;

        if (_queueCollect)
        {
            _queueLabels.Add(_queue[_queueIndex - 1].Label);
            _queueStats.Add(new Dictionary<string, double[]>(LastStats));
            _queueCollect = false;
        }

        if (_queueIndex >= _queue.Count)
        {
            WriteQueueSummary();
            QueueDone = true;
            EditorApplication.update -= QueueTick;
            return;
        }

        Step step = _queue[_queueIndex++];
        step.Setup?.Invoke();
        Begin(step.Label, step.Frames, Path.Combine(_queueDir, $"{_queueIndex:00}_{step.Label}.md"), step.Warmup);
        _queueCollect = true;
    }

    private static void WriteQueueSummary()
    {
        // 지표 이름은 첫 결과의 순서를 따른다.
        List<string> names = new List<string>();
        foreach (Dictionary<string, double[]> stats in _queueStats)
        {
            foreach (string key in stats.Keys) if (!names.Contains(key)) names.Add(key);
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"## {_queueTitle} (중앙값 / 평균)");
        sb.AppendLine();
        sb.Append("| 지표 |");
        foreach (string label in _queueLabels) sb.Append($" {label} |");
        sb.AppendLine();
        sb.Append("| --- |");
        foreach (string label in _queueLabels) sb.Append(" ---: |");
        sb.AppendLine();

        foreach (string name in names)
        {
            sb.Append($"| {name} |");
            foreach (Dictionary<string, double[]> stats in _queueStats)
            {
                if (!stats.TryGetValue(name, out double[] v)) { sb.Append(" - |"); continue; }
                bool small = name.Contains("(ms") || name.Contains("ms)");
                string format = small ? "F2" : "F0";
                sb.Append($" {v[1].ToString(format)} / {v[0].ToString(format)} |");
            }

            sb.AppendLine();
        }

        string fullPath = Path.GetFullPath(Path.Combine(_queueDir, "summary.md"));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        File.WriteAllText(fullPath, sb.ToString(), new UTF8Encoding(false));
    }

    private static void AppendRow(StringBuilder sb, string name, List<double> values, bool decimals)
    {
        if (values.Count == 0) { sb.AppendLine($"| {name} | 값 없음 | | | |"); return; }

        List<double> sorted = new List<double>(values);
        sorted.Sort();
        double sum = 0;
        foreach (double v in sorted) sum += v;

        string format = decimals ? "F2" : "F0";
        double mean = sum / sorted.Count;
        double median = sorted[sorted.Count / 2];
        LastStats[name] = new[] { mean, median };
        double p95 = sorted[Mathf.Min(sorted.Count - 1, (int)(sorted.Count * 0.95))];
        double max = sorted[sorted.Count - 1];
        sb.AppendLine($"| {name} | {mean.ToString(format)} | {median.ToString(format)} | {p95.ToString(format)} | {max.ToString(format)} |");
    }
}
