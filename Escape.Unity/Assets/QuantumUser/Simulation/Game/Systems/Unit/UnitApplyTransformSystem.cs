namespace Quantum {
  using Photon.Deterministic;

  public unsafe class UnitApplyTransformSystem : SystemMainThreadFilter<UnitApplyTransformSystem.Filter> {
    ComponentSet without;

    public struct Filter {
      public EntityRef Entity;

      public Transform3D*              Transform;
      public Unit*                     Unit;
      public CharacterSpectatorCamera* SpectatorCamera;
    }

    public override ComponentSet Without { get; } = ComponentSet.Create<KCC>();

    public override void Update(Frame f, ref Filter filter) {
      var lookRotation = FPQuaternion.RadianAxis(filter.SpectatorCamera->CharacterCurrentRotation, FPVector3.Up);
     
      filter.Transform->Rotation = lookRotation;
    }
  }
}