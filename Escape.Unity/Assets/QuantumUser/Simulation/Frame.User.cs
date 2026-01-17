namespace Quantum {
  using System.Diagnostics.CodeAnalysis;
  public unsafe partial class Frame {
    public GameModeAsset GameMode       => FindAsset(RuntimeConfig.GameModeAsset);
    public AimingAsset   GameModeAiming => FindAsset(GameMode.aiming);

#if UNITY_ENGINE
#endif

    [SuppressMessage("ReSharper", "EnforceIfStatementBraces")]
    public bool IsAlly(EntityRef a, EntityRef b) {
      if (!a.IsValid) return false;
      if (!b.IsValid) return false;

      if (a == b) return true;

      if (Unsafe.TryGetPointer(a, out Team* teamA) && Unsafe.TryGetPointer(b, out Team* teamB))
        return teamA->Index == teamB->Index;

      return false;
    }
  }
}