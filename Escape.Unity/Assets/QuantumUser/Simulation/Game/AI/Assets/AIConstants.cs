using Photon.Deterministic;

namespace Quantum {
  public static class AIConstants {
    public static readonly FP DISTANCE_FACTOR = FP._0_99 - FP._0_04;

    /// имя должно совпадать с именем... вроде с именем ассета (или объекта на сцене)!!!!
    public const string NAV_MESH_NAME = "QuantumNavMesh";
    public const string BB_TARGET_REF = "TargetRef";

    public const string BB_FLAG_IS_STRAIFING = "IsStraifing";

    public const string BB_LAST_SEEN_TARGET_POS = "LastSeenTargetPos";
    public const string BB_LAST_SEEN_TARGET_TIME = "LastSeenTargetTime";
    public const string BB_MEMORY_TARGET_REF = "MemoryTargetRef";
    public const string BB_HEARD_TARGET_REF = "HeardTargetRef";
    
    // public const string Shoot_preShotAimingSeconds
    public static class ConfigSettings {
      public const string SHOOT_PRE_SHOT_AIMING_SECONDS = "Shoot_preShotAimingSeconds";
    }
  }
}