namespace Quantum {
  /// <summary>
  /// LateLagCompensationSystem destroys all proxies created in EarlyLagCompensationSystem and records transforms of entities with LagCompensationTarget component.
  /// This system should run last.
  /// </summary>
  public unsafe class LateLagCompensationSystem : SystemMainThread {
    public override void Update(Frame f) {
      // Cleanup - destroy all existing proxies.
      var proxies = f.Filter<LagCompensationProxy>();
      proxies.UseCulling = false;

      while (proxies.NextUnsafe(out EntityRef proxyEntity, out LagCompensationProxy* proxy)) {
        f.Destroy(proxyEntity);
      }

      if (f.IsVerified) {
        // Store transforms of lag compensated entities.
        // It is fine to record in verified frames only because the snapshot interpolation happens between two verified frames.
        var targets = f.Filter<Transform3D, LagCompensationTarget>();
        while (targets.NextUnsafe(out EntityRef entity, out Transform3D* transform, out LagCompensationTarget* target)) {
          target->StoreTransform(f.Number, transform);
        }
      }
    }
  }
}