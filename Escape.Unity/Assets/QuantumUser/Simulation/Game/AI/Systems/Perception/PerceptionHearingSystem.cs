namespace Quantum {
  using Photon.Deterministic;

  public unsafe class PerceptionHearingSystem : SystemMainThreadFilter<PerceptionHearingSystem.Filter> {
    public struct Filter {
      public EntityRef         EntityRef;
      public Transform3D*      Transform;
      public Team*             Team;
      public Bot*              Bot;
      public PerceptionMemory* Memory;
    }

    public override ComponentSet Without { get; } = ComponentSet.Create<CharacterStateDead>();

    public override void Update(Frame f, ref Filter filter) {
      if (f.IsPredicted) {
        return;
      }

      if (filter.Bot->HearingModule == default) {
        return;
      }

      var config              = f.FindAsset(filter.Bot->HearingModule);
      var currentTime         = f.Number * f.DeltaTime;
      var timeSinceLastUpdate = currentTime - filter.Bot->LastHearingUpdateTime;

      if (timeSinceLastUpdate < config.UpdateInterval) {
        return;
      }

      filter.Bot->LastHearingUpdateTime = currentTime;

      SystemMetrics.Begin("PerceptionHearing");
      UpdateHearing(f, in filter, config);
      SystemMetrics.End("PerceptionHearing");
    }

    void UpdateHearing(Frame f, in Filter filter, BotHearingModule config) {
      var heardEnemies = f.ResolveList(filter.Bot->HeardEnemies);
      heardEnemies.Clear();

      var sourcePos            = filter.Transform->Position;
      var hearingRadiusSquared = config.Radius * config.Radius;
      var speedThreshold       = config.SpeedThreshold;
      var currentTime          = f.Number * f.DeltaTime;

      var globalConfig = f.FindAsset(f.GameMode.BotGlobalConfig);
      if (globalConfig.Debug.HearingEnabled) {
        DebugDrawHelper.DrawCircle(f, sourcePos, config.Radius, FPQuaternion.Identity, globalConfig.Debug.HearingColor,
          config.UpdateInterval * 2, wire: true);
      }

      bool isThisBotInvisibleByPlayer = f.Has<BotInvisibleByPlayer>(filter.EntityRef);

      var targetFilter = f.Filter<Unit, KCC, Team, Transform3D>();
      while (targetFilter.NextUnsafe(
               out var targetEntity,
               out var targetUnit,
               out var targetKcc,
               out var targetTeam,
               out var targetTransform)) {
        if (targetEntity == filter.EntityRef) {
          continue;
        }

        if (filter.Team->Equals(targetTeam)) {
          continue;
        }

        if (f.TryGetPointer<Bot>(targetEntity, out var targetBot)) {
          //чтобы боты NPC не слышали друг друга, это для пекфоманса
          if (config.IgnoreOtherNpc && !targetBot->IsPlayerBot) {
            continue;
          }

          if (config.InvisibleBotsIgnoreEachOther) {
            // если рядом нету ни одного игрока, то мы друг друга не слышим. Если в OptimizedRange для какого-то из ботов будет игрок - тогда эта проверка не сработает
            bool targetBotInvisibleByPlayer = f.Has<BotInvisibleByPlayer>(targetEntity);
            if (isThisBotInvisibleByPlayer && targetBotInvisibleByPlayer) {
              continue;
            }
          }
        }

        if (f.Has<CharacterStateDead>(targetEntity)) {
          continue;
        }

        var distanceSquared = FPVector3.DistanceSquared(sourcePos, targetTransform->Position);
        if (distanceSquared > hearingRadiusSquared) {
          continue;
        }

        if (targetKcc->RealSpeed < speedThreshold) {
          continue;
        }

        heardEnemies.Add(targetEntity);
        // f.LogTrace(filter.EntityRef, $"[HEARING] Bot {filter.EntityRef} hears {targetEntity} - Speed: {targetKcc->RealSpeed}");

        UpdateMemoryEntry(f, in filter, targetEntity, targetTransform, currentTime);
      }
    }

    static void UpdateMemoryEntry(Frame f, in Filter filter, EntityRef target, Transform3D* targetTransform, FP currentTime) {
      var memory = filter.Memory;
      if (memory->MemoryModule == default) {
        return;
      }

      var memoryConfig = f.FindAsset(memory->MemoryModule);
      var knownTargets = f.ResolveDictionary(memory->KnownTargets);

      if (knownTargets.TryGetValue(target, out var entry)) {
        entry.LastKnownPosition = targetTransform->Position;

        if (f.TryGetPointer<KCC>(target, out var kcc)) {
          entry.LastKnownVelocity = kcc->Data.RealVelocity;
        }

        entry.LastHeardTime  = currentTime;
        entry.PerceptionType = entry.PerceptionType == PerceptionType.Seen || entry.PerceptionType == PerceptionType.Both
          ? PerceptionType.Both
          : PerceptionType.Heard;
        entry.Confidence     = FP._1;

        knownTargets[target] = entry;
      }
      else {
        if (knownTargets.Count >= memoryConfig.MaxTrackedTargets) {
          return;
        }

        var newEntry = new PerceptionMemoryEntry {
          Target            = target,
          LastKnownPosition = targetTransform->Position,
          LastKnownVelocity = FPVector3.Zero,
          LastSeenTime      = FP._0,
          LastHeardTime     = currentTime,
          Confidence        = FP._1,
          PerceptionType    = PerceptionType.Heard,
        };

        if (f.TryGetPointer<KCC>(target, out var kcc)) {
          newEntry.LastKnownVelocity = kcc->Data.RealVelocity;
        }

        knownTargets.Add(target, newEntry);
        filter.Bot->ForceBTUpdate = true;
      }
    }
  }
}