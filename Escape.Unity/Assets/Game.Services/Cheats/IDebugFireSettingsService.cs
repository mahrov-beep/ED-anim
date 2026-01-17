namespace Game.Services.Cheats {
    using UnityEngine.InputSystem;

    public interface IDebugFireSettingsService {
        bool DebugFireEnabled { get; set; }
        Key  DebugFireKey { get; set; }
    }
}
