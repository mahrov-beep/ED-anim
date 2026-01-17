namespace Game.UI.Controllers.Tutorial {
    using Cysharp.Threading.Tasks;
    using Multicast;

    public abstract class BaseTutorialSequence : BaseCoreTutorialSequence {
        public virtual async UniTask On_MainMenu_Flow(ControllerBase.Context context) {
            await this.ExecuteAsync(context, (ctx, seq) => seq.On_MainMenu_Flow(ctx));
        }

        public virtual async UniTask On_GunsmithMenu_Activated(ControllerBase.Context context) {
            await this.ExecuteAsync(context, (ctx, seq) => seq.On_GunsmithMenu_Activated(ctx));
        }

        public virtual async UniTask On_GunsmithMenu_LoadoutBuy(ControllerBase.Context context) {
            await this.ExecuteAsync(context, (ctx, seq) => seq.On_GunsmithMenu_LoadoutBuy(ctx));
        }

        public virtual async UniTask On_GunsmithMenu_Close(ControllerBase.Context context) {
            await this.ExecuteAsync(context, (ctx, seq) => seq.On_GunsmithMenu_Close(ctx));
        }

        public virtual async UniTask On_GameModeSelector_Activated(ControllerBase.Context context) {
            await this.ExecuteAsync(context, (ctx, seq) => seq.On_GameModeSelector_Activated(ctx));
        }

        public virtual async UniTask On_GameModeSelector_ModeSelected(ControllerBase.Context context, string gameModeKey) {
            await this.ExecuteAsync(context, (ctx, seq) => seq.On_GameModeSelector_ModeSelected(ctx, gameModeKey));
        }

        public virtual async UniTask On_GameModeSelector_GameConfirmed(ControllerBase.Context context, string gameModeKey) {
            await this.ExecuteAsync(context, (ctx, seq) => seq.On_GameModeSelector_GameConfirmed(ctx, gameModeKey));
        }
    }
}