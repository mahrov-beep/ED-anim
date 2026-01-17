namespace Game.Domain.ExpProgressionRewards {
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using Multicast;
    using Multicast.Collections;
    using Multicast.Numerics;
    using Shared;
    using Shared.Defs;
    using Shared.UserProfile.Commands.Rewards;
    using Shared.UserProfile.Data;
    using Shared.UserProfile.Data.ExpProgressionRewards;
    using Sirenix.OdinInspector;
    using UniMob;
    using UnityEngine;

    public class ExpProgressionRewardsModel : KeyedSingleInstanceModel<ExpProgressionRewardDef, SdExpProgressionReward, ExpProgressionRewardModel> {
        [Inject] private GameDef       gameDef;
        [Inject] private SdUserProfile userProfile;

        [Atom] private ExpProgressionRewardModel UserSelected { get; set; }

        public ExpProgressionRewardsModel(Lifetime lifetime, LookupCollection<ExpProgressionRewardDef> defs, SdUserProfile gameData)
            : base(lifetime, defs, gameData.ExpProgressionRewards.Lookup) {
            this.AutoConfigureData = true;
        }

        [Atom] public List<ExpProgressionRewardModel> All => this.Values.OrderBy(it => it.LevelToComplete).ToList();

        [Atom]            public List<ExpProgressionRewardModel> AllUnlocked  => this.All.Where(static it => it.IsUnlocked).ToList();
        [Atom, CanBeNull] public ExpProgressionRewardModel       LastUnlocked => this.AllUnlocked.LastOrDefault();
        [Atom, CanBeNull] public ExpProgressionRewardModel       FirstLocked  => this.All.FirstOrDefault(static it => it.IsUnlocked == false);

        [Atom, CanBeNull] public ExpProgressionRewardModel FirstLockedWithReward => this.All
            .FirstOrDefault(static it => it.IsUnlocked == false && it.RewardsCount > 0);

        [Atom] public List<Reward> RewardsPreviewForMainMenu => this.All
            .Where(static it => it.IsUnlocked == false)
            .SelectMany(static it => it.RewardsPreview)
            .Take(3)
            .ToList();

        [Atom] public ExpProgressionRewardModel Selected {
            get => this.UserSelected ?? this.FirstLocked;
            set => this.UserSelected = value;
        }

        public bool TryGetLevelUp(out LevelDef currentLevelDef, out LevelDef nextLevelDef) {
            currentLevelDef = this.gameDef.GetLevel(this.userProfile.Level.Value);
            nextLevelDef    = this.gameDef.GetLevel(this.userProfile.Level.Value + 1);

            var atMaxLevel   = currentLevelDef.level == nextLevelDef.level;
            var hasEnoughExp = this.userProfile.Exp.Value >= currentLevelDef.expToNextLevel;

            return !atMaxLevel && hasEnoughExp;
        }
    }

    public class ExpProgressionRewardModel : Model<ExpProgressionRewardDef, SdExpProgressionReward> {
        [Inject] private SdUserProfile userProfile;

        public ExpProgressionRewardModel(Lifetime lifetime, ExpProgressionRewardDef def, SdExpProgressionReward data) : base(lifetime, def, data) {
        }

        [ShowInInspector] private string InspectorKey => this.Key;

        [ShowInInspector] public int LevelToComplete => this.Def.levelToComplete;
        [ShowInInspector] public int RewardsCount    => this.Def.rewards.Count;

        public float PlacesTakenInRewardsRow => Mathf.Max(0.5f, this.RewardsCount);

        public bool IsUnlocked => this.userProfile.Level.Value > this.LevelToComplete;
        public bool IsClaimed  => this.Data.Claimed.Value;

        [Atom] public List<Reward> RewardsPreview => this.Def.rewards.Select(static it => RewardBuildUtility.Build(it)).ToList();
    }
}