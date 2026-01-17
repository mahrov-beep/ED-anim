namespace Game.Services.Cheats {
    using UnityEngine.InputSystem;

    /// <summary>
    /// Stores debug fire settings for cheats panel and character behaviour.
    /// </summary>
    public class DebugFireSettingsService : IDebugFireSettingsService {
        public bool DebugFireEnabled { get; set; }
        public Key  DebugFireKey { get; set; } = Key.T;
    }
}
