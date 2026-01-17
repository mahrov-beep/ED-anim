namespace Game.Shared.UserProfile.Commands.MailBox {
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileMailBoxViewAllMessagesCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
    }

    public class UserProfileMailBoxViewAllMessagesCommandHandler : UserProfileServerCommandHandler<UserProfileMailBoxViewAllMessagesCommand> {
        private readonly GameDef gameDef;

        public UserProfileMailBoxViewAllMessagesCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileMailBoxViewAllMessagesCommand command) {
            foreach (var sdMessage in gameData.MailBox.Messages) {
                sdMessage.Viewed.Value = true;
            }

            return Ok;
        }
    }
}