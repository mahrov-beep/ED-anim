namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine.Scripting;

  [Preserve]
  public unsafe class PingPongTargetSystem : SystemMainThreadFilter<Aspect<Transform3D, PingPongTarget>> {
    public override void Update(Frame f, ref Aspect<Transform3D, PingPongTarget> filter) {
      Transform3D*    transform = filter.c1;
      PingPongTarget* target    = filter.c2;

      if (target->Origin == FPVector3.Zero) {
        target->Origin = transform->Position;
      }

      FPVector3 dir = target->Direction;

      // dir.Y = FP._0;
      if (dir == FPVector3.Zero) {
        return;
      }

      FPVector3 dirNorm = dir.Normalized;

      FP distance = target->Distance;
      if (distance <= FP._0) {
        return;
      }

      FP speed = target->Speed;
      if (speed <= FP._0) {
        speed = FP._1;
      }

      target->Phase += f.DeltaTime * speed;

      FP total = distance * FP._2;

      if (target->Phase >= total) {
        target->Phase = FPMath.Repeat(target->Phase, total);
      }

      FP u = target->Phase;
      FP offsetScalar;
      if (u <= distance) {
        offsetScalar = -distance + u * FP._2;
      }
      else {
        offsetScalar = -distance + FP._2 * (total - u);
      }

      FPVector3 newPos = target->Origin + dirNorm * offsetScalar;

      transform->Position = newPos;
    }
  }
}