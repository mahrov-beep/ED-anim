// ReSharper disable NonReadonlyMemberInGetHashCode

// ReSharper disable InconsistentNaming

namespace Quantum {
    using System;
    using MessagePack;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using JetBrains.Annotations;

    // снимок состояния игры
    // по снимку сервер определяет кто победил и генерирует GameResults
    // здесь мы не делаем никаких предположений о том кто победитель,
    // кто получит сколько награды и т.д. Тут только состояние игры
    [Serializable, MessagePackObject, RequireFieldsInit]
    public sealed class GameSnapshot {
        [Key(1), CanBeNull]              public string                 GameMode;
        [Key(2)]                         public bool                   IsGameEnded;
        [Key(3), CanBeNull, ItemNotNull] public List<GameSnapshotUser> Users;
        [Key(4), CanBeNull, ItemNotNull] public List<GameSnapshotTeam> Teams;
        [Key(5)]                         public GameRules              GameRule;
    }

    [Serializable, MessagePackObject, RequireFieldsInit]
    public sealed class GameSnapshotTeam {
        [Key(0)] public int TeamNumber;
    }

    [Serializable, MessagePackObject, RequireFieldsInit]
    public sealed class GameSnapshotUser {
        [Key(0)] public int  ActorNumber;
        [Key(1)] public Guid UserId;
        [Key(2)] public int  GameTeamId;
        [Key(3)] public int  Frags;
        [Key(4)] public bool IsDead;

        [Key(5), CanBeNull] public GameSnapshotLoadout Loadout;
    }

    [Serializable, MessagePackObject, RequireFieldsInit]
    public sealed class GameSnapshotLoadout {
        [Key(0), CanBeNull, ItemCanBeNull] public GameSnapshotLoadoutItem[] SlotItems;
        [Key(1), CanBeNull, ItemCanBeNull] public GameSnapshotLoadoutItem[] TrashItems;

        public bool TryGetItemAtSlot(CharacterLoadoutSlots slot, out GameSnapshotLoadoutItem result) {
            result = this.SlotItems != null && slot.ToInt() < this.SlotItems.Length ? this.SlotItems[slot.ToInt()] : null;
            return result != null;
        }

        public void SetItemToSlot(CharacterLoadoutSlots slot, GameSnapshotLoadoutItem item) {
            this.SlotItems ??= new GameSnapshotLoadoutItem[CharacterLoadoutSlotsExtension.CHARACTER_LOADOUT_SLOTS];

            this.SlotItems[slot.ToInt()] = item;
        }

        public GameSnapshotLoadoutItem GetItemWithGuid(string guid) {
            if (GetItemWithGuidFromArray(guid, this.SlotItems) is { } matchingSlotItem) {
                return matchingSlotItem;
            }

            if (GetItemWithGuidFromArray(guid, this.TrashItems) is { } matchingTrashItem) {
                return matchingTrashItem;
            }

            return null;

            static GameSnapshotLoadoutItem GetItemWithGuidFromArray(string guid, GameSnapshotLoadoutItem[] items) {
                if (items == null) {
                    return null;
                }

                foreach (var item in items) {
                    if (item == null) {
                        continue;
                    }

                    if (item.ItemGuid == guid) {
                        return item;
                    }

                    if (item.GetAttachmentWithGuid(guid) is { } matchingAttachmentItem) {
                        return matchingAttachmentItem;
                    }
                }

                return null;
            }
        }

        public GameSnapshotLoadout DeepClone(bool generateNewGuids = false) {
            return new GameSnapshotLoadout {
                SlotItems = this.SlotItems == null
                    ? null
                    : Array.ConvertAll(this.SlotItems, it => it?.DeepClone(generateNewGuids)),

                TrashItems = this.TrashItems == null
                    ? null
                    : Array.ConvertAll(this.TrashItems, it => it?.DeepClone(generateNewGuids)),
            };
        }

