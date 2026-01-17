namespace Game.UI.Widgets.Game {
    using Domain.GameInventory;
    using ECS.Systems.Player;
    using Multicast;
    using Quantum;
    using Services.Photon;
    using UniMob.UI;
    using Views.Game;

    [RequireFieldsInit]
    public class UnitAbilityWidget : StatefulWidget {
    }

    public class UnitAbilityState : ViewState<UnitAbilityWidget>, IUnitAbilityState {
        [Inject] private PhotonService      photonService;
        [Inject] private LocalPlayerSystem  localPlayerSystem;
        [Inject] private GameInventoryModel gameInventoryModel;

        public override WidgetViewReference View =>
            this.SlotType switch {
                CharacterLoadoutSlots.Skill => UiConstants.Views.HUD.UnitAbility,
                _ => UiConstants.Views.GameInventory.SlotEmptyItem,
            };

        private Frame Frame => this.photonService.PredictedFrame;

        private EntityRef ItemEntity {
            get {
                if (this.gameInventoryModel.TryGetSlotItem(CharacterLoadoutSlots.Skill, out var slotItemModel)) {
                    return slotItemModel.ItemEntity;
                }

                return EntityRef.None;
            }
        }

        private Ability Ability => this.Frame!.Get<Ability>(this.ItemEntity);

        private GameAbilityModel AbilityModel => this.gameInventoryModel.AbilityModel;
        private AbilityItemAsset ItemAsset    => this.Ability.GetConfig(this.Frame);

        public CharacterLoadoutSlots SlotType => CharacterLoadoutSlots.Skill;

        public float ReloadingProgress => this.AbilityModel.ReloadingTimer;

        public string ItemKey  => this.ItemAsset.ItemKey;
        public string ItemIcon => ItemEntity != EntityRef.None ? this.ItemAsset.IconLarge : string.Empty;

        public void TriggerAction() { }    
    }
}