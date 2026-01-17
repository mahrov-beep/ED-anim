namespace Quantum {
  using UnityEngine;

  [CreateAssetMenu(menuName = "Quantum/Game/Knock Settings Config", fileName = "KnockSettingsConfig")]
  public class KnockSettingsConfig : ScriptableObject {
    [SerializeField]
    KnockSettings settings = KnockSettings.Default;

    public KnockSettings Settings => settings;

    public KnockSettings GetKnockSettings() {
      return settings.WithFallbacks();
    }
  }
}
