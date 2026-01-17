namespace Game.Shared.UserProfile.Commands.MailBox {
    using System;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;
    using Rewards;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileMailBoxClaimMessageCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public string MailMessageGuid;
    }

    public class UserProfileMailBoxClaimMessageCommandHandler : UserProfileServerCommandHandler<UserProfileMailBoxClaimMessageCommand> {
        private readonly GameDef gameDef;

        public UserProfileMailBoxClaimMessageCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileMailBoxClaimMessageCommand command) {
            if (!gameData.MailBox.Messages.TryGetValue(command.MailMessageGuid, out var message)) {
                return BadRequest("Mail message not exist");
            }

            if (message.Claimed.Value) {
                return BadRequest("Mail message already claimed");
            }

            message.Claimed.Value = true;

            var rewardGuid = Guid.NewGuid().ToString();

            await context.Execute(new UserProfileQueueRewardCommand {
                RewardGuid = rewardGuid,
                Reward     = message.Reward.Value,
            });

            await context.Execute(new UserProfileOpenRewardCommand {
                RewardGuid = rewardGuid,
            });

            return Ok;
        }
    }
}