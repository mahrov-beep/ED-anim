namespace Game.Shared.Balance {
    using System;
    using System.Collections.Generic;
    using Defs;
    using Quantum;
    using UserProfile.Data;

    public readonly struct QuestCounterTasksBalance {
        private readonly GameDef       gameDef;
        private readonly SdUserProfile userProfile;

        public QuestCounterTasksBalance(GameDef gameDef, SdUserProfile userProfile) {
            this.gameDef     = gameDef;
            this.userProfile = userProfile;
        }

        public QuestCounterTaskDef GetDef(string taskKey) {
            return this.gameDef.QuestCounterTasks.Get(taskKey);
        }

        public QuestPropertyOperation GetCounterPropertyOperation(string taskKey) {
            return this.GetDef(taskKey).counterOperation;
        }

        public QuestCounterPropertyTypes GetCounterPropertyType(string taskKey) {
            return this.GetDef(taskKey).counterProperty;
        }

        public int GetInitialValue(string taskKey) {
            return 0;
        }

        public int GetCounterValue(string taskKey) {
            return this.userProfile.QuestCounterTasks.Lookup.TryGetValue(taskKey, out var data)
                ? data.Counter.Value
                : -1;
        }

        public int GetTotalValue(string taskKey) {
            return this.GetDef(taskKey).counterValue;
        }

        public bool IsRevealed(string taskKey) {
            return this.GetCounterValue(taskKey) >= 0;
        }

        public bool IsUnlocked(string taskKey) {
            return this.GetCounterValue(taskKey) >= this.GetTotalValue(taskKey);
        }

        public bool IsMatchFilters(string taskKey, QuestTaskFilters[] filters) {
            foreach (var requiredFilter in this.GetDef(taskKey).counterFilters) {
                if (Array.IndexOf(filters, requiredFilter) == -1) {
                    return false;
                }
            }

            return true;
        }

        public bool IsMatchResetTrigger(string taskKey, QuestCounterPropertyTypes trigger) {
            return this.GetDef(taskKey).resetByTriggers.Contains(trigger);
        }

        public IEnumerable<string> EnumerateTasksForQuest(string questKey) {
            foreach (var taskDef in this.gameDef.QuestCounterTasks.Items) {
                if (taskDef.quest == questKey) {
                    yield return taskDef.key;
                }
            }
        }

        public void TryApplyNewValue(string taskKey, int newCounter, QuestTaskFilters[] filters = null) {
            if (this.IsUnlocked(taskKey) || !this.IsRevealed(taskKey)) {
                return;
            }

            if (filters != null && !this.IsMatchFilters(taskKey, filters)) {
                return;
            }

            newCounter = this.GetCounterPropertyOperation(taskKey) switch {
                QuestPropertyOperation.Increment => this.GetCounterValue(taskKey) + 1,
                QuestPropertyOperation.Add => this.GetCounterValue(taskKey) + newCounter,
                QuestPropertyOperation.Max => Math.Max(this.GetCounterValue(taskKey), newCounter),
                _ => 0,
            };

            var data = this.userProfile.QuestCounterTasks.Lookup.GetOrCreate(taskKey, out _);
            data.Counter.Value = Math.Min(newCounter, this.GetTotalValue(taskKey));
        }

        public void TryReset(string taskKey, QuestCounterPropertyTypes byTrigger) {
            if (this.IsUnlocked(taskKey) || !this.IsRevealed(taskKey)) {
                return;
            }

            if (!this.IsMatchResetTrigger(taskKey, byTrigger)) {
                return;
            }

            var data = this.userProfile.QuestCounterTasks.Lookup.GetOrCreate(taskKey, out _);
            data.Counter.Value = 0;
        }

        public void ForceLock(string taskKey) {
            if (!this.userProfile.QuestCounterTasks.Lookup.TryGetValue(taskKey, out var data)) {
                return;
            }

            data.Counter.Value = -1;
        }

        public void ForceUnlock(string taskKey) {
            var data = this.userProfile.QuestCounterTasks.Lookup.GetOrCreate(taskKey, out _);
            data.Counter.Value = this.GetTotalValue(taskKey);
        }
    }
}