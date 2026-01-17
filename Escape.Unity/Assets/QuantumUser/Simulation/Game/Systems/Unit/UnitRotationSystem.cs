namespace Quantum {
  using UnityEngine.Scripting;

  [Preserve]
  public unsafe class UnitRotationSystem : SystemMainThreadFilter<UnitRotationSystem.Filter> {
    public struct Filter {
      public EntityRef Entity;

      public Unit*                     Unit;
      public InputContainer*           InputContainer;
      public CharacterSpectatorCamera* SpectatorCamera;
    }

    public override void Update(Frame f, ref Filter filter) {
      if (EAttributeType.Set_LockRotation.IsValueSet(f, filter.Entity)) {
        return;
      }

      var aiming = f.GameModeAiming;

      var spectatorCamera = filter.SpectatorCamera;

      aiming.UpdateSpectatorCamera(f, filter.Entity, filter.InputContainer, spectatorCamera);

      spectatorCamera->CharacterCurrentRotation      = aiming.GetCharacterRotation(f, spectatorCamera);
      spectatorCamera->CharacterCurrentPitchRotation = aiming.GetCharacterPitchRotation(f, spectatorCamera);
    }
  }
}