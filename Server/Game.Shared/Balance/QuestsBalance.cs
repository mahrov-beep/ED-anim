namespace Game.Shared.Balance {
    using System;
    using System.Collections.Generic;
    using Defs;
    using UserProfile.Commands.Rewards;
    using UserProfile.Data;
    using UserProfile.Data.Quests;

    public readonly struct QuestsBalance {
        private readonly GameDef       gameDef;
        private readonly SdUserProfile userProfile;

        public QuestsBalance(GameDef gameDef, SdUserProfile userProfile) {
            this.gameDef     = gameDef;
            this.userProfile = userProfile;
        }

        public QuestDef GetDef(string questKey) {
            return this.gameDef.Quests.Get(questKey);
        }


        public List<string> GetPrevQuests(string questKey) {
            return this.GetDef(questKey).prevQuests;
        }

        public SdQuestStates GetState(string questKey) {
            return this.userProfile.Quests.Lookup.TryGetValue(questKey, out var data)
                ? data.State.Value
                : SdQuestStates.Locked;
        }

        public bool CanBeRevealed(string questKey) {
            if (this.GetState(questKey) != SdQuestStates.Locked) {
                return false;
            }

            foreach (var prevQuest in this.GetPrevQuests(questKey)) {
                if (this.GetState(prevQuest) != SdQuestStates.Completed) {
                    return false;
                }
            }

            return true;
        }

        public bool CanBeCompleted(string questKey) {
            if (this.GetState(questKey) != SdQuestStates.Revealed) {
                return false;
            }

            var counterQuestTasksBalance = new QuestCounterTasksBalance(this.gameDef, userProfile);

            foreach (var taskKey in counterQuestTasksBalance.EnumerateTasksForQuest(questKey)) {
                if (!counterQuestTasksBalance.IsUnlocked(taskKey)) {
                    return false;
                }
            }

            var donateItemTasksBalance = new QuestDonateItemTasksBalance(this.gameDef, this.userProfile);
            foreach (var taskKey in donateItemTasksBalance.EnumerateTasksForQuest(questKey)) {
                if (donateItemTasksBalance.GetState(taskKey) != SdQuestDonateItemTaskStates.Completed) {
                    return false;
                }
            }

            return true;
        }
    }
}