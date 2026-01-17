namespace Game.ServerRunner.CommandHandlers.UserProfile;

using Db;
using Grains;
using Microsoft.EntityFrameworkCore;
using Multicast;
using Npgsql;
using Shared;
using Shared.UserProfile;
using Shared.UserProfile.Commands;
using Shared.UserProfile.Data;
public class UserProfileSetNickNameCommandHandler(
                GameDef gameDef,
                IDbContextFactory<GameDbContext> dbContextFactory,
                ILogger<UserProfileSetNickNameCommandHandler> logger)
                : UserProfileServerCommandHandler<UserProfileSetNickNameCommand> {

    public override async Task<ServerCommandResult> Execute(
                    UserProfileServerCommandContext context,
                    SdUserProfile sdUserProfile,
                    UserProfileSetNickNameCommand command) {

        if (context is not GrainUserProfileServerCommandContext grainContext) {
            return InternalError("Context must be a grain context");
        }

        var currentNick = sdUserProfile.NickName.Value;
        var newNick     = command.NewNickName?.Trim() ?? string.Empty;

        if (currentNick == newNick) {
            logger.LogWarning("No-op NicknameUpdate; ignored. Fix client to avoid redundant calls.");
            return Ok;
        }

        if (newNick.Length > UserProfileSetNickNameCommand.MaxLength) {
            return BadRequest("NickName too long");
        }

        if (newNick.Any(it => !UserProfileSetNickNameCommand.IsValidChar(it))) {
            return BadRequest("NickName contains disallowed characters");
        }

        if (string.IsNullOrWhiteSpace(newNick)) {
            return BadRequest("NickName must not be empty");
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();

        var userId = grainContext.UserProfileGrainReference.GetUserId();

        try {
            var affected = await db.UserProfiles
                            .Where(p => p.Id == userId)
                            .ExecuteUpdateAsync(setters => setters
                                            .SetProperty(p => p.NickName, newNick));

            if (affected <= 0) {
                return InternalError("User profile not found");
            }
        }
        catch (PostgresException pgEx) when (pgEx.SqlState == PostgresErrorCodes.UniqueViolation) {
            return BadRequest("NickName already taken");
        }
        catch (PostgresException pgEx) {
            logger.LogError(pgEx, "Unhandled PostgresException:");
            return BadRequest("NickName already taken");
        }
        catch (DbUpdateException dbEx) {
            logger.LogError(dbEx, "Unhandled DbUpdateException:");
            return BadRequest("NickName already taken");
        }

        sdUserProfile.NickName.Value = newNick;

        return Ok;
    }
}