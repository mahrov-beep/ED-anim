namespace Game.Shared.UserProfile.Commands.Quests {
    using System;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Balance;
    using Data;
    using Data.Quests;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileDonateItemQuestsCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public string QuestDonateItemTaskKey;
        [Key(1)] public string StorageItemToDonateGuid;
    }

    public class UserProfileDonateItemQuestsCommandHandler : UserProfileServerCommandHandler<UserProfileDonateItemQuestsCommand> {
        private readonly GameDef gameDef;

        public UserProfileDonateItemQuestsCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileDonateItemQuestsCommand command) {
            if (!this.gameDef.QuestDonateItemTasks.TryGet(command.QuestDonateItemTaskKey, out var taskDef)) {
                return BadRequest("QuestDonateItemTask not exist");
            }

            var balance = new QuestDonateItemTasksBalance(this.gameDef, gameData);

            if (balance.GetState(taskDef.key) != SdQuestDonateItemTaskStates.Revealed) {
                return BadRequest("DonateItemTask cannot be donated");
            }

            if (!balance.IsStorageItemCanBeDonated(taskDef.key, command.StorageItemToDonateGuid)) {
                return BadRequest("Passed item cannot be donated");
            }

            gameData.Storage.Lookup.Remove(command.StorageItemToDonateGuid);

            var taskData = gameData.QuestDonateItemTasks.Lookup.GetOrCreate(taskDef.key, out _);
            taskData.State.Value = SdQuestDonateItemTaskStates.Completed;

            return Ok;
        }
    }
}