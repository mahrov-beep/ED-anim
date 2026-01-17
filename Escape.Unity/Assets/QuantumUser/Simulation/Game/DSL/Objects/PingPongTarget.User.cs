namespace Quantum {
  using Photon.Deterministic;

  public unsafe partial struct PingPongTarget {
    public static ComponentHandler<PingPongTarget> OnAdd => static (f, e, c) => {
      if (f.TryGetPointer<Transform3D>(e, out var t)) {
        c->Origin = t->Position;
      }
      c->Phase = FP._0;
      if (c->Speed <= FP._0) {
        c->Speed = FP._1;
      }
    };
  }
}


