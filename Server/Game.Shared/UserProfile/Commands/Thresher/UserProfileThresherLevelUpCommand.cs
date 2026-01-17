namespace Game.Shared.UserProfile.Commands.Thresher {
    using System.Linq;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileThresherLevelUpCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public string ThresherKey;
    }

    public class UserProfileThresherLevelUpCommandHandler : UserProfileServerCommandHandler<UserProfileThresherLevelUpCommand> {
        private readonly GameDef gameDef;

        public UserProfileThresherLevelUpCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileThresherLevelUpCommand command) {
            if (!this.gameDef.Threshers.TryGet(command.ThresherKey, out var thresherDef)) {
                return BadRequest("Thresher not exist");
            }

            var thresherData = gameData.Threshers.Get(thresherDef.key);

            var thresherLevelDef = thresherDef.level[thresherData.Level.Value - 1];

            if (thresherDef.level.Count <= thresherData.Level.Value - 1) {
                return BadRequest("At max level");
            }

            foreach (var (itemKey, requiredCount) in thresherLevelDef.items) {
                var currentCount = gameData.Storage.Lookup.Count(storageItem => storageItem.Item.Value.ItemKey == itemKey);

                if (currentCount < requiredCount) {
                    return BadRequest("No enough items in storage");
                }
            }

            foreach (var (itemKey, requiredCount) in thresherLevelDef.items) {
                for (var i = 0; i < requiredCount; i++) {
                    var itemToTake = gameData.Storage.Lookup.First(storageItem => storageItem.Item.Value.ItemKey == itemKey);
                    gameData.Storage.Lookup.Remove(itemToTake);
                }
            }

            thresherData.Level.Value += 1;

            return Ok;
        }
    }
}