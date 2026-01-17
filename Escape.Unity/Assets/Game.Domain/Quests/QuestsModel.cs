namespace Game.Domain.Quests {
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using Multicast;
    using Multicast.Collections;
    using Multicast.Numerics;
    using Shared;
    using Shared.Balance;
    using Shared.Defs;
    using Shared.UserProfile.Commands.Rewards;
    using Shared.UserProfile.Data;
    using Shared.UserProfile.Data.Quests;
    using UniMob;

    public class QuestsModel : KeyedSingleInstanceModel<QuestDef, SdQuest, QuestModel> {
        public QuestsModel(Lifetime lifetime, LookupCollection<QuestDef> defs, SdUserProfile gameData)
            : base(lifetime, defs, gameData.Quests.Lookup) {
            this.AutoConfigureData = true;
        }

        [Atom] public List<QuestModel> QuestsVisibleInQuestMenu => this.Values.ToList();
        [Atom] public List<QuestModel> QuestsVisibleInGame      => this.Values.Where(it => it.IsVisibleInGame).ToList();

        [Atom, CanBeNull]
        public QuestModel QuestMenuInitialSelectedQuest => this.QuestsVisibleInQuestMenu
            .FirstOrDefault(it => it.CanBeRevealed || it.State is SdQuestStates.Revealed or SdQuestStates.Completed);
        
        [Atom] public int Notify => this.QuestsVisibleInQuestMenu.Sum(it => it.Notifier);
    }

    public class QuestModel : Model<QuestDef, SdQuest> {
        [Inject] private QuestDonateItemTasksModel questDonateItemTasksModel;
        
        [Inject] private GameDef       gameDef;
        [Inject] private SdUserProfile gameData;

        private QuestsBalance Balance => new QuestsBalance(this.gameDef, this.gameData);

        public QuestModel(Lifetime lifetime, QuestDef def, SdQuest data) : base(lifetime, def, data) {
        }

        [Atom] public bool IsVisibleInGame => this.Balance.GetState(this.Key) == SdQuestStates.Revealed;

        [Atom] public int Notifier => (this.CanBeRevealed || this.CanBeCompleted ? 1 : 0) +
                                      this.questDonateItemTasksModel.EnumerateForQuest(this.Key).Sum(t => t.Notifier);

        [Atom] public SdQuestStates State => this.Balance.GetState(this.Key);

        [Atom] public bool CanBeRevealed => this.Balance.CanBeRevealed(this.Key);

        [Atom] public bool   CanBeCompleted => this.Balance.CanBeCompleted(this.Key);

        public List<Reward> RewardsPreview => this.Data.Reward.Value switch {
            { AmountType: RewardAmountType.LootBox } lootBoxReward => lootBoxReward.LootBoxRewards.ToList(),
            { IsNone: false } reward => new List<Reward> { reward },
            _ => this.Def.rewards.Select(static it => RewardBuildUtility.Build(it)).ToList(),
        };
    }
}