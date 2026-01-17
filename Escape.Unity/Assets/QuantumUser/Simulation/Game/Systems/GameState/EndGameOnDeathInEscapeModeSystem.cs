namespace Quantum {
  public unsafe class EndGameOnDeathInEscapeModeSystem : SystemSignalsOnly, ISignalOnUnitDead {
    public void OnUnitDead(Frame f, EntityRef unitEntity) {
      if (f.GameMode.rule != GameRules.Escape) {
        return;
      }

      if (f.Has<Bot>(unitEntity)) {
        return;
      }

      if (!f.TryGet(unitEntity, out Unit unit) || unit.PlayerRef == default) {
        return;
      }

      if (f.TryGetPointer(unitEntity, out CharacterLoadout* loadout) &&
          loadout->TryFindRebirthTicketInTrash(f, out _)) {
        return;
      }

      var photonActorId = f.PlayerToActorId(unit.PlayerRef);
      if (photonActorId == null) {
        Log.Error("photon actor id is null");
        return;
      }

      var playerSnapshot = GameSnapshotHelper.Make(f);

      f.Events.GameLost(f, photonActorId.Value, playerSnapshot);
    }
  }
}