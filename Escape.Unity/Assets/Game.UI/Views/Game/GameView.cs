namespace Game.UI.Views.Game {
    using Hud;
    using UniMob.UI;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class GameView : AutoView<IGameState> {
        [SerializeField, Required] private ViewPanel worldPanel;
        [SerializeField, Required] private ViewPanel nearbyItemsPanel;
        [SerializeField, Required] private ViewPanel selectableWeaponPanel;
        [SerializeField, Required] private ViewPanel unitAbilityWeaponPanel;
        [SerializeField, Required] private ViewPanel knifeButtonPanel;
        [SerializeField, Required] private ViewPanel questsPanel;
        [SerializeField, Required] private ViewPanel timerPanel;
        [SerializeField, Required] private ViewPanel minimapPanel;
        [SerializeField, Required] private ViewPanel  kitPanel;
        [SerializeField, Required] private GameObject useKitRoot;
        [SerializeField, Required] private OpenInventoryView inventoryIndicatorView;
        [SerializeField, Required] private KnockControlsVisibility knockControlsView;

        [SerializeField, Required] private VignetteView vignetteView;
        [SerializeField, Required] private HealthView   healthViewView;
        [SerializeField, Required] private StatsView    statsView;

        [SerializeField, Required] private ListenedCueView listenedCueView;
        [SerializeField, Required] private DamageSourceCueView damageSourceCueView;
        [SerializeField, Required] private GrenadeIndicatorView grenadeIndicatorView;

        protected override void Render() {
            base.Render();

            this.worldPanel.Render(this.State.WorldViews);
            this.nearbyItemsPanel.Render(this.State.NearbyItems);
            this.selectableWeaponPanel.Render(this.State.SelectableWeapon);

            this.unitAbilityWeaponPanel.Render(this.State.UnitAbility);
            this.knifeButtonPanel.Render(this.State.KnifeAttack);
            this.questsPanel.Render(this.State.Quests);
            this.timerPanel.Render(this.State.TimerSummary);
            this.minimapPanel.Render(this.State.Map);

            var kitVisible = this.State.KitVisible;
            if (this.useKitRoot.activeSelf != kitVisible) {
                this.useKitRoot.SetActive(kitVisible);
            }

            if (kitVisible) {
                this.kitPanel.Render(this.State.Kit);
            }

            this.inventoryIndicatorView.Render(this.State.InventoryIndicator);

            this.knockControlsView.Render(this.State.KnockControls);

            vignetteView.Render(State.Vignette);
            listenedCueView.Render(State.ListenedCue);
            damageSourceCueView.Render(State.DamageSourceCue);
            grenadeIndicatorView.Render(State.GrenadeIndicator);
            statsView.Render(State.Stats);
            healthViewView.Render(State.Health);
        }
    }

    public interface IGameState : IViewState {
        IState WorldViews       { get; }
        IState NearbyItems      { get; }
        IState SelectableWeapon { get; }

        IState UnitAbility { get; }
        IState KnifeAttack { get; }
        IState Quests      { get; }
        IState TimerSummary { get; }
        IState Map         { get; }
        bool  KitVisible   { get; }
        IState Kit         { get; }
        IOpenInventoryIndicatorState InventoryIndicator { get; }

        IKnockControlsViewState    KnockControls    { get; }
        IVignetteViewState         Vignette         { get; }
        IListenedCueViewState      ListenedCue      { get; }
        IDamageSourceCueViewState  DamageSourceCue  { get; }
        IGrenadeIndicatorViewState GrenadeIndicator { get; }
        IStatsViewState            Stats            { get; }
        IHealthState               Health           { get; }
    }
}
