namespace Quantum {
  using Collections;
  using Photon.Deterministic;
  using static LineOfSightHelper;

  public unsafe class PerceptionVisionSystem : SystemMainThreadFilter<PerceptionVisionSystem.Filter> {
    public struct Filter {
      public EntityRef         EntityRef;
      public Transform3D*      Transform;
      public Team*             Team;
      public Bot*              Bot;
      public PerceptionMemory* Memory;
    }

    struct Other {
      public EntityRef    EntityRef;
      public Transform3D* Transform;
      public Team*        Team;
      public Unit*        Unit;
    }

    public override ComponentSet Without { get; } = ComponentSet.Create<CharacterStateDead>();

    public override void Update(Frame f, ref Filter filter) {
      if (f.IsPredicted) {
        return;
      }

      if (filter.Bot->VisionModule == default) {
        return;
      }

      var config              = f.FindAsset(filter.Bot->VisionModule);
      var currentTime         = f.Number * f.DeltaTime;
      var timeSinceLastUpdate = currentTime - filter.Bot->LastVisionUpdateTime;

      if (timeSinceLastUpdate < config.updateInterval) {
        return;
      }

      filter.Bot->LastVisionUpdateTime = currentTime;

      SystemMetrics.Begin("PerceptionVision");
      UpdateVision(f, in filter, config, currentTime);
      SystemMetrics.End("PerceptionVision");
    }

    static void UpdateVision(Frame f, in Filter me, BotVisionModule config, FP currentTime) {
      var visibleEnemies = f.ResolveList(me.Bot->VisibleEnemies);
      visibleEnemies.Clear();

      UpdateForwardVision(f, in me, config, currentTime, visibleEnemies);
      UpdateBackVision(f, in me, config, currentTime, visibleEnemies);
    }

    static void UpdateForwardVision(Frame f, in Filter me, BotVisionModule config, FP currentTime, QList<EntityRef> visibleEnemies) {
      var sourcePos     = me.Transform->Position;
      var sourceForward = me.Transform->Forward;

      var visionRadius        = config.forwardRadius;
      var visionRadiusSquared = visionRadius * visionRadius;
      var visionAngle         = config.forwardAngle;
      var halfAngle           = visionAngle / 2;

      var globalConfig = f.FindAsset(f.GameMode.BotGlobalConfig);
      if (globalConfig.Debug.VisionForwardEnabled) {
        DebugDrawHelper.DrawSector(f, sourcePos, sourceForward, visionRadius, visionAngle, globalConfig.Debug.VisionForwardColor, config.updateInterval * 2);
      }

      var otherFilter = f.FilterStruct(out Other other);
      while (otherFilter.Next(&other)) {
        if (me.EntityRef == other.EntityRef) {
          continue;
        }

        if (me.Team->Equals(other.Team)) {
          continue;
        }

        if (f.Has<CharacterStateDead>(other.EntityRef)) {
          continue;
        }

        var distanceSquared = FPVector3.DistanceSquared(sourcePos, other.Transform->Position);
        if (distanceSquared > visionRadiusSquared) {
          continue;
        }

        var toTarget = (other.Transform->Position - sourcePos).Normalized;
        var angle    = FPVector3.Angle(sourceForward, toTarget);

        if (angle > halfAngle) {
          continue;
        }

        var meEyeHeight    = FPVector3.Up * UnitColliderHeightHelper.GetCurrentHeight(f, me.EntityRef);
        var otherEyeHeight = FPVector3.Up * UnitColliderHeightHelper.GetCurrentHeight(f, other.EntityRef);

        SystemMetrics.Begin("Vision.LineOfSight");
        var hasLineOfSight = HasLineSight(f, sourcePos + meEyeHeight, other.Transform->Position + otherEyeHeight);
        SystemMetrics.End("Vision.LineOfSight");

        if (!hasLineOfSight) {
          continue;
        }

        visibleEnemies.Add(other.EntityRef);

        UpdateMemoryEntry(f, in me, in other, currentTime, dt: config.updateInterval);
      }
    }

