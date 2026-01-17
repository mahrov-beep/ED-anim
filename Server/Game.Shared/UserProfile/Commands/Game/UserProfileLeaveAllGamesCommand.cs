namespace Game.Shared.UserProfile.Commands.Game {
    using MessagePack;
    using Multicast;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileLeaveAllGamesCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
    }
}