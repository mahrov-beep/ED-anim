namespace Game.Shared.UserProfile {
    using Multicast;
    using Data;

    public abstract class UserProfileServerCommandHandler<TCommand>
        : ServerCommandHandler<UserProfileServerCommandContext, SdUserProfile, TCommand>
        where TCommand : class, IUserProfileServerCommand {
    }
}