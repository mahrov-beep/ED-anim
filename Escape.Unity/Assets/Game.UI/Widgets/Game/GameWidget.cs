namespace Game.UI.Widgets.Game {
  using System.Collections.Generic;
  using System.Linq;
  using Controllers.Features.GameInventory;
  using Domain.GameInventory;
  using Domain.Game;
  using ECS.Systems.Player;
  using GameInventory;
  using Hud;
  using MainMenu;
  using Controllers.Features.Settings;
  using Multicast;
  using Quantum;
  using Services.Photon;
  using UnityEngine;
  using UniMob;
  using UniMob.UI;
  using UniMob.UI.Widgets;
  using Views;
  using Views.Game;
  using Views.Game.Hud;

  [RequireFieldsInit]
  public class GameWidget : StatefulWidget { }

  public class GameState : ViewState<GameWidget>, IGameState {
    [Inject] private GameInventoryModel gameInventoryModel;
    [Inject] private PhotonService photonService;
    [Inject] private LocalPlayerSystem localPlayerSystem;
    [Inject] private GameLocalCharacterModel localCharacterModel;
    [Inject] private GameStateModel gameStateModel;

    public override WidgetViewReference View => UiConstants.Views.Game.Screen;

    [Atom] public IState NearbyItems => this.RenderChild(_ => new GameNearbyWidget());

    [Atom] public IState SelectableWeapon => this.RenderChild(_ => new SelectableWeaponListWidget());

    [Atom] public IState UnitAbility => this.RenderChild(_ => new UnitAbilityOptionalWidget());
    [Atom] public IState KnifeAttack => this.RenderChild(_ => new KnifeAttackOptionalWidget());
    [Atom] public IState Quests => this.RenderChild(_ => new GameQuestsWidget());
    [Atom] public IState TimerSummary => this.RenderChild(_ => this.BuildTimerSummary());
    [Atom] public IState Map => this.RenderChild(_ => new MapWidget());

    [Atom] private GameInventoryTrashItemModel KitModel => this.ResolveKitModel();

    [Atom] public bool KitVisible => this.KitModel != null;

    [Atom]
    public IState Kit => this.RenderChild(_ => this.BuildKitWidget());

    [Atom] public IKnockControlsViewState KnockControls => this.RenderChildT(_ => new KnockControlsWidget()).As<KnockControlsState>();
    [Atom] public IVignetteViewState Vignette => this.RenderChildT(_ => new VignetteWidget()).As<VignetteState>();
    [Atom] public IListenedCueViewState ListenedCue => this.RenderChildT(_ => new ListenedCueWidget()).As<ListenedCueState>();
    [Atom] public IDamageSourceCueViewState DamageSourceCue => this.RenderChildT(_ => new DamageSourceCueWidget()).As<DamageSourceCueState>();
    [Atom] public IGrenadeIndicatorViewState GrenadeIndicator => this.RenderChildT(_ => new GrenadeIndicatorWidget()).As<GrenadeIndicatorState>();
    [Atom] public IStatsViewState Stats => this.RenderChildT(_ => new StatsWidget()).As<StatsState>();
    [Atom] public IHealthState Health => this.RenderChildT(_ => new HealthWidget()).As<HealthState>();
    [Atom] public IOpenInventoryIndicatorState InventoryIndicator => this.RenderChildT(_ => new OpenInventoryIndicatorWidget()).As<OpenInventoryIndicatorState>();


    [Atom] public IState WorldViews => this.RenderChild(_ => new MainMenuDynamicWidget());

    private Widget BuildKitWidget() {
      var kitModel = this.KitModel;

      if (kitModel == null) {
        return new EmptySlotWidget();
      }

      return new GameInventoryTrashItemWidget {
          Key         = Key.Of(kitModel),
          Model       = kitModel,
          IndexI      = 0,
          IndexJ      = 0,
          IsHudButton = true,
          NoDragging  = true,
          Source      = 0,
          InShop      = false,
      };
    }

    private GameInventoryTrashItemModel ResolveKitModel() {
        return this.localCharacterModel.BestMedKit;
    }

    private Widget BuildTimerSummary() {
      if (this.gameStateModel.GameState == EGameStates.Game) {
        return new GameEscapeModeSummaryWidget();
      }

      return new Empty();
    }
  }
}

