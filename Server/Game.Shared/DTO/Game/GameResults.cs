namespace Quantum {
    using System;
    using Game.Shared;
    using Game.Shared.UserProfile.Data;
    using JetBrains.Annotations;
    using MessagePack;
    using Multicast.Numerics;

    // информация о том кто победил
    [Union(0, typeof(GameResultsDeathMatch))]
    [Union(1, typeof(GameResultsTeamDeathMatch))]
    [Union(2, typeof(GameResultsEscape))]
    public abstract class GameResults {
        public static GameResults Initial(GameSnapshotLoadout loadout, GameRules rule) {
            return rule switch {
                GameRules.Escape => GameResultsEscape.Initial(loadout),
                _ => throw new ArgumentException($"Unexpected game mode: {rule}"),
            };
        }

        public static GameResults FromGameSnapshot(
            GameDef gameDef, SdUserProfile useProfile, GameSnapshot snapshot, string gameId, Guid userId) {
            return snapshot.GameRule switch {
                GameRules.DeathMatch => GameResultsDeathMatch.FromGameSnapshot(gameDef, useProfile, snapshot, gameId, userId),
                GameRules.TeamDeathMatch => GameResultsTeamDeathMatch.FromGameSnapshot(gameDef, useProfile, snapshot, gameId, userId),
                GameRules.Escape => GameResultsEscape.FromGameSnapshot(gameDef, useProfile, snapshot, gameId, userId),
                _ => throw new ArgumentException($"Unexpected game mode: {snapshot.GameMode}"),
            };
        }

        [PublicAPI] public abstract bool                GetIsLoadoutLost();
        [PublicAPI] public abstract Reward              GetReward();
        [PublicAPI] public abstract GameSnapshotLoadout GetLoadout();
        [PublicAPI] public abstract int                 GetKills();
    }
}