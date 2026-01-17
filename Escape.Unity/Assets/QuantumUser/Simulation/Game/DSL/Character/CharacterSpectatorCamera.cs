namespace Quantum {
  using Photon.Deterministic;
  using static Photon.Deterministic.FPQuaternion;
  using static Photon.Deterministic.FPVector2;
  public unsafe partial struct CharacterSpectatorCamera {
    public static ComponentHandler<CharacterSpectatorCamera> OnAdd => (f, e, c) => {
      c->CameraEntity = f.Create(f.GameModeAiming.unitSpectatorCamera);
    };

    public static ComponentHandler<CharacterSpectatorCamera> OnRemove => (f, e, c) => {
      f.Destroy(c->CameraEntity);
    };

    // /// world forward direction => FPVector2.Rotate(FPVector2.Up, -SpectatorCameraCurrentRotation)
    // public FPVector2 Forward => Rotate(Up, -SpectatorCameraCurrentRotation);

    // public FPQuaternion CameraCurrentRotation => RadianAxis(SpectatorCameraCurrentRotation, FPVector3.Up);
  }
}