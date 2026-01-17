namespace _Project.Scripts.GameView {
  using System;
  using Game.ECS.Systems.Player;
  using Multicast;
  using Quantum;
  using Sirenix.OdinInspector;
  using UnityEngine;

  /// <summary>
  /// Base class for visibility views that need to resolve the local player, compare teams
  /// and toggle a set of visuals on a time budget.
  /// </summary>
  public abstract class TeamVisibilityViewBase : QuantumEntityViewComponent {
    private const float MinDistanceCheckInterval = 0.01f;

    [SerializeField, Required]
    private GameObject[] visualObjects = Array.Empty<GameObject>();

    [SerializeField, Tooltip("Check distance interval (seconds)")]
    private float distanceCheckInterval = 0.2f;

    [SerializeField, Tooltip("Keep the entity visible until a local player is resolved")]
    private bool visibleWithoutLocalPlayer = true;
    private LocalPlayerSystem localPlayerSystem;   
    private bool isVisible = true;
    private EntityRef cachedLocalPlayer = EntityRef.None;
    private byte cachedPlayerTeamIndex;
    private byte cachedEntityTeamIndex;
    private bool hasPlayerTeam;
    private bool hasEntityTeam;
    private bool isSameTeam;
    private float nextDistanceCheckTime;

    protected bool HasLocalPlayer => cachedLocalPlayer != EntityRef.None;
    protected EntityRef LocalPlayer => cachedLocalPlayer;
    protected bool IsSameTeam => isSameTeam;
    protected bool IsCurrentlyVisible => isVisible;
    protected float DistanceCheckInterval => distanceCheckInterval;
    protected bool VisibleWhenNoLocalPlayer => visibleWithoutLocalPlayer;

    public override void OnActivate(Frame frame) {
      base.OnActivate(frame);

      localPlayerSystem = App.Get<LocalPlayerSystem>();
      
      RegisterQuantumEventHandlers();
      RefreshLocalContext(frame);
      UpdateVisibility(frame);
    }

    public override void OnDeactivate() {
      QuantumEvent.UnsubscribeListener(this);
      base.OnDeactivate();
    }

    public override void OnUpdateView() {
      base.OnUpdateView();      

      var frame = VerifiedFrame;
      if (frame == null) {
        return;
      }

      if (HasLocalPlayer && !frame.Exists(LocalPlayer)) {
        RefreshLocalContext(frame);
      }

      if (!HasLocalPlayer) {
        if (VisibleWhenNoLocalPlayer) {
          SetVisibility(true);
        }

        return;
      }

      if (IsSameTeam) {
        SetVisibility(true);
        return;
      }

      if (!ShouldEvaluateThisFrame()) {
        return;
      }

      UpdateVisibility(frame);
    }

    protected void UpdateVisibility(Frame frame) {
      if (!HasLocalPlayer) {
        if (VisibleWhenNoLocalPlayer) {
          SetVisibility(true);
        }
        return;
      }

      if (IsSameTeam) {
        SetVisibility(true);
        return;
      }

      var shouldBeVisible = EvaluateVisibility(frame, LocalPlayer);
      SetVisibility(shouldBeVisible);
    }

    protected void SetVisibility(bool visible) {
      if (isVisible == visible) {
        return;
      }

      isVisible = visible;

      if (visualObjects != null) {
        for (var i = 0; i < visualObjects.Length; ++i) {
          var obj = visualObjects[i];
          if (obj != null) {
            obj.SetActive(visible);
          }
        }
      }

      OnVisibilityChanged(visible);
    }

    protected void ResetDistanceCheckTimer() {
      nextDistanceCheckTime = 0f;
    }

    protected virtual void OnVisibilityChanged(bool visible) {
    }

    protected virtual void OnLocalPlayerChanged(EntityRef previous, EntityRef current) {
    }

    protected virtual void RegisterQuantumEventHandlers() {
    }

    protected abstract bool EvaluateVisibility(Frame frame, EntityRef localPlayer);

    protected virtual unsafe EntityRef ResolveLocalPlayer(Frame frame) {
      if (localPlayerSystem != null) {
        if (!localPlayerSystem.HasNotLocalEntityRef(out var localRef) && localRef != EntityRef.None) {
          if (frame.Exists(localRef)) {
            return localRef;
          }
        }
      }

      var game = QuantumRunner.Default?.Game;
      if (game == null) {
        return EntityRef.None;
      }

      var filter = frame.Filter<Unit, Team>();
      while (filter.NextUnsafe(out var entity, out var unit, out _)) {
        if (game.PlayerIsLocal(unit->PlayerRef)) {
          return entity;
        }
      }

      return EntityRef.None;
    }

    private bool ShouldEvaluateThisFrame() {
      var now = Time.unscaledTime;
      if (now < nextDistanceCheckTime) {
        return false;
      }

      nextDistanceCheckTime = now + Mathf.Max(distanceCheckInterval, MinDistanceCheckInterval);
      return true;
    }

    private unsafe void RefreshLocalContext(Frame frame) {
      var previousLocalPlayer = cachedLocalPlayer;

      cachedLocalPlayer = ResolveLocalPlayer(frame);
      hasPlayerTeam = false;
      hasEntityTeam = false;
      cachedPlayerTeamIndex = default;
      cachedEntityTeamIndex = default;

      if (cachedLocalPlayer != EntityRef.None) {
        if (frame.Unsafe.TryGetPointer<Team>(cachedLocalPlayer, out var playerTeam)) {
          cachedPlayerTeamIndex = playerTeam->Index;
          hasPlayerTeam = true;
        }
      }

      if (frame.Unsafe.TryGetPointer<Team>(EntityRef, out var entityTeam)) {
        cachedEntityTeamIndex = entityTeam->Index;
        hasEntityTeam = true;
      }

      isSameTeam = hasPlayerTeam && hasEntityTeam && cachedPlayerTeamIndex == cachedEntityTeamIndex;
      ResetDistanceCheckTimer();

      if (previousLocalPlayer != cachedLocalPlayer) {
        OnLocalPlayerChanged(previousLocalPlayer, cachedLocalPlayer);
      }
    }
  }
}
