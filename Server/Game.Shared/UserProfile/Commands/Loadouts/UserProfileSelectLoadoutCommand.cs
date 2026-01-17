namespace Game.Shared.UserProfile.Commands.Loadouts {
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileSelectLoadoutCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public string LoadoutGuid;
    }

    public class UserProfileSelectLoadoutCommandHandler : UserProfileServerCommandHandler<UserProfileSelectLoadoutCommand> {
        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileSelectLoadoutCommand command) {
            if (string.IsNullOrEmpty(command.LoadoutGuid)) {
                return BadRequest("LoadoutGuid is empty");
            }

            if (!gameData.Loadouts.Lookup.ContainsKey(command.LoadoutGuid)) {
                return BadRequest("Loadout not exist");
            }

            gameData.Loadouts.SelectedLoadout.Value = command.LoadoutGuid;

            return Ok;
        }
    }
}

