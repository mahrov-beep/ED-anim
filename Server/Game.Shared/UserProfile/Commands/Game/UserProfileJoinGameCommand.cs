namespace Game.Shared.UserProfile.Commands.Game {
    using MessagePack;
    using Multicast;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileJoinGameCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public string GameId;
    }
}