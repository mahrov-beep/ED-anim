namespace Quantum {
  using System;
  using Photon.Deterministic;
  using System.Collections.Generic;
  using System.Diagnostics.CodeAnalysis;

  public unsafe class PerceptionMemorySystem : SystemMainThreadFilter<PerceptionMemorySystem.Filter> {
    readonly List<EntityRef>                                      toRemoveBuffer      = new();
    readonly List<KeyValuePair<EntityRef, PerceptionMemoryEntry>> memoryEntriesBuffer = new();

    static readonly Comparison<KeyValuePair<EntityRef, PerceptionMemoryEntry>> PruneComparison =
      (a, b) => {
        var cmp = CalculateMemoryImportance(b.Value).CompareTo(CalculateMemoryImportance(a.Value));

        return cmp != 0 ? cmp : a.Key.Index.CompareTo(b.Key.Index);
      };

    public struct Filter {
      public EntityRef         EntityRef;
      public PerceptionMemory* Memory;
      public Bot*              Bot;
    }

    public override void Update(Frame f, ref Filter filter) {
      if (f.IsPredicted) {
        return;
      }

      if (filter.Memory->MemoryModule == default) {
        return;
      }

      var config = f.FindAsset(filter.Memory->MemoryModule);

      FP visionUpdateInterval = FP._0_50;
      if (filter.Bot->VisionModule != default) {
        var visionConfig = f.FindAsset(filter.Bot->VisionModule);
        visionUpdateInterval = visionConfig.updateInterval;
      }

      SystemMetrics.Begin("Memory.Decay");
      ProcessMemoryDecay(f, in filter, config, visionUpdateInterval);
      SystemMetrics.End("Memory.Decay");
    }

    void ProcessMemoryDecay(Frame f, in Filter filter, BotMemoryModule config, FP visionUpdateInterval) {
      var memory = filter.Memory;

      var currentTime = f.Number * f.DeltaTime;
      toRemoveBuffer.Clear();
      var knownTargets = f.ResolveDictionary(memory->KnownTargets);

      foreach (var kvp in knownTargets) {
        var target = kvp.Key;
        var entry  = kvp.Value;

        if (!f.Exists(target)) {
          toRemoveBuffer.Add(target);
          continue;
        }

        if (f.Has<CharacterStateDead>(target)) {
          toRemoveBuffer.Add(target);
          continue;
        }

        var timeSinceLastPerception = GetTimeSinceLastPerception(entry, currentTime);

        if (timeSinceLastPerception > config.ForgetTime) {
          toRemoveBuffer.Add(target);
          continue;
        }

        var decayAmount = f.DeltaTime / config.DecayTime;
        entry.Confidence = FPMath.Max(FP._0, entry.Confidence - decayAmount);

        if (entry.Confidence <= FP._0) {
          toRemoveBuffer.Add(target);
          continue;
        }

        var timeSinceLastSeen = currentTime - entry.LastSeenTime;
        if (timeSinceLastSeen > visionUpdateInterval * 2) {
          entry.ContinuousVisionTime = FPMath.Max(FP._0, entry.ContinuousVisionTime - visionUpdateInterval);
        }

        if (entry.LastKnownVelocity != FPVector3.Zero) {
          var timeSinceUpdate = GetTimeSinceLastPerception(entry, currentTime);
          entry.LastKnownPosition += entry.LastKnownVelocity * timeSinceUpdate;
        }

        knownTargets[target] = entry;
      }

      foreach (var target in toRemoveBuffer) {
        knownTargets.Remove(target);
      }

      if (knownTargets.Count > config.MaxTrackedTargets) {
        PruneLeastImportantMemories(f, in filter, config);
      }
    }

    static FP GetTimeSinceLastPerception(PerceptionMemoryEntry entry, FP currentTime) {
      return currentTime - FPMath.Max(entry.LastSeenTime, entry.LastHeardTime);
    }

    void PruneLeastImportantMemories(Frame f, in Filter filter, BotMemoryModule config) {
      var memory       = filter.Memory;
      var knownTargets = f.ResolveDictionary(memory->KnownTargets);
      memoryEntriesBuffer.Clear();

      foreach (var kvp in knownTargets) {
        memoryEntriesBuffer.Add(new KeyValuePair<EntityRef, PerceptionMemoryEntry>(kvp.Key, kvp.Value));
      }

      memoryEntriesBuffer.Sort(PruneComparison);

      var toRemove = memoryEntriesBuffer.Count - config.MaxTrackedTargets;
      for (int i = memoryEntriesBuffer.Count - 1; i >= memoryEntriesBuffer.Count - toRemove && i >= 0; i--) {
        knownTargets.Remove(memoryEntriesBuffer[i].Key);
      }
    }

    static FP CalculateMemoryImportance(PerceptionMemoryEntry entry) {
      var recencyScore    = FPMath.Max(entry.LastSeenTime, entry.LastHeardTime);
      var confidenceScore = entry.Confidence;
      var perceptionTypeScore = entry.PerceptionType switch {
        PerceptionType.Both => FP._2,
        PerceptionType.Seen => FP._1_50,
        _ => FP._1,
      };

      return recencyScore + confidenceScore * 10 + perceptionTypeScore * 5;
    }
  }
}