namespace Multicast.Diagnostics {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using JetBrains.Annotations;
    using UnityEngine;
    using UnityEngine.Pool;
    using Debug = UnityEngine.Debug;

    public struct DebugTimer : IDisposable {
        private static readonly ObjectPool<Stopwatch> Stopwatches =
            new(() => new Stopwatch(), actionOnGet: sw => sw.Start(), actionOnRelease: sw => sw.Reset());

        private string    category;
        private string    name;
        private Stopwatch sw;

        public TimeSpan Elapsed => this.sw?.Elapsed ?? TimeSpan.Zero;

        [MustDisposeResource]
        public static DebugTimer Create(string category, string name) {
            return new DebugTimer {
                category = category,
                name     = name,
                sw       = Stopwatches.Get(),
            };
        }

        public void Pause() {
            this.sw?.Stop();
        }

        public void Resume() {
            this.sw?.Start();
        }

        public void Dispose() {
            this.Dispose(logResults: true);
        }

        public void Dispose(bool logResults) {
            if (this.sw == null) {
                return;
            }

            if (logResults) {
                if (Debug.unityLogger.IsLogTypeAllowed(LogType.Log)) {
                    Debug.Log($"[DT] {this.category}/{this.name} done in {this.sw.ElapsedMilliseconds} ms");
                }

                this.SendResultToAnalytics();
            }

            Stopwatches.Release(this.sw);
            this.sw = null;
        }

        private void SendResultToAnalytics() {
            try {
                CoreAnalytics.ReportEvent("debug_timer", new Dictionary<string, object> {
                    [this.category] = this.name,
                    ["seconds"]     = (long) this.sw.Elapsed.TotalSeconds,
                    ["millis"]      = this.sw.ElapsedMilliseconds,
                });
            }
            catch {
                //
            }
        }
    }
}