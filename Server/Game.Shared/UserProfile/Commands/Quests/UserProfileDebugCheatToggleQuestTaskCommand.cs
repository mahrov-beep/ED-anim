#if DEBUG
namespace Game.Shared.UserProfile.Commands.Quests {
    using System.Threading.Tasks;
    using Balance;
    using Data;
    using Data.Quests;
    using MessagePack;
    using Multicast;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileDebugCheatToggleQuestTaskCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public string QuestTaskKey;
    }

    public class UserProfileDebugCheatToggleQuestTaskCommandHandler : UserProfileServerCommandHandler<UserProfileDebugCheatToggleQuestTaskCommand> {
        private readonly GameDef gameDef;

        public UserProfileDebugCheatToggleQuestTaskCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileDebugCheatToggleQuestTaskCommand command) {
            if (string.IsNullOrWhiteSpace(command.QuestTaskKey)) {
                return BadRequest("QuestTaskKey is empty");
            }

            if (this.gameDef.QuestCounterTasks.TryGet(command.QuestTaskKey, out var counterTaskDef)) {
                var taskKey = counterTaskDef.key;
                var balance = new QuestCounterTasksBalance(this.gameDef, gameData);
                var total   = balance.GetTotalValue(taskKey);
                var data    = gameData.QuestCounterTasks.Lookup.GetOrCreate(taskKey, out _);

                data.Counter.Value = data.Counter.Value >= total ? 0 : total;
                return Ok;
            }

            if (this.gameDef.QuestDonateItemTasks.TryGet(command.QuestTaskKey, out var donateTaskDef)) {
                var taskKey = donateTaskDef.key;
                var data    = gameData.QuestDonateItemTasks.Lookup.GetOrCreate(taskKey, out _);

                data.State.Value = data.State.Value == SdQuestDonateItemTaskStates.Completed
                    ? SdQuestDonateItemTaskStates.Revealed
                    : SdQuestDonateItemTaskStates.Completed;
                return Ok;
            }

            return BadRequest("Quest task not exist");
        }
    }
}
#endif
