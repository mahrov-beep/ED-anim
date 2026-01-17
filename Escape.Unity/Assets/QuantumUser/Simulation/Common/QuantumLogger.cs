namespace Quantum {
  using System;
  using System.Diagnostics;
  using Core;
  using Debug = UnityEngine.Debug;

  public static class QuantumLogger {
    public static void LogTrace(this BTParams p, object message) {
      var f = p.FrameThreadSafe;
      var e = p.Entity;

      if (f.IsVerified) {
        // Debug.Log($"[AI]({f.Number}) {e}: {message} ");
      }
    }
    
    [Conditional("DEBUG")]
    public static void LogDebug(this BTParams p, object message) {
      var f = p.FrameThreadSafe;
      var e = p.Entity;

      if (f.IsVerified) {
        Debug.Log($"[AI]({f.Number}) {e}: {message} ");
      }
    }

    public static void LogWarning(this BTParams p, object message) {
      var f = p.FrameThreadSafe;
      var e = p.Entity;

      if (f.IsVerified) {
        Debug.LogWarning($"[AI]({f.Number}) {e}: {message} ");
      }
    }

    public static void LogError(this BTParams p, object message) {
      var f = p.FrameThreadSafe;
      var e = p.Entity;

      if (f.IsVerified) {
        Debug.LogError($"[AI]({f.Number}) {e}: {message} ");
      }
    }
    
    public static void LogTrace(this FrameBase f, EntityRef e, object message) {
      if (f.IsVerified) {
        Debug.Log($"[Q]({f.Number}) {e}: {message} ");
      }
    }

    public static void LogDebug(this FrameBase f, EntityRef e, object message) {
      if (f.IsVerified) {
        Debug.Log($"[Q]({f.Number}) {e}: {message} ");
      }
    }

    public static void LogWarning(this FrameBase f, EntityRef e, object message) {
      if (f.IsVerified) {
        Debug.LogWarning($"[Q]({f.Number}) {e}: {message} ");
      }
    }

    public static void LogError(this FrameBase f, EntityRef e, object message) {
      if (f.IsVerified) {
        Debug.LogError($"[Q]({f.Number}) {e}: {message} ");
      }
    }

    public static void LogError<T>(this FrameBase f, EntityRef e, string message, Func<T, string> componentLogFormat)
            where T : unmanaged, IComponent {

      if (f.IsVerified) {
        if (componentLogFormat != null) {
          bool hasComponent = f.TryGet(e, out T component);

          message = hasComponent ? 
                  $"{message}. {typeof(T)}:{componentLogFormat(component)}" :
                  message;

          if (!hasComponent) {
            f.LogError(e, $"Broken component format {typeof(T)}");

            return;
          }
        }

        f.LogError(e, message);
      }
    }
  }
}