namespace Game.UI.Controllers.Features.ExpProgressionRewards {
    using System;
    using Cysharp.Threading.Tasks;
    using Domain;
    using Domain.Currencies;
    using Domain.ExpProgressionRewards;
    using Multicast;
    using Shared;
    using Shared.UserProfile.Commands.Exp;
    using Shared.UserProfile.Data;
    using SoundEffects;
    using Widgets.LevelUp;

    [Serializable, RequireFieldsInit]
    public struct ExpProgressionRewardsLevelUpControllerArgs : IResultControllerArgs {
    }

    public class ExpProgressionRewardsLevelUpController : ResultController<ExpProgressionRewardsLevelUpControllerArgs> {
        [Inject] private CurrenciesModel            currenciesModel;
        [Inject] private SdUserProfile              userProfile;
        [Inject] private GameDef                    gameDef;
        [Inject] private ISoundEffectService        soundEffectService;
        [Inject] private ExpProgressionRewardsModel expProgressionRewardsModel;

        protected override async UniTask Execute(Context context) {
            while (this.expProgressionRewardsModel.TryGetLevelUp(out var currentLevelDef, out var nextLevelDef)) {
                var levelContinueTcs = new UniTaskCompletionSource();

                var levelUpScreen = await context.RunDisposable(new UiScreenControllerArgs {
                    Route = UiConstants.Routes.LevelUp,
                    Page = () => new LevelUpWidget {
                        PrevLevel  = currentLevelDef.level,
                        NextLLevel = nextLevelDef.level,
                        OnContinue = () => OnContinue().Forget(),
                    },
                    TransitionDuration        = 0,
                    ReverseTransitionDuration = 0f,
                });
                await levelContinueTcs.Task;
                await levelUpScreen.DisposeAsync();

                async UniTask OnContinue() {
                    this.soundEffectService.PlayOneShot(CoreConstants.SoundEffectKeys.LEVEL_UP);

                    await context.Server.ExecuteUserProfile(new UserProfileLevelUpCommand(), ServerCallRetryStrategy.RetryWithUserDialog);

                    levelContinueTcs.TrySetResult();
                }
            }
        }
    }
}