    static void UpdateBackVision(Frame f, in Filter me, BotVisionModule config, FP currentTime, QList<EntityRef> visibleEnemies) {
      var sourcePos   = me.Transform->Position;
      var backForward = -me.Transform->Forward;

      var visionRadius        = config.backRadius;
      var visionRadiusSquared = visionRadius * visionRadius;
      var visionAngle         = config.backAngle;
      var halfAngle           = visionAngle / 2;

      var globalConfig = f.FindAsset(f.GameMode.BotGlobalConfig);
      if (globalConfig.Debug.VisionBackEnabled) {
        DebugDrawHelper.DrawSector(f, sourcePos, backForward, visionRadius, visionAngle, globalConfig.Debug.VisionBackColor, config.updateInterval * 2);
      }

      var otherFilter = f.FilterStruct(out Other other);
      while (otherFilter.Next(&other)) {
        if (me.EntityRef == other.EntityRef) {
          continue;
        }

        if (me.Team->Equals(other.Team)) {
          continue;
        }

        if (f.Has<CharacterStateDead>(other.EntityRef)) {
          continue;
        }

        var distanceSquared = FPVector3.DistanceSquared(sourcePos, other.Transform->Position);
        if (distanceSquared > visionRadiusSquared) {
          continue;
        }

        var toTarget = (other.Transform->Position - sourcePos).Normalized;
        var angle    = FPVector3.Angle(backForward, toTarget);

        if (angle > halfAngle) {
          continue;
        }

        var meEyeHeight    = FPVector3.Up * UnitColliderHeightHelper.GetCurrentHeight(f, me.EntityRef);
        var otherEyeHeight = FPVector3.Up * UnitColliderHeightHelper.GetCurrentHeight(f, other.EntityRef);

        SystemMetrics.Begin("VisionBack.LineOfSight");
        var hasLineOfSight = HasLineSight(f, sourcePos + meEyeHeight, other.Transform->Position + otherEyeHeight);
        SystemMetrics.End("VisionBack.LineOfSight");

        if (!hasLineOfSight) {
          continue;
        }

        if (!visibleEnemies.Contains(other.EntityRef)) {
          visibleEnemies.Add(other.EntityRef);
        }

        UpdateMemoryEntry(f, in me, in other, currentTime, dt: config.updateInterval);
      }
    }

    static void UpdateMemoryEntry(Frame f, in Filter me, in Other other, FP currentTime, FP dt) {
      var memory = me.Memory;
      if (memory->MemoryModule == default) {
        return;
      }

      var memoryConfig = f.FindAsset(memory->MemoryModule);
      var knownTargets = f.ResolveDictionary(memory->KnownTargets);

      var target = other.EntityRef;
      if (knownTargets.TryGetValue(target, out var entry)) {
        entry.LastKnownPosition = other.Transform->Position;

        if (f.TryGetPointer<KCC>(target, out var kcc)) {
          entry.LastKnownVelocity = kcc->Data.RealVelocity;
        }

        entry.LastSeenTime         =  currentTime;
        entry.ContinuousVisionTime += dt;
        entry.PerceptionType = entry.PerceptionType == PerceptionType.Heard || entry.PerceptionType == PerceptionType.Both
          ? PerceptionType.Both
          : PerceptionType.Seen;
        entry.Confidence           =  FP._1;

        knownTargets[target] = entry;
      }
      else {
        if (knownTargets.Count >= memoryConfig.MaxTrackedTargets) {
          return;
        }

        var newEntry = new PerceptionMemoryEntry {
          Target               = target,
          LastKnownPosition    = other.Transform->Position,
          LastKnownVelocity    = FPVector3.Zero,
          LastSeenTime         = currentTime,
          LastHeardTime        = FP._0,
          Confidence           = FP._1,
          PerceptionType       = PerceptionType.Seen,
          ContinuousVisionTime = dt,
        };

        if (f.TryGetPointer<KCC>(target, out var kcc)) {
          newEntry.LastKnownVelocity = kcc->Data.RealVelocity;
        }

        knownTargets.Add(target, newEntry);
        me.Bot->ForceBTUpdate = true;
      }
    }
  }
}
