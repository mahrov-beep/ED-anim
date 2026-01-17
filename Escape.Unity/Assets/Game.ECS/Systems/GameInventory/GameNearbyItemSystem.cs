namespace Game.ECS.Systems.GameInventory {
    using System;
    using Domain.GameInventory;
    using Multicast;
    using Player;
    using Quantum;
    using Services.Photon;
    using UnityEngine;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class GameNearbyItemSystem : SystemBase {
        [Inject] private PhotonService                  photonService;
        [Inject] private LocalPlayerSystem              localPlayerSystem;
        [Inject] private GameNearbyItemsModel           gameNearbyItemsModel;
        [Inject] private GameNearbyInteractiveZoneModel interactiveZoneModel;

        public override void OnAwake() { }

        public override void Dispose() {
            base.Dispose();

            this.gameNearbyItemsModel.DeleteOutdated(int.MaxValue);
        }

        public override void OnUpdate(float deltaTime) {
            if (this.photonService.PredictedFrame is not { } f) {
                return;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            if (!f.TryGet(localRef, out CharacterLoadout loadout)) {
                return;
            }

            if (!f.TryGet(localRef, out Unit unit)) {
                return;
            }

            this.UpdateNearbyItemBox(f, this.gameNearbyItemsModel.NearbyItemBox, unit.NearbyItemBox, localRef);
            this.UpdateNearbyItemBox(f, this.gameNearbyItemsModel.NearbyBackpack, unit.NearbyBackpack, localRef);

            var frameNum = Time.frameCount;

            this.gameNearbyItemsModel.NearbyItemEntity         = unit.NearbyItemBox;
            this.gameNearbyItemsModel.NearbyBackpackItemEntity = unit.NearbyBackpack;

            var nearbyRef = loadout.StorageEntity;

            if (this.gameNearbyItemsModel.NearbyItemEntity != EntityRef.None && f.TryGet(this.gameNearbyItemsModel.NearbyItemEntity, out ItemBox itemBox)) {
                this.gameNearbyItemsModel.ItemBoxTimer = itemBox.TimerToOpen.AsFloat;
                this.gameNearbyItemsModel.ItemBoxTime  = itemBox.TimeToOpen.AsFloat;
            }
            else {
                this.gameNearbyItemsModel.ItemBoxTimer = 0;
                this.gameNearbyItemsModel.ItemBoxTime  = 0;
            }

            if (nearbyRef != EntityRef.None && f.TryGet(nearbyRef, out ItemBox nearbyItemBox)) {
                var isOpenedByLocal = nearbyItemBox.OpenerUnitRef == localRef;
                
                if (isOpenedByLocal) {
                    this.gameNearbyItemsModel.OpenedNearbyItemEntity = nearbyRef;

                    var nearbyItems = f.ResolveList(nearbyItemBox.ItemRefs);
                    foreach (var nearbyItem in nearbyItems) {
                        var nearbyItemModel = this.gameNearbyItemsModel.UpdateNearbyItem(frameNum, nearbyItem);

                        nearbyItemModel.HasEnoughSpaceInInventory.Value = loadout.HasEnoughFreeSpaceForItem(f, nearbyItem);
                    }
                }
                else {
                    gameNearbyItemsModel.IsOpenedByOtherPlayer = nearbyItemBox.OpenerUnitRef != EntityRef.None;
                }
            }

            if (this.gameNearbyItemsModel.OpenedNearbyItemEntity != EntityRef.None &&
                            nearbyRef == EntityRef.None) {
                this.gameNearbyItemsModel.OpenedNearbyItemEntity = EntityRef.None;
            }

            this.gameNearbyItemsModel.DeleteOutdated(frameNum);
        }

        private void UpdateNearbyItemBox(Frame f, GameNearbyItemBoxModel itemBoxModel, EntityRef nearbyItemBox, EntityRef localRef) {
            itemBoxModel.Entity = nearbyItemBox;

            if (nearbyItemBox != EntityRef.None && f.TryGet(nearbyItemBox, out ItemBox itemBox)) {
                itemBoxModel.ItemBoxTimer = itemBox.TimerToOpen.AsFloat;
                itemBoxModel.ItemBoxTime  = itemBox.TimeToOpen.AsFloat;

                itemBoxModel.IsOpenedByMe          = itemBox.OpenerUnitRef == localRef;
                itemBoxModel.IsOpenedByOtherPlayer = itemBox.OpenerUnitRef != EntityRef.None && itemBox.OpenerUnitRef != localRef;
            }
            else {
                itemBoxModel.ItemBoxTimer = 0;
                itemBoxModel.ItemBoxTime  = 0;

                itemBoxModel.IsOpenedByMe = false;
                itemBoxModel.IsOpenedByOtherPlayer = false;
            }
        }
    }
}