        public static GameSnapshotLoadoutItem[] MakeLoadoutSlots(Action<GameSnapshotLoadoutItem[]> builder) {
            var array = new GameSnapshotLoadoutItem[CharacterLoadoutSlotsExtension.CHARACTER_LOADOUT_SLOTS];
            builder(array);
            return array;
        }
    }

    [Serializable, MessagePackObject, RequireFieldsInit]
    public sealed class GameSnapshotStorage {
        [Key(0), CanBeNull, ItemCanBeNull] public GameSnapshotLoadoutItem[] items;
    }

    [Serializable, MessagePackObject, RequireFieldsInit]
    public sealed class GameSnapshotLoadoutItem {
        [Key(0), CanBeNull] public string ItemGuid;
        [Key(1), CanBeNull] public string ItemKey;

        [Key(2), CanBeNull, ItemCanBeNull] public GameSnapshotLoadoutWeaponAttachment[] WeaponAttachments;

        [Key(3)]            public byte   IndexI;
        [Key(4)]            public byte   IndexJ;
        [Key(5)]            public bool   Rotated;
        [Key(6)]            public ushort Used;
        [Key(7), CanBeNull] public string SafeGuid;
        [Key(8)]            public bool   AddToLoadoutAfterFail;

        [CanBeNull]
        public GameSnapshotLoadoutItem GetAttachmentWithGuid(string guid) {
            if (this.WeaponAttachments == null) {
                return null;
            }

            foreach (var attachment in this.WeaponAttachments) {
                if (attachment != null && attachment.ItemGuid == guid) {
                    return attachment.ToItem();
                }
            }

            return null;
        }

        public GameSnapshotLoadoutItem DeepClone(bool generateNewGuids = false) {
            return new GameSnapshotLoadoutItem {
                ItemGuid = generateNewGuids ? Guid.NewGuid().ToString() : this.ItemGuid,
                ItemKey  = this.ItemKey,
                WeaponAttachments = this.WeaponAttachments == null
                    ? null
                    : Array.ConvertAll(this.WeaponAttachments, it => it?.DeepClone(generateNewGuids)),
                IndexI                = this.IndexI,
                IndexJ                = this.IndexJ,
                Rotated               = this.Rotated,
                Used                  = this.Used,
                SafeGuid              = this.SafeGuid,
                AddToLoadoutAfterFail = this.AddToLoadoutAfterFail,
            };
        }

        public static GameSnapshotLoadoutWeaponAttachment[] MakeAttachments(Action<GameSnapshotLoadoutWeaponAttachment[]> builder) {
            var array = new GameSnapshotLoadoutWeaponAttachment[WeaponAttachmentSlotsExtension.WEAPON_ATTACHMENT_SLOTS];
            builder(array);
            return array;
        }
    }

    [Serializable, MessagePackObject, RequireFieldsInit]
    public sealed class GameSnapshotLoadoutWeaponAttachment {
        [Key(0), CanBeNull] public string ItemGuid;
        [Key(1), CanBeNull] public string ItemKey;
        [Key(2)]            public byte   IndexI;
        [Key(3)]            public byte   IndexJ;
        [Key(4)]            public bool   Rotated;
        [Key(5)]            public ushort Used;

        public GameSnapshotLoadoutItem ToItem() => new() {
            ItemGuid          = this.ItemGuid,
            ItemKey           = this.ItemKey,
            WeaponAttachments = null,
            IndexI            = this.IndexI,
            IndexJ            = this.IndexJ,
            Rotated           = this.Rotated,
            Used              = this.Used,
        };

        public GameSnapshotLoadoutWeaponAttachment DeepClone(bool generateNewGuids = false) {
            return new GameSnapshotLoadoutWeaponAttachment {
                ItemGuid = generateNewGuids ? Guid.NewGuid().ToString() : this.ItemGuid,
                ItemKey  = this.ItemKey,
                IndexI   = this.IndexI,
                IndexJ   = this.IndexJ,
                Rotated  = this.Rotated,
                Used     = this.Used,
            };
        }
    }
}