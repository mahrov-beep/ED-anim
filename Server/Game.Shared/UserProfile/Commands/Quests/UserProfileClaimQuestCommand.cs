namespace Game.Shared.UserProfile.Commands.Quests {
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Balance;
    using Data;
    using Data.MailBox;
    using Data.Quests;
    using MailBox;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileClaimQuestCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public string QuestKey;
    }

    public class UserProfileClaimQuestCommandHandler : UserProfileServerCommandHandler<UserProfileClaimQuestCommand> {
        private readonly GameDef gameDef;

        public UserProfileClaimQuestCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileClaimQuestCommand command) {
            if (!this.gameDef.Quests.TryGet(command.QuestKey, out var questDef)) {
                return BadRequest("Quest not exist");
            }

            var questKey = questDef.key;

            var questBalance = new QuestsBalance(this.gameDef, gameData);

            if (!questBalance.CanBeCompleted(questKey)) {
                return BadRequest("Quest cannot be completed");
            }

            var questData = gameData.Quests.Lookup.GetOrCreate(questKey, out _);

            questData.State.Value = SdQuestStates.Completed;

            await context.Execute(new UserProfileMailBoxQueueRewardMessageCommand {
                Type   = SdMailBoxMessageTypes.QuestReward,
                Reward = questData.Reward.Value,
            });

            return Ok;
        }
    }
}