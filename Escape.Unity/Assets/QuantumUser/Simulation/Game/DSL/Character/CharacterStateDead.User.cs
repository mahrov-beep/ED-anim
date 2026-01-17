namespace Quantum {
  using Photon.Deterministic;

  public unsafe partial struct CharacterStateDead {
    public static ComponentHandler<CharacterStateDead> ResetInput => static (f, e, _) =>
      InputHelper.ResetMovementInput(f, e);

    public static ComponentHandler<CharacterStateDead> DebotifyDeadUnit => static (f, e, _) => {
      if (f.Has<Bot>(e)) {
        AIHelper.Debotify(f, e);
      }
    };

    public static ComponentHandler<CharacterStateDead> DisableComponents => static (f, e, _) => {
      SetActive(f, e, false);
    };

    public static ComponentHandler<CharacterStateDead> EnableComponents => static (f, e, _) => {
      SetActive(f, e, true);
    };

    static void SetActive(Frame f, EntityRef e, bool enabled) {
      if (f.TryGetPointer<PhysicsCollider3D>(e, out var collider3D)) {
        collider3D->Enabled = enabled;
      }

      if (f.TryGetPointer<PhysicsBody3D>(e, out var body3D)) {
        body3D->Enabled = enabled;
      }

      if (f.TryGetPointer<KCC>(e, out var kcc)) {
        kcc->SetActive(enabled);
      }
    }
  }
}
