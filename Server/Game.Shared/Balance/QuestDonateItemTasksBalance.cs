namespace Game.Shared.Balance {
    using System.Collections.Generic;
    using Defs;
    using Multicast;
    using UserProfile.Data;
    using UserProfile.Data.Quests;

    public class QuestDonateItemTasksBalance {
        private readonly GameDef       gameDef;
        private readonly SdUserProfile userProfile;

        public QuestDonateItemTasksBalance(GameDef gameDef, SdUserProfile userProfile) {
            this.gameDef     = gameDef;
            this.userProfile = userProfile;
        }

        public QuestDonateItemTaskDef GetDef(string taskKey) {
            return this.gameDef.QuestDonateItemTasks.Get(taskKey);
        }

        public SdQuestDonateItemTaskStates GetState(string taskKey) {
            return this.userProfile.QuestDonateItemTasks.Lookup.TryGetValue(taskKey, out var data)
                ? data.State.Value
                : SdQuestDonateItemTaskStates.Locked;
        }

        public bool IsRevealed(string taskKey) {
            return this.GetState(taskKey) is SdQuestDonateItemTaskStates.Revealed or SdQuestDonateItemTaskStates.Completed;
        }

        public bool IsCompleted(string taskKey) {
            return this.GetState(taskKey) is SdQuestDonateItemTaskStates.Completed;
        }

        public bool TryFindItemToDonateInStorage(string taskKey, out string itemGuid) {
            foreach (var storageItem in this.userProfile.Storage.Lookup) {
                if (!this.IsStorageItemCanBeDonated(taskKey, storageItem.Item.Value.ItemGuid)) {
                    continue;
                }

                itemGuid = storageItem.ItemGuid;
                return true;
            }

            itemGuid = null;
            return false;
        }

        public bool IsStorageItemCanBeDonated(string taskKey, string storageItemGuid) {
            if (!this.userProfile.Storage.Lookup.TryGetValue(storageItemGuid, out var storageItem)) {
                return false;
            }

            var itemSetupToDonateKey = this.GetDef(taskKey).itemSetup;
            var itemSetupBalance     = new ItemSetupBalance(this.gameDef, this.userProfile);

            return itemSetupBalance.IsMatch(itemSetupToDonateKey, storageItem.Item.Value);
        }


        public IEnumerable<string> EnumerateTasksForQuest(string questKey) {
            foreach (var taskDef in this.gameDef.QuestDonateItemTasks.Items) {
                if (taskDef.quest == questKey) {
                    yield return taskDef.key;
                }
            }
        }
    }
}