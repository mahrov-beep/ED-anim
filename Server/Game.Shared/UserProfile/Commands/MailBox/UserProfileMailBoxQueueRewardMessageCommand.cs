namespace Game.Shared.UserProfile.Commands.MailBox {
    using System;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;
    using Data.MailBox;
    using Multicast.Numerics;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileMailBoxQueueRewardMessageCommand : IUserProfileServerCommand {
        [Key(0)] public Reward                Reward;
        [Key(1)] public SdMailBoxMessageTypes Type;
    }

    public class UserProfileMailBoxQueueRewardMessageCommandHandler : UserProfileServerCommandHandler<UserProfileMailBoxQueueRewardMessageCommand> {
        private readonly GameDef      gameDef;
        private readonly ITimeService timeService;

        public UserProfileMailBoxQueueRewardMessageCommandHandler(GameDef gameDef, ITimeService timeService) {
            this.gameDef     = gameDef;
            this.timeService = timeService;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileMailBoxQueueRewardMessageCommand command) {
            var messageGuid = Guid.NewGuid().ToString();
            var message     = gameData.MailBox.Messages.GetOrCreate(messageGuid, out _);

            message.Type.Value        = command.Type;
            message.ReceiveDate.Value = this.timeService.Now;
            message.Reward.Value      = command.Reward;
            message.Claimed.Value     = false;

            return Ok;
        }
    }
}