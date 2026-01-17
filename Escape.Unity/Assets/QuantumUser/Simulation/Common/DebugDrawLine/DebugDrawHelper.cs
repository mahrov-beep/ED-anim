namespace Quantum {
  using System.Diagnostics;
  using Photon.Deterministic;
  public static unsafe class DebugDrawHelper {
    [Conditional("UNITY_EDITOR")]
    public static void DrawLine(Frame f,
            FPVector3 start,
            FPVector3 end,
            ColorRGBA color,
            FP durationSec) {

      var e = f.Create();
      ObjectLifetime.Set(f, e, durationSec);
      
      var drawGizmo = new DebugGizmo { Color = color };
      var line      = drawGizmo.Data.LineData;
      line->Start = start;
      line->End   = end;

      f.Set(e, drawGizmo);
    }

    [Conditional("UNITY_EDITOR")]
    public static void DrawLine(Frame f,
            Transform3D* start,
            Transform3D* end,
            ColorRGBA color,
            FP durationSec) {
      
      DrawLine(f, start->Position, end->Position, color, durationSec);
    }

    [Conditional("UNITY_EDITOR")]
    public static void DrawRay(Frame f,
            FPVector3 origin,
            FPVector2 directionXZ,
            ColorRGBA color,
            FP durationSec) {

      var e = f.Create();
      ObjectLifetime.Set(f, e, durationSec);

      var drawGizmo = new DebugGizmo { Color = color };
      var ray       = drawGizmo.Data.RayData;
      ray->Origin    = origin;
      ray->Direction = directionXZ.XOY;

      f.Set(e, drawGizmo);
    }

    [Conditional("UNITY_EDITOR")]
    public static void DrawCircle(Frame f,
            FPVector3 center,
            FP radius,
            FPQuaternion rotation,
            ColorRGBA color,
            FP durationSec,
            bool wire = false) {

      var e = f.Create();
      ObjectLifetime.Set(f, e, durationSec);

      var drawGizmo = new DebugGizmo { Color = color };
      var circle    = drawGizmo.Data.CircleData;
      circle->Center   = center;
      circle->Radius   = radius;
      circle->Rotation = rotation;
      circle->Wire     = wire;

      f.Set(e, drawGizmo);
    }

    [Conditional("UNITY_EDITOR")]
    public static void DrawSector(Frame f,
            FPVector3 center,
            FPVector3 forward,
            FP radius,
            FP angleInDegrees,
            ColorRGBA color,
            FP durationSec,
            int segments = 8) {

      var halfAngle = angleInDegrees / 2;
      var up = FPVector3.Up;

      var leftDir = FPQuaternion.AngleAxis(-halfAngle, up) * forward;
      var leftEnd = center + leftDir * radius;
      DrawLine(f, center, leftEnd, color, durationSec);

      var rightDir = FPQuaternion.AngleAxis(halfAngle, up) * forward;
      var rightEnd = center + rightDir * radius;
      DrawLine(f, center, rightEnd, color, durationSec);

      var angleStep = angleInDegrees / segments;
      var prevPoint = leftEnd;
      for (int i = 1; i <= segments; i++) {
        var angle = -halfAngle + angleStep * i;
        var dir = FPQuaternion.AngleAxis(angle, up) * forward;
        var point = center + dir * radius;
        DrawLine(f, prevPoint, point, color, durationSec);
        prevPoint = point;
      }
    }
  }

}