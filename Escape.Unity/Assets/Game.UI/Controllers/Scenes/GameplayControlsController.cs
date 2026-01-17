namespace Game.UI.Controllers.Scenes {
    using System;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Domain.Game;
    using Domain.GameInventory;
    using ECS.Systems.Player;
    using Features.GameInventory;
    using Features.Settings;
    using Multicast;
    using Multicast.GameProperties;
    using Quantum;
    using Quantum.Commands;
    using Services.Photon;
    using UniMob;
    using UniMob.UI.Widgets;
    using UnityEngine;
    using UnityEngine.InputSystem;

    [Serializable, RequireFieldsInit]
    public struct GameplayControlsControllerArgs : IFlowControllerArgs {
    }

    public class GameplayControlsController : FlowController<GameplayControlsControllerArgs> {
        [Inject] private PhotonService           photonService;
        [Inject] private GameNearbyItemsModel    nearbyItemsModel;
        [Inject] private GamePropertiesModel     gamePropertiesModel;
        [Inject] private GameLocalCharacterModel localCharacterModel;

        private InputActionMap gameplayActionMap;
        private NavigatorState navigator;

        private InputAction mouseLookAction;
        private InputAction swipeLookAction;

        [Atom] public bool UseMobileControls => Application.isMobilePlatform || this.gamePropertiesModel.Get(GameProperties.Booleans.ForceUseMobileControls);

        protected override async UniTask Activate(Context context) {
            this.navigator = context.RootNavigator;

            var playerInput = PlayerInput.GetPlayerByIndex(0);
            this.gameplayActionMap = playerInput.actions.FindActionMap("GameplayActions", true);

            this.gameplayActionMap.FindAction("OpenInventory", true).Subscribe(this.Lifetime, ctx => this.RequestFlow(this.OpenInventory, ctx, FlowOptions.NowOrNever));
            this.gameplayActionMap.FindAction("OpenSettings", true).Subscribe(this.Lifetime, ctx => this.RequestFlow(this.OpenSettings, ctx, FlowOptions.NowOrNever));

            this.gameplayActionMap.FindAction("OpenNearbyItemBox", true).Subscribe(this.Lifetime, ctx => this.RequestFlow(this.OpenNearbyItemBox, ctx, FlowOptions.NowOrNever));
            this.gameplayActionMap.FindAction("OpenNearbyBackpack", true).Subscribe(this.Lifetime, ctx => this.RequestFlow(this.OpenNearbyBackpack, ctx, FlowOptions.NowOrNever));

            this.gameplayActionMap.FindAction("EquipBestFromNearbyItemBox", true).Subscribe(this.Lifetime, ctx => this.RequestFlow(this.EquipBestFromNearbyItemBox, ctx, FlowOptions.NowOrNever));
            this.gameplayActionMap.FindAction("EquipBestFromNearbyBackpack", true).Subscribe(this.Lifetime, ctx => this.RequestFlow(this.EquipBestFromNearbyBackpack, ctx, FlowOptions.NowOrNever));

            this.gameplayActionMap.FindAction("Heal", true).Subscribe(this.Lifetime, ctx => this.RequestFlow(this.Heal, ctx, FlowOptions.NowOrNever));

            this.gameplayActionMap.FindAction("UseAbility", true).Subscribe(this.Lifetime, ctx => this.RequestFlow(this.UseAbility, ctx, FlowOptions.NowOrNever));
            this.gameplayActionMap.FindAction("KnifeAttack", true).Subscribe(this.Lifetime, ctx => this.RequestFlow(this.KnifeAttack, ctx, FlowOptions.NowOrNever));
            this.gameplayActionMap.FindAction("Revive", true).Subscribe(this.Lifetime, ctx => this.RequestFlow(this.Revive, ctx, FlowOptions.NowOrNever));

            this.mouseLookAction = this.gameplayActionMap.FindAction("LookDelta_Mouse");
            this.swipeLookAction = this.gameplayActionMap.FindAction("LookDelta_Screen");

            Atom.Reaction(this.Lifetime, () => (this.navigator.IsOnGameplay(), this.UseMobileControls), this.ReactActivateControls);
        }

        protected override void OnUpdate() {
            var isOnGameplay = this.navigator.IsOnGameplay();

            var cursorLocked = isOnGameplay && !this.UseMobileControls;
            Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible   = !cursorLocked;
        }

        private void ReactActivateControls((bool isOnGameplay, bool useMobileControls) state) {
            if (state.isOnGameplay) {
                this.gameplayActionMap.Enable();
            }
            else {
                this.gameplayActionMap.Disable();
            }

            var useMouseControl = state is { isOnGameplay: true, useMobileControls: false };
            var useSwipeControl = state is { isOnGameplay: true, useMobileControls: true };

            if (useSwipeControl) {
                this.swipeLookAction.Enable();
            }
            else {
                this.swipeLookAction.Disable();
            }

            if (useMouseControl) {
                this.mouseLookAction.Enable();
            }
            else {
                this.mouseLookAction.Disable();
            }
        }

        private async UniTask OpenInventory(Context context, InputAction.CallbackContext actionContext) {
            GameInventoryFeatureEvents.Open.Raise();
        }

        private async UniTask OpenSettings(Context context, InputAction.CallbackContext actionContext) {
            await UniTask.NextFrame();
            SettingsFeatureEvents.Open.Raise();
        }

        private UniTask OpenNearbyItemBox(Context context, InputAction.CallbackContext callbackContext) {
            return this.OpenNearby(context, callbackContext, isBackpack: false);
        }

        private UniTask OpenNearbyBackpack(Context context, InputAction.CallbackContext callbackContext) {
            return this.OpenNearby(context, callbackContext, isBackpack: true);
        }

        private async UniTask OpenNearby(Context context, InputAction.CallbackContext callbackContext, bool isBackpack) {
            if (!this.photonService.TryGetPredicted(out var f)) {
                return;
            }

            var itemBoxRef = isBackpack
                ? this.nearbyItemsModel.NearbyBackpackItemEntity
                : this.nearbyItemsModel.NearbyItemEntity;

            if (!f.TryGet(itemBoxRef, out ItemBox itemBox)) {
                return;
            }

            this.photonService.Runner?.Game.SendCommand(new OpenItemBoxCommand {
                OpenBackpack = isBackpack,
            });

            if (itemBox.TimerToOpen > 0) {
                return;
            }

            GameInventoryFeatureEvents.Open.Raise();
        }

        private UniTask EquipBestFromNearbyItemBox(Context context, InputAction.CallbackContext callbackContext) {
            return this.EquipBestFromNearby(context, callbackContext, isBackpack: false);
        }

        private UniTask EquipBestFromNearbyBackpack(Context context, InputAction.CallbackContext callbackContext) {
            return this.EquipBestFromNearby(context, callbackContext, isBackpack: true);
        }

        private async UniTask EquipBestFromNearby(Context context, InputAction.CallbackContext callbackContext, bool isBackpack) {
            if (!this.photonService.TryGetPredicted(out var f)) {
                return;
            }

            var itemBoxRef = isBackpack
                ? this.nearbyItemsModel.NearbyBackpackItemEntity
                : this.nearbyItemsModel.NearbyItemEntity;

            if (!f.TryGet(itemBoxRef, out ItemBox itemBox)) {
                return;
            }

            this.photonService.Runner?.Game.SendCommand(new PickUpBestFromNearbyItemBoxLoadoutCommand {
                NeedToRemoveFromStorage = false,
                EquipTrash              = true,
                IsBackpack              = isBackpack,
            });
        }

        private async UniTask Heal(Context context, InputAction.CallbackContext callbackContext) {
            var heal = this.localCharacterModel.BestMedKit;

            if (heal == null) {
                return;
            }

            this.photonService.Runner?.Game.SendCommand(new UseItemCommand {
                ItemEntity = heal.ItemEntity,
            });
        }

        private async UniTask UseAbility(Context context, InputAction.CallbackContext callbackContext) {
            // сейчас логика в InputPollSystem, возможно стоит переделать на QuantumCommand
        }

        private async UniTask KnifeAttack(Context context, InputAction.CallbackContext callbackContext) {
            var canAttack = this.localCharacterModel.CanKnifeAttack && !this.localCharacterModel.IsKnifeAttacking;

            if (!canAttack) {
                return;
            }

            this.photonService.Runner?.Game.SendCommand(new KnifeAttackCommand());
        }

        private async UniTask Revive(Context context, InputAction.CallbackContext callbackContext) {
            var canStartRevive = this.localCharacterModel.CanReviveTeammate && !this.localCharacterModel.IsRevivingTeammate;

            if (!canStartRevive) {
                return;
            }

            this.photonService.Runner?.Game.SendCommand(new ReviveCommand());
        }
    }
}