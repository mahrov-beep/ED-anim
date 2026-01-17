namespace Game.Shared.UserProfile.Commands.Rewards {
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;
    using Multicast.Numerics;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileQueueDropRewardCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public string RewardGuid;
        [Key(1)] public Reward Reward;
    }

    public class UserProfileQueueDropRewardCommandHandler : UserProfileServerCommandHandler<UserProfileQueueDropRewardCommand> {
        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileQueueDropRewardCommand command) {
            await context.Execute(new UserProfileQueueRewardCommand() {
                RewardGuid = command.RewardGuid,
                Reward     = command.Reward,
            });

            return Ok;
        }
    }
}