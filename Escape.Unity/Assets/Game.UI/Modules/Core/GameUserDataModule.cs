namespace Game.UI.Modules.Core {
    using Domain.UserData;
    using Multicast.Install;
    using Multicast.Modules.Playtime;
    using Multicast.Modules.UserData;
    using Multicast.UserData;

    [ScriptableModule(Category = ScriptableModuleCategory.GAME_CORE)]
    public class GameUserDataModule : UserDataModuleBase<GameData> {
        protected override GameData CreateUserData(UdArgs args) => new GameData(args);
    }
}