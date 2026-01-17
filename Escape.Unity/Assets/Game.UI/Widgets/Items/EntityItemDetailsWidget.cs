namespace Game.UI.Widgets.Items {
    using System.Linq;
    using Multicast;
    using Quantum;
    using Services.Photon;
    using UniMob.UI;
    using UniMob.UI.Widgets;

    [RequireFieldsInit]
    public class EntityItemDetailsWidget : StatefulWidget {
        public EntityRef ItemEntity;
    }

    public class EntityItemDetailsState : HocState<EntityItemDetailsWidget> {
        [Inject] private PhotonService photonService;

        public override Widget Build(BuildContext context) {
            var f = this.photonService.PredictedFrame;
            if (f == null) {
                return new Empty();
            }

            if (!f.Exists(this.Widget.ItemEntity)) {
                return new Empty();
            }

            if (!f.TryGet(this.Widget.ItemEntity, out Item item)) {
                return new Empty();
            }

            var itemAsset = f.FindAsset(item.Asset);

            switch (itemAsset) {
                case WeaponItemAsset weaponItemAsset:
                    return this.BuildWeaponAttachments(weaponItemAsset);
            }

            return new Empty();
        }

        private Widget BuildWeaponAttachments(WeaponItemAsset weaponItemAsset) {
            if (weaponItemAsset.attachmentsSchema == null) {
                return new Empty();
            }

            var f = this.photonService.PredictedFrame;
            if (f == null || !f.TryGet(this.Widget.ItemEntity, out WeaponItem weaponItem)) {
                return new Empty();
            }

            return new GridFlow {
                Padding = new RectPadding(15, 5, 5, 5),
                MainAxisSize       = AxisSize.Max,
                CrossAxisSize      = AxisSize.Max,
                MainAxisAlignment  = MainAxisAlignment.End,
                CrossAxisAlignment = CrossAxisAlignment.End,
                Children = {
                    weaponItemAsset.attachmentsSchema.slots
                        .Select(slot => weaponItem.AttachmentAtSlot(slot))
                        .Select(this.BuildWeaponAttachment),
                },
            };
        }

        private Widget BuildWeaponAttachment(EntityRef attachmentEntity) {
            if (attachmentEntity == EntityRef.None) {
                return new EmptyItemAttachmentMarkerWidget();
            }

            var f = this.photonService.PredictedFrame;
            if (f == null || !f.Exists(attachmentEntity) || !f.TryGet(attachmentEntity, out Item attachmentItem)) {
                return new EmptyItemAttachmentMarkerWidget();
            }
            var attachmentAsset = f.FindAsset(attachmentItem.Asset);

            return new ItemAttachmentMarkerWidget {
                ItemKey = attachmentAsset.ItemKey,
            };
        }
    }
}