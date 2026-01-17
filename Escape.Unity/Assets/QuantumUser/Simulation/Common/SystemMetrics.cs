using System.Collections.Generic;
using System.Diagnostics;

public static class SystemMetrics {
  public struct MetricData {
    public long   TotalTicks;
    public int    UpdateCount;
    public double MinMs;
    public double MaxMs;
    public double LastMs;

    public double AverageMs => UpdateCount > 0
      ? (double)TotalTicks / UpdateCount / Stopwatch.Frequency * 1000
      : 0;
  }

  static readonly Dictionary<string, MetricData> Metrics     = new();
  static readonly Dictionary<string, Stopwatch>  Stopwatches = new();

  public static IReadOnlyDictionary<string, MetricData> All => Metrics;

  [Conditional("UNITY_EDITOR")]
  [Conditional("DEVELOPMENT_BUILD")]
  public static void Begin(string name) {
    if (!Stopwatches.TryGetValue(name, out var sw)) {
      sw = new Stopwatch();
      Stopwatches[name] = sw;
    }
    sw.Restart();
  }

  [Conditional("UNITY_EDITOR")]
  [Conditional("DEVELOPMENT_BUILD")]
  public static void End(string name) {
    if (!Stopwatches.TryGetValue(name, out var sw)) {
      return;
    }

    sw.Stop();
    var ms = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1000;

    if (!Metrics.TryGetValue(name, out var data)) {
      data = new MetricData { MinMs = double.MaxValue };
    }

    data.TotalTicks += sw.ElapsedTicks;
    data.UpdateCount++;
    data.LastMs   = ms;
    data.MinMs    = System.Math.Min(data.MinMs, ms);
    data.MaxMs    = System.Math.Max(data.MaxMs, ms);
    Metrics[name] = data;
  }

  public static void Reset() {
    Metrics.Clear();
  }
}