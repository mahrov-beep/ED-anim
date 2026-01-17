namespace Game.Shared.UserProfile.Commands.Quests {
    using System;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Balance;
    using Data;
    using Data.Quests;
    using Rewards;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileRevealQuestCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public string QuestKey;
    }

    public class UserProfileRevealQuestCommandHandler : UserProfileServerCommandHandler<UserProfileRevealQuestCommand> {
        private readonly GameDef gameDef;

        public UserProfileRevealQuestCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileRevealQuestCommand command) {
            if (!this.gameDef.Quests.TryGet(command.QuestKey, out var questDef)) {
                return BadRequest("Quest not exist");
            }

            var questKey = questDef.key;

            var questBalance             = new QuestsBalance(this.gameDef, gameData);
            var counterQuestTasksBalance = new QuestCounterTasksBalance(this.gameDef, gameData);
            var donateItemTasksBalance   = new QuestDonateItemTasksBalance(this.gameDef, gameData);

            if (!questBalance.CanBeRevealed(questKey)) {
                return BadRequest("Quest cannot be revealed");
            }

            var questData = gameData.Quests.Lookup.GetOrCreate(questKey, out _);

            questData.State.Value  = SdQuestStates.Revealed;
            questData.Reward.Value = RewardBuildUtility.BuildLootBox(SharedConstants.LootBoxTypes.CONGRATULATIONS, "quest", questDef.rewards);

            foreach (var taskKey in counterQuestTasksBalance.EnumerateTasksForQuest(questKey)) {
                var taskData = gameData.QuestCounterTasks.Lookup.GetOrCreate(taskKey, out _);
                taskData.Counter.Value = counterQuestTasksBalance.GetInitialValue(questKey);
            }
            
            foreach (var taskKey in donateItemTasksBalance.EnumerateTasksForQuest(questKey)) {
                var taskData = gameData.QuestDonateItemTasks.Lookup.GetOrCreate(taskKey, out _);
                taskData.State.Value = SdQuestDonateItemTaskStates.Revealed;
            }

            return Ok;
        }
    }
}