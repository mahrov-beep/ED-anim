#if MAP_PROFILE
namespace _Project.Scripts.Minimap {
    using System.Collections.Generic;
    using UnityEngine;

    public static class MapViewProfiler {
        public static bool Enabled = true;

        public static double ClearMarkersAvg;
        public static double DrawWaypointAvg;
        public static double DrawExitAvg;
        public static double DrawInterestAvg;
        public static double DrawItemBoxAvg;
        public static double DrawEnemyAvg;
        public static double DrawPartyAvg;
        public static double DrawGrenadeAvg;
        public static double UpdateZoomAvg;
        public static double UpdatePositionAvg;
        public static double UpdateRotationAvg;
        public static double TotalFrameAvg;

        public static int WaypointDotsCount;
        public static int ExitIconCount;
        public static int ExitPointerCount;
        public static int InterestIconCount;
        public static int InterestPointerCount;
        public static int SpawnedItemBoxCount;
        public static int DroppedItemBoxCount;
        public static int EnemyCount;
        public static int PartyCount;
        public static int GrenadeCount;
        public static int ActiveIconsCount;

        private static int frameCount;
        private const int AVERAGE_WINDOW = 60;
        private const int BUFFER_SIZE = 2000;

        private static readonly Queue<double>[] metricBuffers = new Queue<double>[12];
        private static readonly double[] minValues = new double[12];
        private static readonly double[] maxValues = new double[12];
        private static bool buffersInitialized = false;

        private static void InitializeBuffers() {
            if (buffersInitialized) return;
            for (int i = 0; i < metricBuffers.Length; i++) {
                metricBuffers[i] = new Queue<double>(BUFFER_SIZE);
                minValues[i] = double.MaxValue;
                maxValues[i] = double.MinValue;
            }
            buffersInitialized = true;
        }

        public static void UpdateAverage(ref double avg, double newValue) {
            if (!Enabled) return;
            if (frameCount < AVERAGE_WINDOW) {
                avg = (avg * frameCount + newValue) / (frameCount + 1);
            } else {
                avg = avg * 0.98 + newValue * 0.02;
            }
        }

        public static void RecordMetric(int index, double value) {
            if (!Enabled) return;
            InitializeBuffers();

            var buffer = metricBuffers[index];
            if (buffer.Count >= BUFFER_SIZE) {
                buffer.Dequeue();
            }
            buffer.Enqueue(value);

            minValues[index] = System.Math.Min(minValues[index], value);
            maxValues[index] = System.Math.Max(maxValues[index], value);
        }

        public static void IncrementFrame() {
            if (!Enabled) return;
            if (frameCount < AVERAGE_WINDOW) frameCount++;
        }

        public static void ResetCounters() {
            if (!Enabled) return;
            WaypointDotsCount = 0;
            ExitIconCount = 0;
            ExitPointerCount = 0;
            InterestIconCount = 0;
            InterestPointerCount = 0;
            SpawnedItemBoxCount = 0;
            DroppedItemBoxCount = 0;
            EnemyCount = 0;
            PartyCount = 0;
            GrenadeCount = 0;
            ActiveIconsCount = 0;
        }

        public static void LogStats() {
            if (!Enabled) return;
            UnityEngine.Debug.Log($"[MapView Performance] Total: {TotalFrameAvg:F3}ms | " +
                $"Clear: {ClearMarkersAvg:F3}ms [{ActiveIconsCount}] | " +
                $"Waypoint: {DrawWaypointAvg:F3}ms [{WaypointDotsCount}] | " +
                $"Exit: {DrawExitAvg:F3}ms [{ExitIconCount}+{ExitPointerCount}] | " +
                $"Interest: {DrawInterestAvg:F3}ms [{InterestIconCount}+{InterestPointerCount}] | " +
                $"ItemBox: {DrawItemBoxAvg:F3}ms [{SpawnedItemBoxCount}+{DroppedItemBoxCount}] | " +
                $"Enemy: {DrawEnemyAvg:F3}ms [{EnemyCount}] | " +
                $"Party: {DrawPartyAvg:F3}ms [{PartyCount}] | " +
                $"Grenade: {DrawGrenadeAvg:F3}ms [{GrenadeCount}]");
        }

        private static double CalculateMedian(Queue<double> buffer) {
            if (buffer.Count == 0) return 0;
            var sorted = new List<double>(buffer);
            sorted.Sort();
            int mid = sorted.Count / 2;
            if (sorted.Count % 2 == 0) {
                return (sorted[mid - 1] + sorted[mid]) / 2.0;
            }
            return sorted[mid];
        }

        public static void ExportToCSV() {
            if (!Enabled) return;
            InitializeBuffers();

            var timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var path = System.IO.Path.Combine(Application.persistentDataPath, "MapProfiler");

            if (!System.IO.Directory.Exists(path)) {
                System.IO.Directory.CreateDirectory(path);
            }

            var filePath = System.IO.Path.Combine(path, $"mapstats_{timestamp}.csv");

            try {
                using (var writer = new System.IO.StreamWriter(filePath)) {
                    writer.WriteLine("Metric,Average(ms),Min(ms),Max(ms),Median(ms),Samples");

                    var metricNames = new[] {
                        "ClearMarkers", "DrawWaypoint", "DrawExit", "DrawInterest",
                        "DrawItemBox", "DrawEnemy", "DrawParty", "DrawGrenade",
                        "UpdateZoom", "UpdatePosition", "UpdateRotation", "TotalFrame"
                    };

                    var averages = new[] {
                        ClearMarkersAvg, DrawWaypointAvg, DrawExitAvg, DrawInterestAvg,
                        DrawItemBoxAvg, DrawEnemyAvg, DrawPartyAvg, DrawGrenadeAvg,
                        UpdateZoomAvg, UpdatePositionAvg, UpdateRotationAvg, TotalFrameAvg
                    };

                    for (int i = 0; i < metricNames.Length; i++) {
                        var median = CalculateMedian(metricBuffers[i]);
                        var samples = metricBuffers[i].Count;
                        writer.WriteLine($"{metricNames[i]},{averages[i]:F6},{minValues[i]:F6},{maxValues[i]:F6},{median:F6},{samples}");
                    }
                }

                UnityEngine.Debug.Log($"[MapView Profiler] Statistics exported to: {filePath}");
            } catch (System.Exception ex) {
                UnityEngine.Debug.LogError($"[MapView Profiler] Failed to export statistics: {ex.Message}");
            }
        }
    }
}
#endif
