namespace Game.ServerRunner.CommandHandlers.UserProfile;

using Multicast;
using System.Threading.Tasks;
using Grains;
using Multicast.Numerics;
using Quantum;
using Shared;
using Shared.UserProfile;
using Shared.UserProfile.Commands.Game;
using Shared.UserProfile.Data;

public class UserProfileJoinGameCommandHandler(
    IGrainFactory grainFactory,
    GameDef gameDef
) : UserProfileServerCommandHandler<UserProfileJoinGameCommand> {
    public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileJoinGameCommand command) {
        if (context is not GrainUserProfileServerCommandContext grainContext) {
            return InternalError("Context must be a grain context");
        }

        if (string.IsNullOrEmpty(command.GameId)) {
            return BadRequest("GameId must be not empty");
        }

        if (!gameDef.GameModes.TryGet(gameData.GameModes.SelectedGameMode.Value, out var gameModeDef)) {
            return BadRequest("Invalid game mode");
        }

        if (!gameData.Loadouts.Lookup.TryGetValue(gameData.Loadouts.SelectedLoadout.Value, out var selectedLoadout)) {
            return BadRequest("Selected loadout not valid");
        }

        if (!string.IsNullOrEmpty(selectedLoadout.LockedForGame.Value) &&
            selectedLoadout.LockedForGame.Value != command.GameId) {
            return BadRequest("Selected loadout locked for another game");
        }

        selectedLoadout.LockedForGame.Value = command.GameId;

        var playedGameData = gameData.PlayedGames.Create(command.GameId);
        playedGameData.IsPlaying.Value     = true;
        playedGameData.StartTime.Value     = GameTime.FromUtcDateTime_UNSAFE(DateTime.UtcNow);
        playedGameData.RewardClaimed.Value = false;
        playedGameData.GameResult.Value    = GameResults.Initial(selectedLoadout.LoadoutSnapshot.Value, gameModeDef.gameRule);

        await grainFactory
            .GetGameGrain(command.GameId)
            .AddPlayer(grainContext.UserProfileGrainReference);

        return Ok;
    }
}