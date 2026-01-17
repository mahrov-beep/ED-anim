using Multicast;
using UnityEngine;

public class MulticastLogProxy {
    [RuntimeInitializeOnLoadMethod]
    public static void Setup() {
        MulticastLog.ErrorLogCallback = LogError;
        MulticastLog.DebugLogCallback = LogDebug;
    }

    private static void LogError(string tag, string message) {
        Debug.unityLogger.LogError(tag, message);
    }

    private static void LogDebug(string tag, string message) {
        Debug.unityLogger.Log(tag, message);
    }
}