namespace Game.ServerRunner.CommandHandlers.UserProfile;

using System.Collections.Immutable;
using Multicast;
using System.Threading.Tasks;
using Grains;
using Quantum;
using Shared;
using Shared.DTO;
using Shared.UserProfile;
using Shared.UserProfile.Commands.Game;
using Shared.UserProfile.Data;

public class UserProfileApplyGameResultsCommandHandler(GameDef gameDef) : UserProfileServerCommandHandler<UserProfileApplyGameResultsCommand> {
    public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileApplyGameResultsCommand command) {
        if (context is not GrainUserProfileServerCommandContext grainContext) {
            return InternalError("Context must be a grain context");
        }

        if (string.IsNullOrEmpty(command.GameId)) {
            return BadRequest("GameId is empty");
        }

        if (command.GameSnapshot == null) {
            return BadRequest("GameSnapshot is null");
        }

        if (!gameData.PlayedGames.TryGetValue(command.GameId, out var playedGameData)) {
            return BadRequest("Played game data not exist");
        }

        if (playedGameData.IsPlaying.Value == false) {
            return BadRequest("Trying to apply game result for non playing game");
        }

        var resultForUser = GameResults.FromGameSnapshot(
            gameDef, gameData, command.GameSnapshot, command.GameId, grainContext.UserProfileGrainReference.GetUserId());

        playedGameData.IsPlaying.Value  = false;
        playedGameData.GameResult.Value = resultForUser;

        return Ok;
    }
}