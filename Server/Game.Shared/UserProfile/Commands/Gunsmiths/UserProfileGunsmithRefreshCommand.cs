namespace Game.Shared.UserProfile.Commands.Gunsmiths {
    using System;
    using System.Linq;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Balance;
    using Data;
    using Defs;
    using Quantum;
    using UserProfile.Helpers;

    [MessagePackObject, RequireFieldsInit]
    public class UserProfileGunsmithRefreshCommand : IUserProfileServerCommand, IServerCommandExecutableFromClient {
        [Key(0)] public string GunsmithKey;
    }

    public class UserProfileGunsmithRefreshCommandHandler : UserProfileServerCommandHandler<UserProfileGunsmithRefreshCommand> {
        private readonly GameDef gameDef;

        public UserProfileGunsmithRefreshCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, UserProfileGunsmithRefreshCommand command) {
            if (!this.gameDef.Gunsmiths.TryGet(command.GunsmithKey, out var gunsmithDef)) {
                return BadRequest("Gunsmith key is invalid");
            }

            var thresher      = gameData.Threshers.Get(gunsmithDef.thresher);
            var gunsmithLevel = thresher.Level.Value;
            var gunsmithData  = gameData.Gunsmiths.Get(gunsmithDef.key);

            // skip refresh if up-to-date
            if (gunsmithData.LastRefreshOnLevel.Value == gunsmithLevel) {
                return Ok;
            }

            var itemSetupBalance = new ItemSetupBalance(this.gameDef, gameData);

            foreach (var sdGunsmithLoadout in gunsmithData.Loadouts.ToList()) {
                gunsmithData.Loadouts.Remove(sdGunsmithLoadout);
            }

            var newLoadoutSnapshots = this.gameDef.GunsmithLoadouts.Items
                .Where(it => it.ownerGunsmith == gunsmithDef.key)
                .Where(it => gunsmithLevel >= it.minLevel && gunsmithLevel <= it.maxLevel)
                .Select(it => new { loadoutKey = it.key, loadout = MakeLoadoutSnapshotFromGunsmithLoadout(it, itemSetupBalance) })
                .ToList();

            foreach (var gameSnapshotLoadout in newLoadoutSnapshots) {
                var newLoadoutData = gunsmithData.Loadouts.Create(Guid.NewGuid().ToString());

                newLoadoutData.GunsmithLoadoutKey.Value = gameSnapshotLoadout.loadoutKey;
                newLoadoutData.Loadout.Value            = gameSnapshotLoadout.loadout;
            }

            gunsmithData.LastRefreshOnLevel.Value = gunsmithLevel;

            return Ok;
        }

        private GameSnapshotLoadout MakeLoadoutSnapshotFromGunsmithLoadout(GunsmithLoadoutDef def, ItemSetupBalance itemSetupBalance) {
            var slots = GameSnapshotLoadout.MakeLoadoutSlots(slots => {
                slots[CharacterLoadoutSlots.MeleeWeapon.ToInt()]     = itemSetupBalance.MakeItemOrNull(def.meleeWeapon);
                slots[CharacterLoadoutSlots.PrimaryWeapon.ToInt()]   = itemSetupBalance.MakeItemOrNull(def.primaryWeapon);
                slots[CharacterLoadoutSlots.SecondaryWeapon.ToInt()] = itemSetupBalance.MakeItemOrNull(def.secondaryWeapon);
                slots[CharacterLoadoutSlots.Backpack.ToInt()]        = itemSetupBalance.MakeItemOrNull(def.backpack);
                slots[CharacterLoadoutSlots.Helmet.ToInt()]          = itemSetupBalance.MakeItemOrNull(def.helmet);
                slots[CharacterLoadoutSlots.Armor.ToInt()]           = itemSetupBalance.MakeItemOrNull(def.armor);
                slots[CharacterLoadoutSlots.Headphones.ToInt()]      = itemSetupBalance.MakeItemOrNull(def.headphones);
                slots[CharacterLoadoutSlots.Safe.ToInt()]            = itemSetupBalance.MakeItemOrNull(def.safe);
                slots[CharacterLoadoutSlots.Skin.ToInt()]            = itemSetupBalance.MakeItemOrNull(def.skin);
                slots[CharacterLoadoutSlots.Skill.ToInt()]           = itemSetupBalance.MakeItemOrNull(def.skill);
                slots[CharacterLoadoutSlots.Perk1.ToInt()]           = itemSetupBalance.MakeItemOrNull(def.perk1);
                slots[CharacterLoadoutSlots.Perk2.ToInt()]           = itemSetupBalance.MakeItemOrNull(def.perk2);
                slots[CharacterLoadoutSlots.Perk3.ToInt()]           = itemSetupBalance.MakeItemOrNull(def.perk3);
            });

            var trashItems = LoadoutTrashPlacementHelper.ArrangeTetrisItems(
                this.gameDef,
                def.trash,
                itemSetupBalance);

            return new GameSnapshotLoadout {
                SlotItems = slots,
                TrashItems = trashItems,
            };
        }

    }
}