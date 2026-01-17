namespace Game.Shared.UserProfile.Commands.Game {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;
    using Data.MailBox;
    using Loadouts;
    using Quantum;
    using Rewards;
    using MailBox;
    using Helpers;
    using Multicast.Numerics;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileConfirmGameResultCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public string GameId;
    }

    public class UserProfileConfirmGameResultCommandHandler : UserProfileServerCommandHandler<UserProfileConfirmGameResultCommand> {
        private readonly GameDef gameDef;

        public UserProfileConfirmGameResultCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileConfirmGameResultCommand command) {
            if (string.IsNullOrEmpty(command.GameId)) {
                return BadRequest("GameId is empty");
            }

            if (!gameData.PlayedGames.TryGetValue(command.GameId, out var sdGameResult)) {
                return BadRequest("Confirmed game not exist");
            }

            if (sdGameResult.IsPlaying.Value == true) {
                return BadRequest("Trying to confirm yet playing game");
            }

            if (sdGameResult.RewardClaimed.Value) {
                return BadRequest("Confirmed game reward already claimed");
            }

            var gameResult = sdGameResult.GameResult.Value;

            GameSnapshotLoadoutItem[] trashItems = default;
            GameSnapshotLoadoutItem   safe       = default;
            
            if (gameData.Loadouts.TryFindByLockedForGame(sdGameResult.GameId, out var usedLoadout)) {
                var isLost = gameResult.GetIsLoadoutLost();
                if (ServerConfig.DisableLoadoutLost) {
                    isLost = false;
                }

                if (isLost) {
                    var loadoutValue = gameResult.GetLoadout();
                    
                    if (loadoutValue.SlotItems != null) {
                        safe = loadoutValue.SlotItems[CharacterLoadoutSlots.Safe.ToInt()];
                    }

                    if (loadoutValue.TrashItems != null) {
                        var trashItemsList = new List<GameSnapshotLoadoutItem>();
                        
                        foreach (var trash in loadoutValue.TrashItems) {
                            if (string.IsNullOrEmpty(trash.SafeGuid) && !trash.AddToLoadoutAfterFail) {
                                continue;
                            }
                            
                            trashItemsList.Add(trash);
                        }

                        trashItems = trashItemsList.Count > 0 ? trashItemsList.ToArray() : default;
                    }

                    gameData.Loadouts.Lookup.Remove(usedLoadout);
                }
                else {
                    usedLoadout.LockedForGame.Value   = string.Empty;
                    usedLoadout.LoadoutSnapshot.Value = gameResult.GetLoadout();
                }
            }

            if (gameData.Loadouts.Lookup.Count > 0) {
                var nextLoadout = gameData.Loadouts.Lookup.First();

                var rewards = new List<Reward>();

                if (safe != null && !string.IsNullOrEmpty(safe.ItemKey)) {
                    rewards.Add(RewardBuildUtility.BuildItem(safe.ItemKey));
                }

                if (trashItems != null && safe != null && !string.IsNullOrEmpty(safe.ItemGuid)) {
                    foreach (var trashItem in trashItems) {
                        if (trashItem == null) {
                            continue;
                        }

                        if (!string.IsNullOrEmpty(trashItem.SafeGuid) && trashItem.SafeGuid == safe.ItemGuid) {
                            rewards.Add(RewardBuildUtility.BuildItem(trashItem.ItemKey));
                        }
                    }
                }

                if (rewards.Count > 0) {
                    await context.Execute(new UserProfileMailBoxQueueRewardMessageCommand {
                        Type   = SdMailBoxMessageTypes.LootBoxReward,
                        Reward = RewardBuildUtility.Combine("safe_return", rewards.ToArray()),
                    });
                }

                gameData.Loadouts.SelectedLoadout.Value = nextLoadout.Guid;
            }
            else {
                await context.Execute(new EnsureInitialLoadoutCommand {
                    LoadoutKey         = SharedConstants.Game.PlayerLoadouts.BASE,
                    CreateNewIfNotExist = true,
                    Safe                = safe,
                    TrashItems          = trashItems,
                });
            }

            var rewardGuid = Guid.NewGuid().ToString();

            await context.Execute(new UserProfileQueueRewardCommand {
                RewardGuid = rewardGuid,
                Reward     = gameResult.GetReward(),
            });
            await context.Execute(new UserProfileOpenRewardCommand {
                RewardGuid = rewardGuid,
            });

            sdGameResult.RewardClaimed.Value = true;

            // store only up to 5 games history to reduce user profile size
            if (gameData.PlayedGames.Count > 5) {
                var toDelete = gameData.PlayedGames
                    .OrderBy(it => it.StartTime.Value.AsDateTime)
                    .First();

                gameData.PlayedGames.Remove(toDelete);
            }

            return Ok;
        }
    }
}