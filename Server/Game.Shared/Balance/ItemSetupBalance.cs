namespace Game.Shared.Balance {
    using System;
    using System.Linq;
    using Defs;
    using JetBrains.Annotations;
    using Quantum;
    using UserProfile.Data;

    public class ItemSetupBalance {
        private readonly GameDef       gameDef;
        private readonly SdUserProfile userProfile;

        public ItemSetupBalance(GameDef gameDef, SdUserProfile userProfile) {
            this.gameDef     = gameDef;
            this.userProfile = userProfile;
        }

        public ItemSetupDef GetDef(string itemSetupKey) {
            return this.gameDef.ItemSetups.Get(itemSetupKey);
        }

        public GameSnapshotLoadoutItem MakeItemOrNull(string itemSetupKey) {
            return string.IsNullOrEmpty(itemSetupKey) ? null : this.MakeItem(itemSetupKey);
        }

        public GameSnapshotLoadoutItem MakeItem([NotNull] string itemSetupKey) {
            if (itemSetupKey == null) {
                throw new ArgumentNullException(nameof(itemSetupKey));
            }

            var itemSetupDef = this.GetDef(itemSetupKey);

            var hasAnyAttachment = new[] {
                itemSetupDef.scope,
                itemSetupDef.grip,
                itemSetupDef.muzzle,
                itemSetupDef.magazine,
                itemSetupDef.stock,
                itemSetupDef.ammo,
                itemSetupDef.laser,
            }.Any(it => !string.IsNullOrEmpty(it));

            return new GameSnapshotLoadoutItem {
                ItemGuid = Guid.NewGuid().ToString(),
                ItemKey  = itemSetupDef.itemKey,
                WeaponAttachments = !hasAnyAttachment
                    ? null
                    : GameSnapshotLoadoutItem.MakeAttachments(attachments => {
                        attachments[WeaponAttachmentSlots.Scope.ToInt()]    = MakeAttachmentOrNull(itemSetupDef.scope);
                        attachments[WeaponAttachmentSlots.Grip.ToInt()]     = MakeAttachmentOrNull(itemSetupDef.grip);
                        attachments[WeaponAttachmentSlots.Muzzle.ToInt()]   = MakeAttachmentOrNull(itemSetupDef.muzzle);
                        attachments[WeaponAttachmentSlots.Magazine.ToInt()] = MakeAttachmentOrNull(itemSetupDef.magazine);
                        attachments[WeaponAttachmentSlots.Stock.ToInt()]    = MakeAttachmentOrNull(itemSetupDef.stock);
                        attachments[WeaponAttachmentSlots.Ammo.ToInt()]     = MakeAttachmentOrNull(itemSetupDef.ammo);
                        attachments[WeaponAttachmentSlots.Laser.ToInt()]    = MakeAttachmentOrNull(itemSetupDef.laser);
                    }),
                IndexI                = 0,
                IndexJ                = 0,
                Rotated               = false,
                Used                  = 0,
                SafeGuid              = null,
                AddToLoadoutAfterFail = false,
            };

            GameSnapshotLoadoutWeaponAttachment MakeAttachmentOrNull(string itemKey) {
                if (string.IsNullOrEmpty(itemKey)) {
                    return null;
                }

                return new GameSnapshotLoadoutWeaponAttachment {
                    ItemGuid = Guid.NewGuid().ToString(),
                    ItemKey  = itemKey,
                    IndexI   = 0,
                    IndexJ   = 0,
                    Rotated  = false,
                    Used     = 0,
                };
            }
        }

        public bool IsMatch(string itemSetupKey, GameSnapshotLoadoutItem item) {
            var itemSetupDef = this.GetDef(itemSetupKey);

            if (itemSetupDef.itemKey != item.ItemKey) {
                return false;
            }

            if (!IsMatchAttachment(WeaponAttachmentSlots.Scope, itemSetupDef.scope) ||
                !IsMatchAttachment(WeaponAttachmentSlots.Grip, itemSetupDef.grip) ||
                !IsMatchAttachment(WeaponAttachmentSlots.Muzzle, itemSetupDef.muzzle) ||
                !IsMatchAttachment(WeaponAttachmentSlots.Magazine, itemSetupDef.magazine) ||
                !IsMatchAttachment(WeaponAttachmentSlots.Stock, itemSetupDef.stock) ||
                !IsMatchAttachment(WeaponAttachmentSlots.Ammo, itemSetupDef.ammo) ||
                !IsMatchAttachment(WeaponAttachmentSlots.Laser, itemSetupDef.laser)) {
                return false;
            }

            return true;

            bool IsMatchAttachment(WeaponAttachmentSlots slot, string itemKey) {
                var actualAttachmentKey = slot.ToInt() < item.WeaponAttachments?.Length
                    ? item.WeaponAttachments[slot.ToInt()]?.ItemKey
                    : null;

                if (string.IsNullOrEmpty(actualAttachmentKey) && string.IsNullOrEmpty(itemKey)) {
                    return true;
                }

                return actualAttachmentKey == itemKey;
            }
        }
    }
}