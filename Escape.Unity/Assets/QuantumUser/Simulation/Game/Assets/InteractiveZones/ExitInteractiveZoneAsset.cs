namespace Quantum.InteractiveZones {
  public class ExitInteractiveZoneAsset : InteractiveZoneAsset {
    public override unsafe bool CanInteract(Frame f, EntityRef zoneEntity, EntityRef beneficiaryUnitEntity) {
      if (!f.TryGet(beneficiaryUnitEntity, out Unit unit)) {
        return false;
      }

      if (f.Has<Bot>(beneficiaryUnitEntity)) {
        return false;
      }

      var zone = f.GetPointer<ExitZone>(zoneEntity);

      return zoneEntity == unit.TargetExitZone;
    }

    public override unsafe void OnInteractComplete(Frame f, EntityRef zoneEntity, EntityRef beneficiaryUnitEntity) {
      if (!f.TryGet(beneficiaryUnitEntity, out Unit unit)) {
        return;
      }

      var photonActorId = f.PlayerToActorId(unit.PlayerRef);
      if (photonActorId == null) {
        Log.Error("photon actor id is null");
        return;
      }

      // оружие не сохраняет заряженные патроны, а AmmoBox сохраняет. Так что возвращаем патроны в AmmoBox
      Weapon.UnloadAmmoFromWeaponToAmmoBox(f, unit.PrimaryWeapon);
      Weapon.UnloadAmmoFromWeaponToAmmoBox(f, unit.SecondaryWeapon);

      var playerSnapshot = GameSnapshotHelper.Make(f);

      // нельзя просто уничтожить ентити, она еще нужна для показа окон завершния игры
      //f.Destroy(beneficiaryUnitEntity);
      f.Add<UnitExited>(beneficiaryUnitEntity);

      f.Signals.OnExitZoneUsed(photonActorId.Value);
      f.Events.ExitZoneUsed(f, photonActorId.Value, playerSnapshot);
    }
  }
}