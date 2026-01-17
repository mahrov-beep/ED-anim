namespace Game.Domain.Quests {
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using Multicast;
    using Multicast.Collections;
    using Shared;
    using Shared.Balance;
    using Shared.Defs;
    using Shared.UserProfile.Data;
    using Shared.UserProfile.Data.Quests;
    using UniMob;
    using UserData;

    public class QuestDonateItemTasksModel : KeyedSingleInstanceModel<QuestDonateItemTaskDef, SdQuestDonateItemTask, QuestDonateItemTaskModel> {
        [Inject] private GameDef       gameDef;
        [Inject] private SdUserProfile gameData;

        private QuestDonateItemTasksBalance Balance => new QuestDonateItemTasksBalance(this.gameDef, this.gameData);

        public QuestDonateItemTasksModel(Lifetime lifetime, LookupCollection<QuestDonateItemTaskDef> defs, SdUserProfile gameData)
            : base(lifetime, defs, gameData.QuestDonateItemTasks.Lookup) {
            this.AutoConfigureData = true;
        }

        public IEnumerable<QuestDonateItemTaskModel> EnumerateForQuest(string questKey) {
            foreach (var taskKey in this.Balance.EnumerateTasksForQuest(questKey)) {
                yield return this.Get(taskKey);
            }
        }
    }

    public class QuestDonateItemTaskModel : Model<QuestDonateItemTaskDef, SdQuestDonateItemTask> {
        [Inject] private GameDef       gameDef;
        [Inject] private SdUserProfile gameData;

        private QuestDonateItemTasksBalance Balance => new QuestDonateItemTasksBalance(this.gameDef, this.gameData);

        public QuestDonateItemTaskModel(Lifetime lifetime, QuestDonateItemTaskDef def, SdQuestDonateItemTask data) : base(lifetime, def, data) {
        }

        public int Notifier => this.CanBeDonated ? 1 : 0;

        public bool IsCompleted => this.Balance.IsCompleted(this.Key);
        public bool IsRevealed  => this.Balance.IsRevealed(this.Key);

        public bool CanBeDonated => this.Balance.GetState(this.Key) is SdQuestDonateItemTaskStates.Revealed &&
                                    !string.IsNullOrEmpty(this.PossibleStorageItemToDonateGuid);

        [Atom, CanBeNull]
        public string PossibleStorageItemToDonateGuid {
            get {
                if (this.Balance.TryFindItemToDonateInStorage(this.Key, out var storageItemGuid)) {
                    return storageItemGuid;
                }

                return null;
            }
        }
    }
}