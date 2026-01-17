namespace Game.Domain.Quests {
    using System;
    using System.Collections.Generic;
    using Multicast;
    using Multicast.Collections;
    using Shared;
    using Shared.Balance;
    using Shared.Defs;
    using Shared.UserProfile.Data;
    using Shared.UserProfile.Data.Quests;
    using UniMob;
    using UserData;

    public class QuestCounterTasksModel : KeyedSingleInstanceModel<QuestCounterTaskDef, SdQuestCounterTask, QuestCounterTaskModel> {
        [Inject] private GameDef       gameDef;
        [Inject] private SdUserProfile gameData;

        private QuestCounterTasksBalance Balance => new QuestCounterTasksBalance(this.gameDef, this.gameData);

        public QuestCounterTasksModel(Lifetime lifetime, LookupCollection<QuestCounterTaskDef> defs, SdUserProfile gameData)
            : base(lifetime, defs, gameData.QuestCounterTasks.Lookup) {
            this.AutoConfigureData = true;
        }

        public IEnumerable<QuestCounterTaskModel> EnumerateForQuest(string questKey) {
            foreach (var taskKey in this.Balance.EnumerateTasksForQuest(questKey)) {
                yield return this.Get(taskKey);
            }
        }
    }

    public class QuestCounterTaskModel : Model<QuestCounterTaskDef, SdQuestCounterTask> {
        [Inject] private GameDef       gameDef;
        [Inject] private SdUserProfile gameData;

        private QuestCounterTasksBalance Balance => new QuestCounterTasksBalance(this.gameDef, this.gameData);

        public QuestCounterTaskModel(Lifetime lifetime, QuestCounterTaskDef def, SdQuestCounterTask data) : base(lifetime, def, data) {
        }

        public string Quest => this.Def.quest;

        public bool IsRevealed => this.Balance.IsRevealed(this.Key);
        public bool IsUnlocked => this.Balance.IsUnlocked(this.Key);

        public int CounterValue => this.Balance.GetCounterValue(this.Key);
        public int TotalValue   => this.Balance.GetTotalValue(this.Key);
    }
}