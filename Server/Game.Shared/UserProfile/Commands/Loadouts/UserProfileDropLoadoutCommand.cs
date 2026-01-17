namespace Game.Shared.UserProfile.Commands.Loadouts {
    using System;
    using System.Linq;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Data;
    using Quantum;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileDropLoadoutCommand : IUserProfileServerCommand {
        [Key(0)] public string LoadoutGuid;
    }

    public class UserProfileDropLoadoutCommandHandler : UserProfileServerCommandHandler<UserProfileDropLoadoutCommand> {
        private readonly GameDef gameDef;

        public UserProfileDropLoadoutCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileDropLoadoutCommand command) {
            if (!gameData.Loadouts.Lookup.TryGetValue(command.LoadoutGuid, out var loadoutData)) {
                return BadRequest("Loadout not exist");
            }

            var loadoutSnapshot = loadoutData.LoadoutSnapshot.Value;

            // все предметы нужно переложить в хранилище.
            // Аттачменты перекладываются вместе с предметами, поэтому их не добавляем.
            var modifiedItems = Enumerable.Empty<GameSnapshotLoadoutItem>()
                .Concat(loadoutSnapshot.SlotItems ?? Array.Empty<GameSnapshotLoadoutItem>())
                .Concat(loadoutSnapshot.TrashItems ?? Array.Empty<GameSnapshotLoadoutItem>())
                .Where(it => it != null)
                .Select(it => it.ItemGuid)
                .ToList();

            await context.Execute(new UserProfileModifyLoadoutCommand {
                LoadoutGuid       = command.LoadoutGuid,
                LoadoutSnapshot   = null,
                ModifiedItemGuids = modifiedItems,
            });

            return Ok;
        }
    }
}