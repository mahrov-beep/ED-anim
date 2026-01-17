namespace Quantum {
    using System;
    using Game.Shared;
    using Game.Shared.Balance;
    using Game.Shared.UserProfile.Commands.Rewards;
    using Game.Shared.UserProfile.Data;
    using MessagePack;
    using Multicast.Numerics;

    [MessagePackObject, RequireFieldsInit]
    public class GameResultsEscape : GameResults {
        [Key(2)] public bool                IsLoadoutLost;
        [Key(3)] public Reward              Reward;
        [Key(4)] public GameSnapshotLoadout Loadout;
        [Key(5)] public int                 Kills;

        public override bool                GetIsLoadoutLost() => this.IsLoadoutLost;
        public override Reward              GetReward()        => this.Reward;
        public override GameSnapshotLoadout GetLoadout()       => this.Loadout;
        public override int                 GetKills()         => this.Kills;

        public new static GameResultsEscape Initial(GameSnapshotLoadout loadout) {
            return new GameResultsEscape {
                Loadout       = loadout,
                IsLoadoutLost = ServerConfig.DisableLoadoutLost ? false : true,
                Reward = Reward.LootBox("game_reward", SharedConstants.Game.GameModes.INIT_GAME_MODE, new[] {
                    Reward.Int(SharedConstants.RewardTypes.CURRENCY, SharedConstants.Game.Currencies.RATING, -8),
                    Reward.Int(SharedConstants.RewardTypes.CURRENCY, SharedConstants.Game.Currencies.LOADOUT_TICKETS, 1),
                }),
                Kills = 0,
            };
        }

        public new static GameResultsEscape FromGameSnapshot(
            GameDef gameDef, SdUserProfile useProfile, GameSnapshot snapshot, string gameId, Guid userId) {
            var gameModeDef  = gameDef.GameModes.Get(snapshot.GameMode);
            var userLevelDef = gameDef.GetLevel(useProfile.Level.Value);

            var user = snapshot.Users.Find(it => it.UserId == userId);

            var isLoadoutLost = snapshot.IsGameEnded || user.IsDead;

            if (ServerConfig.DisableLoadoutLost) {
                isLoadoutLost = false;
            }

            var reward = snapshot.IsGameEnded || user.IsDead
                ? BuildLoseReward()
                : BuildWinReward();

            return new GameResultsEscape {
                Loadout       = user.Loadout,
                IsLoadoutLost = isLoadoutLost,
                Reward        = reward,
                Kills         = user.Frags,
            };

            Reward BuildWinReward() {
                var beforeBadges = useProfile.Loadouts.TryFindByLockedForGame(gameId, out var loadout)
                    ? ItemBalance.CalculateSellCost(gameDef, loadout.LoadoutSnapshot.Value)[SharedConstants.Game.Currencies.BADGES]
                    : 0;
                var afterBadges = ItemBalance.CalculateSellCost(gameDef, user.Loadout)[SharedConstants.Game.Currencies.BADGES];
                var earningsExp = Math.Max(0, afterBadges - beforeBadges) * gameModeDef.expForEarningsPercent / 100;

                return Reward.LootBox("game_reward", SharedConstants.Game.GameModes.INIT_GAME_MODE, new[] {
                    RewardBuildUtility.BuildExpOrNone(SharedConstants.Game.Exp.MATCH_PLAYED, gameModeDef.expForRaid),
                    RewardBuildUtility.BuildExpOrNone(SharedConstants.Game.Exp.KILLS, gameModeDef.expForKill * user.Frags),
                    RewardBuildUtility.BuildExpOrNone(SharedConstants.Game.Exp.EARNINGS, earningsExp),
                    Reward.Int(SharedConstants.RewardTypes.CURRENCY, SharedConstants.Game.Currencies.RATING, 20),
                    Reward.Int(SharedConstants.RewardTypes.CURRENCY, SharedConstants.Game.Currencies.LOADOUT_TICKETS, 1),
                });
            }

            Reward BuildLoseReward() {
                return Reward.LootBox("game_reward", SharedConstants.Game.GameModes.INIT_GAME_MODE, new[] {
                    RewardBuildUtility.BuildExpOrNone(SharedConstants.Game.Exp.MATCH_PLAYED, gameModeDef.expForLostRaid),
                    RewardBuildUtility.BuildExpOrNone(SharedConstants.Game.Exp.KILLS, gameModeDef.expForKillInLostRaid * user.Frags),
                    Reward.Int(SharedConstants.RewardTypes.CURRENCY, SharedConstants.Game.Currencies.RATING, -8),
                    Reward.Int(SharedConstants.RewardTypes.CURRENCY, SharedConstants.Game.Currencies.LOADOUT_TICKETS, 1),
                });
            }
        }
    }
}