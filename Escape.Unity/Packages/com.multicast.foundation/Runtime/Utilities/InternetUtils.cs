namespace Multicast.Utilities {
    using System.Diagnostics;
    using Cysharp.Threading.Tasks;
    using Diagnostics;
    using UnityEngine;

    public static class InternetUtils {
        public static async UniTask<bool> CheckForInternetConnectionAsync() {
            using var _ = DebugTimer.Create("internet_utils", "check_for_internet");

            var ping = new Ping("8.8.8.8");
            var time = Stopwatch.StartNew();

            while (!ping.isDone) {
                await UniTask.NextFrame();

                if (time.ElapsedMilliseconds > 5000) {
                    return false;
                }
            }

            return true;
        }
    }
}