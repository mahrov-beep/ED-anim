namespace Quantum {
  public unsafe class UnitApplySpectatorCameraTransformSystem : SystemMainThreadFilter<UnitApplySpectatorCameraTransformSystem.Filter> {
    public struct Filter {
      public EntityRef Entity;

      public Unit*                     Unit;
      public CharacterSpectatorCamera* SpectatorCamera;
    }

    public override void Update(Frame f, ref Filter filter) {
      var aiming = f.GameModeAiming;

      aiming.ApplySpectatorEntity(f, filter.Entity, filter.SpectatorCamera);
    }
  }
}