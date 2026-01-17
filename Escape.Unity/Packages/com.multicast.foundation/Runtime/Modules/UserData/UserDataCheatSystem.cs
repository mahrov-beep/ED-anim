namespace Multicast.Modules.UserData {
    using Cheats;
    using Scellecs.Morpeh;

    public class UserDataCheatSystem : SystemBase {
        [Inject] private ICheatButtonsRegistry cheatButtonsRegistry;

        public override void OnAwake() {
            this.cheatButtonsRegistry.RegisterAction("Show UserData Selector After Restart",
                () => UserDataUI.ShowUserDataSelectorOnce = true);
        }

        public override void OnUpdate(float deltaTime) {
        }
    }
}