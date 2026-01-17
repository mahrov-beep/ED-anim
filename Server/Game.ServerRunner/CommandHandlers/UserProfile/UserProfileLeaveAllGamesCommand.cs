namespace Game.ServerRunner.CommandHandlers.UserProfile;

using System.Threading.Tasks;
using Grains;
using Multicast;
using Orleans;
using Shared.UserProfile;
using Shared.UserProfile.Commands.Game;
using Shared.UserProfile.Data;

public class UserProfileLeaveAllGamesCommandHandler(
    IGrainFactory grainFactory
) : UserProfileServerCommandHandler<UserProfileLeaveAllGamesCommand> {
    public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileLeaveAllGamesCommand command) {
        foreach (var sdGame in gameData.PlayedGames) {
            if (sdGame.IsPlaying.Value == false) {
                continue;
            }

            sdGame.IsPlaying.Value = false;
        }

        // Clear matchmaking matched state when player explicitly leaves all games
        if (context is GrainUserProfileServerCommandContext grainContext) {
            await grainFactory.GetGrain<IMatchmakingGrain>("all")
                .ClearMatchedState(grainContext.UserProfileGrainReference.GetUserId());
        }

        return Ok;
    }
}

