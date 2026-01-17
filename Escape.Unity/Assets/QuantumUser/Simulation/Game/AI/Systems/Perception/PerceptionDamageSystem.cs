namespace Quantum {
  using Photon.Deterministic;

  public unsafe class PerceptionDamageSystem : SystemSignalsOnly, ISignalOnUnitDamage {
    public void OnUnitDamage(Frame f, EntityRef source, EntityRef target, FP value) {
      if (f.IsPredicted) {
        return;
      }

      if (!f.TryGetPointer<Attack>(source, out var attack)) {
        return;
      }

      var attackerRef = attack->SourceUnitRef;
      if (!f.Exists(attackerRef) || f.Has<CharacterStateDead>(attackerRef)) {
        return;
      }

      bool isBotWithMemory = f.TryGetPointers<Bot, PerceptionMemory>(target,
        out var bot,
        out var memory);
      if (!isBotWithMemory) {
        return;
      }

      if (f.IsAlly(target, attackerRef)) {
        return;
      }

      if (!f.TryGetPointer<Transform3D>(attackerRef, out var attackerTransform)) {
        return;
      }

      UpdateMemoryEntry(f, bot, memory, attackerRef, attackerTransform);
    }

    static void UpdateMemoryEntry(Frame f, Bot* bot, PerceptionMemory* memory, EntityRef attacker, Transform3D* attackerTransform) {
      var memoryConfig = f.FindAsset(memory->MemoryModule);
      var knownTargets = f.ResolveDictionary(memory->KnownTargets);
      var currentTime  = f.Number * f.DeltaTime;

      if (knownTargets.TryGetValue(attacker, out var entry)) {
        entry.LastKnownPosition = attackerTransform->Position;

        if (f.TryGetPointer<KCC>(attacker, out var kcc)) {
          entry.LastKnownVelocity = kcc->Data.RealVelocity;
        }

        entry.LastHeardTime  = currentTime;
        entry.PerceptionType = entry.PerceptionType == PerceptionType.Seen || entry.PerceptionType == PerceptionType.Both
          ? PerceptionType.Both
          : PerceptionType.Heard;
        entry.Confidence     = FP._1;

        knownTargets[attacker] = entry;
      }
      else {
        if (knownTargets.Count >= memoryConfig.MaxTrackedTargets) {
          return;
        }

        var newEntry = new PerceptionMemoryEntry {
          Target            = attacker,
          LastKnownPosition = attackerTransform->Position,
          LastKnownVelocity = FPVector3.Zero,
          LastSeenTime      = FP._0,
          LastHeardTime     = currentTime,
          Confidence        = FP._1,
          PerceptionType    = PerceptionType.Heard,
        };

        if (f.TryGetPointer<KCC>(attacker, out var kcc)) {
          newEntry.LastKnownVelocity = kcc->Data.RealVelocity;
        }

        knownTargets.Add(attacker, newEntry);
        bot->ForceBTUpdate = bot->Intent.AttackTarget == EntityRef.None;
      }
    }
  }
}