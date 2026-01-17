namespace Game.Shared.UserProfile.Commands.Rewards {
    using System;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;
    using Multicast.Numerics;

    [MessagePackObject, RequireFieldsInit(Optional = new[] { nameof(RewardGuid) })]
    public class UserProfileQueueRewardCommand : IUserProfileServerCommand {
        [Key(0)] public string RewardGuid;
        [Key(1)] public Reward Reward;
    }

    public class UserProfileQueueRewardCommandHandler : UserProfileServerCommandHandler<UserProfileQueueRewardCommand> {
        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileQueueRewardCommand command) {
            gameData.Rewards.CreateInternal(command.RewardGuid ?? Guid.NewGuid().ToString(), command.Reward);
            return Ok;
        }
    }
}