namespace Quantum {
  using System;
  using Photon.Deterministic;
  using Prototypes;
  using Sirenix.OdinInspector;
  using UnityEngine;

  [Serializable]
  public unsafe class RebirthTicketItemAsset : UsableItemAsset {
    [InlineProperty(LabelWidth = 100)]
    public HealthApplicatorPrototype heal;

    public override ItemTypes ItemType => ItemTypes.RebirthTicket;

    public override ItemAssetGrouping Grouping => ItemAssetGrouping.Health;

    public override bool CanBeUsed(Frame f, EntityRef itemEntity) {
      if (!base.CanBeUsed(f, itemEntity)) {
        return false;
      }

      if (!f.TryGet(itemEntity, out Item item)) {
        return false;
      }

      if (!f.Has<Unit>(item.Owner)) {
        return false;
      }

      if (!CharacterFsm.CurrentStateIs<CharacterStateDead>(f, item.Owner)) {
        return false;
      }

      return true;
    }

    public override void UseItem(Frame f, EntityRef itemEntity, EntityRef unitEntity) {
      base.UseItem(f, itemEntity, unitEntity);

      this.heal.ApplyOn(f, unitEntity, unitEntity);

      var targetTeam = f.GetPointer<Team>(unitEntity);

      SetSpawnPoint(f, unitEntity, targetTeam);

      f.Events.UnitRebirth(unitEntity);
    }

    // TODO merge with CharacterCreationSystem
    void SetSpawnPoint(Frame f, EntityRef unitRef, Team* team) {
      var unit = f.GetPointer<Unit>(unitRef);
      var uTransform = f.GetPointer<Transform3D>(unitRef);

      var spawns = f.Filter<SpawnPoint, Transform3D>();

      while (spawns.NextUnsafe(out var e, out var spawn, out var sTransform)) {
        bool isMySpawn = unit->PlayerRef % f.ComponentCount<SpawnPoint>() == spawn->ID;
        if (isMySpawn) {
          uTransform->Teleport(f, sTransform->Position);
          return;
        }
      }

      Debug.LogError($"SpawnPoint not found for {unit->PlayerRef}({(int)unit->PlayerRef}), set random spawn position!");
      
      var randomPos = FPVector2Helper.RandomInsideCircle(&unit->RNG, FPVector2.Zero, FP._10);
      var spawnPos  = NavMeshHelper.FindNearestPointOnNavMesh(f, randomPos.XOY);
      uTransform->Teleport(f, spawnPos);
    }
  }
}