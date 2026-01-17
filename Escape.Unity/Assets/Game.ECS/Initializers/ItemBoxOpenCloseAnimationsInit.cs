namespace Game.ECS.Initializers {
    using System;
    using Game.Domain.Game;
    using Scripts;
    using Systems.Core;
    using Game.Services.Photon;
    using Multicast;
    using Quantum;
    using Scellecs.Morpeh;
    using UnityEngine;
    using static Quantum.QuantumUnityStaticDispatcherAdapter<Quantum.QuantumUnityEventDispatcher, Quantum.EventBase>;

    public class ItemBoxOpenCloseAnimationsInit : IInitializer {
        public World World { get; set; }

        private PhotonService           photonService;
        private QuantumEntityViewSystem updater;
        private GameLocalCharacterModel localCharacterModel;

        private IDisposable onOpen, onClose;

        public void OnAwake() {
            photonService       = App.Get<PhotonService>();
            updater             = App.Get<QuantumEntityViewSystem>();
            localCharacterModel = App.Get<GameLocalCharacterModel>();

            onOpen  = SubscribeManual<EventOpenItemBox>(OnOpenItemBox);
            // onClose = SubscribeManual<EventCloseItemBox>(OnCloseItemBox);
        }

        public void Dispose() {
            onOpen?.Dispose();
            // onClose?.Dispose();
        }

        private void OnOpenItemBox(EventOpenItemBox evt) => SetState(true, evt.itemBoxRef);

        // private void OnCloseItemBox(EventCloseItemBox evt) => SetState(false, evt.itemBoxRef);

        private void SetState(bool isOpened, EntityRef itemBoxRef) {
            if (!photonService.TryGetPredicted(out var f)) {
                return;
            }

            if (!updater.TryGetEntityView(itemBoxRef, out var view)) {
                return;
            }

            const int distanceSquaredThreshold = 3000; //54.7722557505, в будущем можно систему AABB приспособить вообще под все энтити
            var instantlyAnimation =
                            distanceSquaredThreshold
                            <
                            (view.Transform.position - localCharacterModel.PositionView).sqrMagnitude;

            foreach (var component in view.ViewComponents) {
                if (component is IOpenableView openableView) {
                    if (isOpened) {
                        openableView.Open(instantlyAnimation);
                    }
                    else {
                        openableView.Close(instantlyAnimation);
                    }

                    return;
                }
            }

            Debug.LogWarning($"Has no {nameof(IOpenableView)} component for {itemBoxRef}");
        }
    }

}