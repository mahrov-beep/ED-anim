namespace Game.Shared.UserProfile.Commands.Rewards {
    using System;
    using System.Collections.Generic;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;
    using Impl;
    using Multicast.Numerics;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileOpenRewardCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public string RewardGuid;
    }

    public class UserProfileOpenRewardCommandHandler : UserProfileServerCommandHandler<UserProfileOpenRewardCommand> {
        private readonly Dictionary<string, Func<UserProfileServerCommandContext, Reward, Task>> handles = new() {
            [SharedConstants.RewardTypes.CURRENCY] = (ctx, reward) => ctx.Execute(new UserProfileOpenCurrencyRewardCommand { Reward = reward }),
            [SharedConstants.RewardTypes.ITEM]     = (ctx, reward) => ctx.Execute(new UserProfileOpenItemRewardCommand { Reward     = reward }),
            [SharedConstants.RewardTypes.EXP]      = (ctx, reward) => ctx.Execute(new UserProfileOpenExpRewardCommand { Reward      = reward }),
            [SharedConstants.RewardTypes.FEATURE]  = (ctx, reward) => ctx.Execute(new UserProfileOpenFeatureRewardCommand { Reward  = reward }),
        };

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileOpenRewardCommand command) {
            if (string.IsNullOrEmpty(command.RewardGuid)) {
                return BadRequest("Reward guid is empty");
            }

            if (!gameData.Rewards.Contains(command.RewardGuid)) {
                return BadRequest("Reward not exist");
            }

            var mainReward = gameData.Rewards.Dequeue(command.RewardGuid).Reward;
            var rewards    = mainReward.EnumerateAllRewards();

            foreach (var reward in rewards) {
                if (reward.IsNone) {
                    continue;
                }

                if (this.handles.TryGetValue(reward.GetItemType(), out var handler)) {
                    await handler.Invoke(context, reward);
                }
                else {
                    throw new InvalidOperationException($"No drop handler for '{reward.GetItemType()}' reward");
                }
            }

            return Ok;
        }
    }
}