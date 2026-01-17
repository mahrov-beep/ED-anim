namespace Quantum.Quests {
  using System;
  using System.Collections.Generic;
  using UnityEngine.Pool;

  public unsafe class QuestCounterTaskSystem : SystemSignalsOnly,
    ISignalOnGameStart,
    ISignalOnUnitDead,
    ISignalOnExitZoneUsed {
    public void OnGameStart(Frame f) {
      f.Events.QuestCounterTaskDone(f, null,
        QuestCounterPropertyTypes.GameStarted,
        1,
        Array.Empty<QuestTaskFilters>());
    }

    public void OnUnitDead(Frame f, EntityRef e) {
      ReportDied();
      ReportEnemyKill();

      void ReportDied() {
        if (!f.TryGetPointer(e, out Unit* diedUnit)) {
          return;
        }

        var photonActorNr = f.PlayerToActorId(diedUnit->PlayerRef);
        if (photonActorNr == null) {
          return;
        }

        f.Events.QuestCounterTaskDone(f, photonActorNr.Value,
          QuestCounterPropertyTypes.Died,
          1,
          Array.Empty<QuestTaskFilters>());
      }

      void ReportEnemyKill() {
        if (!f.TryGetPointer(e, out CharacterStateDead* deadState)) {
          return;
        }

        if (!f.TryGetPointer(deadState->KilledBy, out Unit* killerUnit)) {
          return;
        }

        var photonActorNr = f.PlayerToActorId(killerUnit->PlayerRef);
        if (photonActorNr == null) {
          return;
        }

        using var _ = ListPool<QuestTaskFilters>.Get(out var filtersBuilder);

        if (killerUnit->GetActiveWeaponConfig(f) is { } killerWeaponAsset) {
          filtersBuilder.AddRange(killerWeaponAsset.questEnemyKilledFilters);
        }

        f.Events.QuestCounterTaskDone(f, photonActorNr.Value,
          QuestCounterPropertyTypes.EnemyKilled,
          1,
          MakeArray(filtersBuilder));
      }
    }

    public void OnExitZoneUsed(Frame f, Int32 photonActorId) {
      f.Events.QuestCounterTaskDone(f, photonActorId,
        QuestCounterPropertyTypes.Extracted,
        1,
        Array.Empty<QuestTaskFilters>());
    }

    static T[] MakeArray<T>(List<T> list) where T : struct {
      return list.Count > 0 ? list.ToArray() : Array.Empty<T>();
    }
  }
}