namespace Quantum {
  using Photon.Deterministic;
  public unsafe class AIDebugDrawSystem : SystemMainThreadFilter<AIDebugDrawSystem.Filter> {
    public struct Filter {
      public EntityRef    EntityRef;
      public Transform3D* Transform;
      public Bot*         Bot;
    }

    public override void Update(Frame f, ref Filter filter) {
      DrawAttackTarget(f, ref filter);
      DrawMovementTarget(f, ref filter);
    }

    static void DrawAttackTarget(Frame f, ref Filter filter) {
      var globalConfig = f.FindAsset(f.GameMode.BotGlobalConfig);
      if (!globalConfig.Debug.AttackTargetEnabled) {
        return;
      }

      var attackTarget = filter.Bot->Intent.AttackTarget;
      if (attackTarget == default) {
        return;
      }

      if (!f.Unsafe.TryGetPointer<Transform3D>(attackTarget, out var targetTransform)) {
        return;
      }

      var botPos    = filter.Transform->Position;
      var targetPos = targetTransform->Position;

      DebugDrawHelper.DrawLine(f, botPos, targetPos, globalConfig.Debug.AttackTargetColor, FP._0_50);
    }

    static void DrawMovementTarget(Frame f, ref Filter filter) {
      var globalConfig = f.FindAsset(f.GameMode.BotGlobalConfig);
      if (!globalConfig.Debug.MovementTargetEnabled) {
        return;
      }

      var movementTarget = filter.Bot->Intent.MovementTarget;
      if (movementTarget == default) {
        return;
      }

      var botPos = filter.Transform->Position;

      DebugDrawHelper.DrawLine(f, botPos, movementTarget, globalConfig.Debug.MovementTargetColor, FP._0_50);
    }
  }

}