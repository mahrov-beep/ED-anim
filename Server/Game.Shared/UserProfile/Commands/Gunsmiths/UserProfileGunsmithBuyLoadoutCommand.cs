namespace Game.Shared.UserProfile.Commands.Gunsmiths {
    using System;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Currencies;
    using Data;
    using Loadouts;
    using Quantum;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileGunsmithBuyLoadoutCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public string GunsmithKey;
        [Key(1)] public string GunsmithLoadoutGuid;
    }

    public class UserProfileGunsmithBuyLoadoutCommandHandler : UserProfileServerCommandHandler<UserProfileGunsmithBuyLoadoutCommand> {
        private readonly GameDef gameDef;

        public UserProfileGunsmithBuyLoadoutCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileGunsmithBuyLoadoutCommand command) {
            if (!gameData.Gunsmiths.Lookup.TryGetValue(command.GunsmithKey, out var gunsmithData)) {
                return BadRequest("Gunsmith not exist");
            }

            if (!gunsmithData.Loadouts.TryGetValue(command.GunsmithLoadoutGuid, out var targetGunsmithLoadout)) {
                return BadRequest("Gunsmith loadout not exist");
            }

            var selectedLoadout = gameData.Loadouts.Get(gameData.Loadouts.SelectedLoadout.Value);

            if (!string.IsNullOrEmpty(selectedLoadout.LockedForGame.Value)) {
                return BadRequest("Cannot modify locked loadout");
            }

            var gunsmithLoadoutDef = this.gameDef.GunsmithLoadouts.Get(targetGunsmithLoadout.GunsmithLoadoutKey.Value);

            await context.Execute(new UserProfileCurrencyTakeAmountCommand {
                CurrencyToTake = gunsmithLoadoutDef.buyCost,
            });

            var prevLoadoutSkin = selectedLoadout.LoadoutSnapshot.Value.TryGetItemAtSlot(CharacterLoadoutSlots.Skin, out var outSkinItem)
                ? outSkinItem.DeepClone()
                : null;

            var newGuid    = Guid.NewGuid().ToString();
            var newLoadout = targetGunsmithLoadout.Loadout.Value.DeepClone(generateNewGuids: true);
            
            var newSdLoadout = gameData.Loadouts.Lookup.GetOrCreate(newGuid, out _);
            newSdLoadout.LoadoutSnapshot.Value = newLoadout;
            newSdLoadout.LockedForGame.Value   = string.Empty;

            // не может быть лодаута без скина
            if (!newLoadout.TryGetItemAtSlot(CharacterLoadoutSlots.Skin, out _)) {
                newLoadout.SetItemToSlot(CharacterLoadoutSlots.Skin, prevLoadoutSkin);
            }

            gameData.Loadouts.SelectedLoadout.Value = newGuid;
            
            return Ok;
        }
    }
}