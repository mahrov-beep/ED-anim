namespace Game.Shared.UserProfile.Commands.FeatureToggles {
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileSetFeatureTogglesCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public ToggleState[] Toggles;

        [MessagePackObject]
        public struct ToggleState {
            [Key(0)] public string FeatureToggleKey;
            [Key(1)] public string Variant;
        }
    }

    public class UserProfileSetFeatureTogglesCommandHandler : UserProfileServerCommandHandler<UserProfileSetFeatureTogglesCommand> {
        private readonly GameDef gameDef;

        public UserProfileSetFeatureTogglesCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileSetFeatureTogglesCommand command) {
            if (command.Toggles is not { } toggles) {
                return BadRequest("Toggles is null");
            }

            foreach (var toggle in toggles) {
                if (!this.gameDef.FeatureToggles.TryGet(toggle.FeatureToggleKey, out var featureToggleDef)) {
                    continue;
                }

                if (string.IsNullOrEmpty(toggle.Variant)) {
                    gameData.FeatureToggles.RemoveFeature(featureToggleDef.key);
                }
                else {
                    var variantData = gameData.FeatureToggles.GetOrCreateFeature(featureToggleDef.key, out var variantCreated);

                    variantData.SetVariant(toggle.Variant);

                    if (!variantCreated) {
                        variantData.IncrementTimesChanged();
                    }
                }
            }

            return Ok;
        }
    }
}