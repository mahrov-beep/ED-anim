namespace Quantum {
  using Photon.Deterministic;

  public unsafe class RotateKCCToSpectatorRotationSystem : SystemMainThreadFilter<RotateKCCToSpectatorRotationSystem.Filter> {
    public struct Filter {
      public EntityRef Entity;

      public KCC*                      KCC;
      public CharacterSpectatorCamera* SpectatorCamera;
    }

    public override void Update(Frame f, ref Filter filter) {
      var spectatorCamera = filter.SpectatorCamera;

      filter.KCC->SetLookRotation(FPQuaternion.RadianAxis(spectatorCamera->CharacterCurrentRotation, FPVector3.Up));
    }
  }
}