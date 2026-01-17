namespace Game.Shared.UserProfile.Commands.Loadouts {
    using System;
    using MessagePack;
    using Multicast;
    using System.Threading.Tasks;
    using Balance;
    using Data;
    using Defs;
    using Quantum;
    using UserProfile.Helpers;

    [MessagePackObject, RequireFieldsInit]
    public class EnsureInitialLoadoutCommand : IUserProfileServerCommand {
        [Key(0)] public string                    LoadoutKey;
        [Key(1)] public bool                      CreateNewIfNotExist;
        [Key(2)] public GameSnapshotLoadoutItem[] TrashItems;
        [Key(3)] public GameSnapshotLoadoutItem   Safe;
    }

    public class EnsureInitialLoadoutCommandHandler : UserProfileServerCommandHandler<EnsureInitialLoadoutCommand> {
        private readonly GameDef gameDef;

        public EnsureInitialLoadoutCommandHandler(GameDef gameDef) {
            this.gameDef = gameDef;
        }

        public override async Task<ServerCommandResult> Execute(UserProfileServerCommandContext context, SdUserProfile gameData, EnsureInitialLoadoutCommand command) {
            var selectedLoadoutGuid = gameData.Loadouts.SelectedLoadout.Value;

            if (!gameData.Loadouts.Lookup.TryGetValue(selectedLoadoutGuid, out var selectedLoadout) && command.CreateNewIfNotExist) {
                var loadoutKey = string.IsNullOrEmpty(command.LoadoutKey)
                    ? SharedConstants.Game.PlayerLoadouts.START
                    : command.LoadoutKey;

                if (!this.gameDef.PlayerLoadouts.TryGet(loadoutKey, out var loadoutDef)) {
                    return BadRequest("Player loadout key is invalid");
                }

                var itemSetupBalance = new ItemSetupBalance(this.gameDef, gameData);
                var loadoutSnapshot = this.MakeLoadoutFromDef(loadoutDef, itemSetupBalance, command.Safe, command.TrashItems);
                var loadoutData = gameData.Loadouts.Lookup.Create(Guid.NewGuid().ToString());
                loadoutData.LoadoutSnapshot.Value = loadoutSnapshot;
                gameData.Loadouts.SelectedLoadout.Value = loadoutData.Guid;
            }

            return Ok;
        }

        private GameSnapshotLoadout MakeLoadoutFromDef(PlayerLoadoutDef def, ItemSetupBalance itemSetupBalance, GameSnapshotLoadoutItem safeItem, GameSnapshotLoadoutItem[] extraTrashItems) {
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

            if (safeItem != default) {
                slots[CharacterLoadoutSlots.Safe.ToInt()] = safeItem;
            }

            var baseTrashItems = LoadoutTrashPlacementHelper.ArrangeTetrisItems(
                this.gameDef,
                def.trash,
                itemSetupBalance);

            var hasExtraTrash = extraTrashItems != null && extraTrashItems.Length > 0;
            var hasBaseTrash = baseTrashItems != null && baseTrashItems.Length > 0;

            GameSnapshotLoadoutItem[] trashItems;
            if (!hasExtraTrash && !hasBaseTrash) {
                trashItems = Array.Empty<GameSnapshotLoadoutItem>();
            }
            else if (!hasExtraTrash) {
                trashItems = baseTrashItems;
            }
            else if (!hasBaseTrash) {
                trashItems = extraTrashItems;
            }
            else {
                trashItems = new GameSnapshotLoadoutItem[baseTrashItems.Length + extraTrashItems.Length];
                Array.Copy(baseTrashItems, trashItems, baseTrashItems.Length);
                Array.Copy(extraTrashItems, 0, trashItems, baseTrashItems.Length, extraTrashItems.Length);
            }

            return new GameSnapshotLoadout {
                SlotItems  = slots,
                TrashItems = trashItems,
            };
        }

        private GameSnapshotLoadout MakeDefaultLoadout(GameSnapshotLoadoutItem safeItem, GameSnapshotLoadoutItem[] trashItems) {
            return new GameSnapshotLoadout {
                SlotItems = MakeLoadoutSlots(slots => {
                    slots[CharacterLoadoutSlots.PrimaryWeapon.ToInt()] = MakeItem(SharedConstants.Game.Items.WEAPON_PP, item => {
                        item.WeaponAttachments = MakeAttachments(attachments => {
                            //
                            attachments[WeaponAttachmentSlots.Ammo.ToInt()] = MakeAttachment(SharedConstants.Game.Items.ATTACHMENT_AMMO_PISTOL);
                        });
                    });
                    slots[CharacterLoadoutSlots.SecondaryWeapon.ToInt()] = MakeItem(SharedConstants.Game.Items.WEAPON_PISTOL_COMMON, item => {
                        item.WeaponAttachments = MakeAttachments(attachments => {
                            //
                            attachments[WeaponAttachmentSlots.Ammo.ToInt()] = MakeAttachment(SharedConstants.Game.Items.ATTACHMENT_AMMO_PISTOL);
                        });
                    });
                    
                    // slots[CharacterLoadoutSlots.Skill.ToInt()] = MakeItem(SharedConstants.Game.Items.ABILITY_GRENADE);
                    slots[CharacterLoadoutSlots.Skin.ToInt()]  = MakeItem(SharedConstants.Game.Items.SKIN_DEFAULT);
                    
                    if (safeItem != default) {
                        slots[CharacterLoadoutSlots.Safe.ToInt()] = safeItem;
                    }
                }),
                TrashItems = MakeArray<GameSnapshotLoadoutItem>(trashItems == default ? 12 : trashItems.Length + 12, items => {
                    items[0]  = MakeItem(SharedConstants.Game.Items.HEAL_BOX_SMALL, indexI: 0, indexJ: 0);
                    items[1]  = MakeItem(SharedConstants.Game.Items.HEAL_BOX_SMALL, indexI: 0, indexJ: 1);
                    items[2]  = MakeItem(SharedConstants.Game.Items.HEAL_BOX_SMALL, indexI: 0, indexJ: 2);
                    items[3]  = MakeItem(SharedConstants.Game.Items.HEAL_BOX_SMALL, indexI: 0, indexJ: 3);
                    items[4]  = MakeItem(SharedConstants.Game.Items.ATTACHMENT_AMMO_PISTOL, indexI: 1, indexJ: 0);
                    items[5]  = MakeItem(SharedConstants.Game.Items.ATTACHMENT_AMMO_PISTOL, indexI: 1, indexJ: 1);
                    items[6]  = MakeItem(SharedConstants.Game.Items.ATTACHMENT_AMMO_PISTOL, indexI: 2, indexJ: 0);
                    items[7]  = MakeItem(SharedConstants.Game.Items.ATTACHMENT_AMMO_PISTOL, indexI: 2, indexJ: 1);
                    items[8]  = MakeItem(SharedConstants.Game.Items.ATTACHMENT_AMMO_PISTOL, indexI: 1, indexJ: 2);
                    items[9]  = MakeItem(SharedConstants.Game.Items.ATTACHMENT_AMMO_PISTOL, indexI: 1, indexJ: 3);
                    items[10] = MakeItem(SharedConstants.Game.Items.ATTACHMENT_AMMO_PISTOL, indexI: 2, indexJ: 2);
                    items[11] = MakeItem(SharedConstants.Game.Items.ATTACHMENT_AMMO_PISTOL, indexI: 2, indexJ: 3);

                    if (trashItems != null) {
                        for (var i = 0; i < trashItems.Length; i++) {
                            items[i + 12] = trashItems[i];
                        }
                    }
                }),
            };

            GameSnapshotLoadoutItem MakeItem(string itemKey, Action<GameSnapshotLoadoutItem> builder = null, int indexI = 0, int indexJ = 0) {
                var item = new GameSnapshotLoadoutItem {
                    ItemGuid              = Guid.NewGuid().ToString(),
                    ItemKey               = itemKey,
                    WeaponAttachments     = null,
                    IndexI                = (byte)indexI,
                    IndexJ                = (byte)indexJ,
                    Rotated               = false,
                    Used                  = 0,
                    SafeGuid              = null,
                    AddToLoadoutAfterFail = false,
                };
                builder?.Invoke(item);
                return item;
            }

            GameSnapshotLoadoutWeaponAttachment MakeAttachment(string itemKey) {
                return new GameSnapshotLoadoutWeaponAttachment {
                    ItemGuid = Guid.NewGuid().ToString(),
                    ItemKey  = itemKey,
                    IndexI   = 0,
                    IndexJ   = 0,
                    Rotated  = false,
                    Used     = 0,
                };
            }

            GameSnapshotLoadoutItem[] MakeLoadoutSlots(Action<GameSnapshotLoadoutItem[]> builder) {
                return MakeArray(CharacterLoadoutSlotsExtension.CHARACTER_LOADOUT_SLOTS, builder);
            }

            GameSnapshotLoadoutWeaponAttachment[] MakeAttachments(Action<GameSnapshotLoadoutWeaponAttachment[]> builder) {
                return MakeArray(WeaponAttachmentSlotsExtension.WEAPON_ATTACHMENT_SLOTS, builder);
            }

            T[] MakeArray<T>(int length, Action<T[]> builder) {
                var array = new T[length];
                builder(array);
                return array;
            }
        }

    }
}