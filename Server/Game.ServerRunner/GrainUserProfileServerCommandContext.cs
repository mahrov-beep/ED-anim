namespace Game.ServerRunner;

using System;
using System.Threading.Tasks;
using Grains;
using Shared.UserProfile;

public class GrainUserProfileServerCommandContext(
    Func<IUserProfileServerCommand, Task> commandExecutor,
    IUserProfileGrain userProfileGrainReference
) : UserProfileServerCommandContext(commandExecutor) {
    public IUserProfileGrain UserProfileGrainReference { get; } = userProfileGrainReference;
}