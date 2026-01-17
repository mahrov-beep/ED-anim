namespace Game.UI.Widgets.Items {
    using System;
    using System.Linq;
    using JetBrains.Annotations;
    using Quantum;
    using UniMob.UI;
    using UniMob.UI.Widgets;

    [RequireFieldsInit]
    public class SnapshotItemDetailsWidget : StatefulWidget {
        public GameSnapshotLoadoutItem Item;
    }

    public class SnapshotItemDetailsState : HocState<SnapshotItemDetailsWidget> {
        private ItemAsset ItemAsset => QuantumUnityDB.Global.GetRequiredAsset<ItemAsset>(
            ItemAssetCreationData.GetItemAssetPath(this.Widget.Item.ItemKey));

        public override Widget Build(BuildContext context) {
            switch (this.ItemAsset) {
                case WeaponItemAsset weaponItemAsset:
                    return this.BuildWeaponAttachments(weaponItemAsset);
            }

            return new Empty();
        }

        private Widget BuildWeaponAttachments(WeaponItemAsset weaponItemAsset) {
            if (weaponItemAsset.attachmentsSchema == null) {
                return new Empty();
            }

            var attachments = this.Widget.Item.WeaponAttachments ?? Array.Empty<GameSnapshotLoadoutWeaponAttachment>();

            return new GridFlow {
                Padding            = new RectPadding(15, 5, 5, 5),
                MainAxisSize       = AxisSize.Max,
                CrossAxisSize      = AxisSize.Max,
                MainAxisAlignment  = MainAxisAlignment.End,
                CrossAxisAlignment = CrossAxisAlignment.End,
                Children = {
                    weaponItemAsset.attachmentsSchema.slots
                        .Select(slot => slot.ToInt() < attachments.Length ? attachments[slot.ToInt()] : null)
                        .Select(this.BuildWeaponAttachment),
                },
            };
        }

        private Widget BuildWeaponAttachment([CanBeNull] GameSnapshotLoadoutWeaponAttachment attachment) {
            if (attachment == null) {
                return new EmptyItemAttachmentMarkerWidget();
            }

            return new ItemAttachmentMarkerWidget {
                ItemKey = attachment.ItemKey,
            };
        }
    }
}