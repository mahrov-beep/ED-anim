namespace Quantum {
    using System;
    using System.Linq;
    using Game.Shared;
    using Game.Shared.UserProfile.Data;
    using MessagePack;
    using Multicast.Numerics;

    [MessagePackObject, RequireFieldsInit]
    public class GameResultsTeamDeathMatch : GameResults {
        [Key(1)] public int                 WinnerTeamNumber;
        [Key(2)] public bool                IsLoadoutLost;
        [Key(3)] public Reward              Reward;
        [Key(4)] public GameSnapshotLoadout Loadout;
        [Key(5)] public int                 Kills;

        public override bool                GetIsLoadoutLost() => this.IsLoadoutLost;
        public override Reward              GetReward()        => this.Reward;
        public override GameSnapshotLoadout GetLoadout()       => this.Loadout;
        public override int                 GetKills()         => this.Kills;

        public new static GameResultsTeamDeathMatch FromGameSnapshot(
            GameDef gameDef, SdUserProfile useProfile, GameSnapshot snapshot, string gameId, Guid userId) {
            var winnerTeamId = snapshot.Users
                .GroupBy(it => it.GameTeamId)
                .Select(it => (teamId: it.Key, teamGrags: it.Sum(u => u.Frags)))
                .OrderByDescending(it => it.teamGrags)
                .Select(it => it.teamId)
                .First();

            var user = snapshot.Users.First(it => it.UserId == userId);

            var isLoadoutLost = snapshot.IsGameEnded == false ||
                                user.GameTeamId != winnerTeamId;

            if (ServerConfig.DisableLoadoutLost) {
                isLoadoutLost = false;
            }

            var reward = snapshot.IsGameEnded || user.GameTeamId != winnerTeamId
                ? Reward.None
                : Reward.Int(SharedConstants.RewardTypes.CURRENCY, SharedConstants.Game.Currencies.BADGES, 10);

            return new GameResultsTeamDeathMatch {
                WinnerTeamNumber = winnerTeamId,
                Loadout          = user.Loadout,
                IsLoadoutLost    = isLoadoutLost,
                Reward           = reward,
                Kills            = user.Frags,
            };
        }
    }
}