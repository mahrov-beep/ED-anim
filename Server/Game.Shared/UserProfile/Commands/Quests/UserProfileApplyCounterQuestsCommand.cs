namespace Game.Shared.UserProfile.Commands.Quests {
    using System;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Balance;
    using Data;
    using Defs;
    using Quantum;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileApplyCounterQuestsCommand : IUserProfileServerCommand {
        [Key(0)] public QuestCounterPropertyTypes Property;
        [Key(1)] public QuestTaskFilters[]        Filters;
        [Key(2)] public int                       Value;
    }

    public class UserProfileApplyCounterQuestsCommandHandler : UserProfileServerCommandHandler<UserProfileApplyCounterQuestsCommand> {
        private readonly GameDef gameDef;

        public UserProfileApplyCounterQuestsCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileApplyCounterQuestsCommand command) {
            var questCounterTasksBalance = new QuestCounterTasksBalance(this.gameDef, gameData);

            foreach (var taskData in gameData.QuestCounterTasks.Lookup) {
                if (questCounterTasksBalance.GetCounterPropertyType(taskData.Key) != command.Property) {
                    continue;
                }

                questCounterTasksBalance.TryApplyNewValue(taskData.Key, command.Value, command.Filters);
            }

            foreach (var taskData in gameData.QuestCounterTasks.Lookup) {
                questCounterTasksBalance.TryReset(taskData.Key, byTrigger: command.Property);
            }

            return Ok;
        }
    }
}