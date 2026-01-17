using Photon.Deterministic;
using UnityEngine.Pool;

namespace Quantum {
  public static unsafe class CrouchHelper {

    public static CrouchSettings ResolveCrouchSettings(Frame f, Unit* unit) {
      var unitAsset = f.FindAsset(unit->Asset);
      return unitAsset.GetCrouchSettings();
    } 
    public static bool CanStand(Frame f, EntityRef characterEntity) {
      if (!f.TryGetPointers(characterEntity, out Unit* unit, out KCC* kcc)) {
        return true;
      }
      
      var settings       = f.FindAsset<KCCSettings>(kcc->Settings);
      FP  standingHeight = settings.Height;
      
      FP crouchHeight = standingHeight * ResolveCrouchSettings(f, unit).CrouchHeightRatio;
      FP clearance    = standingHeight - crouchHeight;

      if (clearance <= FP._0) {
        return true;
      }
   

      FPVector3 origin    = kcc->Data.TargetPosition + new FPVector3(FP._0, crouchHeight, FP._0);
      FPVector3 direction = FPVector3.Up;

      using (ListPool<PlayerRef>.Get(out var ignorePlayers)) {
        ignorePlayers.Add(unit->PlayerRef);

        var hit = PhysicsHelper.Raycast(
          f,
          origin,
          direction,
          clearance,
          ignorePlayers,
          computeDetails: false
        );

        return hit == null;
      }
    }
  }
}


