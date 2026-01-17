namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine.Scripting;
  using static Draw;
  using static UnionDrawData;

  [Preserve]
  public unsafe class DebugDrawSystem : SystemMainThread {
    public struct Filter {
      public EntityRef Entity;

      public DebugGizmo* DebugDraw;
    }

    public override void Update(Frame f) {
      var componentFilter = f.FilterStruct(out Filter filter);

      Filter*   filterPtr   = &filter;
      FPVector3 localCenter = FPVector3.Zero;

      while (componentFilter.Next(filterPtr)) {
        var e    = filter.Entity;
        var draw = filter.DebugDraw;

        if (draw->FromLocalTransform) {
          if (!f.TryGetPointer(e, out Transform3D* transform)) {
            f.LogError(e, "Could not find transform!");
            return;
          }

          localCenter = transform->Position;
        }

        switch (draw->Data.Field) {
          case LINEDATA:
            var line = draw->Data.LineData;

            if (draw->FromLocalTransform) {
              line->Start = localCenter;
            }

            Line(line->Start, line->End, draw->Color);

            break;

          case CIRCLEDATA:
            var circle = draw->Data.CircleData;

            if (draw->FromLocalTransform) {
              circle->Center = localCenter;
            }

            Circle(circle->Center, circle->Radius, circle->Rotation, draw->Color, circle->Wire);

            break;

          case RAYDATA:
            var ray = draw->Data.RayData;

            if (draw->FromLocalTransform) {
              ray->Origin = localCenter;
            }

            Ray(ray->Origin, ray->Direction, draw->Color);

            break;
        }

      }
    }
  }
}