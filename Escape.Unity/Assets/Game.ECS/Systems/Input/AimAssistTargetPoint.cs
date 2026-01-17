namespace QuantumUser.Unity {
  using System;
  using Photon.Deterministic;
  using Quantum;
  using Sirenix.OdinInspector;
  using UnityEngine;

  public class AimAssistTargetPoint : QuantumEntityViewComponent {
    [SerializeField, Required] Collider aimAssistTargetCollider;

    IDisposable     localPlayerAddSubscription;
    CapsuleCollider capsuleCollider;
    Vector3         baseCenter;
    float           baseHeight;
    float           lastAppliedRatio = -1f;

    public override void OnActivate(Frame f) {
      base.OnActivate(f);

      this.CacheColliderData();
      this.ApplyHeightRatio(1f, force: true);
      this.RefreshViewIsLocal(f);

      this.localPlayerAddSubscription = QuantumCallback.SubscribeManual(this, (CallbackLocalPlayerAddConfirmed e)
        => this.RefreshViewIsLocal(e.Frame));
    }

    public override void OnDeactivate() {
      base.OnDeactivate();

      this.ApplyHeightRatio(1f, force: true);
      this.localPlayerAddSubscription.Dispose();
    }

    public override void OnUpdateView() {
      base.OnUpdateView();

      var frame = this.PredictedFrame ?? this.VerifiedFrame;
      if (frame == null || this.EntityRef == EntityRef.None) {
        return;
      }

      this.CacheColliderData();
      if (this.capsuleCollider == null || this.baseHeight <= Mathf.Epsilon) {
        return;
      }

      var currentHeight = UnitColliderHeightHelper.GetCurrentHeight(frame, this.EntityRef);
      if (currentHeight > FP._0) {
        this.ApplyHeightAbsolute(currentHeight.AsFloat);
      }
      else {
        FP ratioFP = UnitColliderHeightHelper.GetCurrentHeightRatio(frame, this.EntityRef);
        this.ApplyHeightRatio(Mathf.Clamp(ratioFP.AsFloat, 0.1f, 1f));
      }
    }

    void RefreshViewIsLocal(Frame f) {
      if (!f.TryGet(this.EntityRef, out Unit unit)) {
        return;
      }

      if (this.Game.PlayerIsLocal(unit.PlayerRef)) {
        // deactivate AimAssistTarget collider for local player
        this.aimAssistTargetCollider.enabled = false;
      }
    }

    void CacheColliderData() {
      this.capsuleCollider ??= this.aimAssistTargetCollider as CapsuleCollider;

      if (this.capsuleCollider == null || this.baseHeight > 0f) {
        return;
      }

      this.baseHeight = this.capsuleCollider.height;
      this.baseCenter = this.capsuleCollider.center;
    }

    void ApplyHeightRatio(float ratio, bool force = false) {
      if (this.capsuleCollider == null || this.baseHeight <= Mathf.Epsilon) {
        return;
      }

      ratio = Mathf.Clamp(ratio, 0.1f, 1f);
      if (!force && Mathf.Abs(ratio - this.lastAppliedRatio) < 0.001f) {
        return;
      }

      var radius           = this.capsuleCollider.radius;
      var targetHeight     = Mathf.Max(radius * 2f, this.baseHeight * ratio);
      var baseBottomOffset = this.baseCenter.y - this.baseHeight * 0.5f;
      var targetCenter     = this.baseCenter;
      targetCenter.y       = baseBottomOffset + targetHeight * 0.5f;

      this.capsuleCollider.height = targetHeight;
      this.capsuleCollider.center = targetCenter;
      this.lastAppliedRatio       = ratio;
    }

    void ApplyHeightAbsolute(float height, bool force = false) {
      if (this.capsuleCollider == null || this.baseHeight <= Mathf.Epsilon) {
        return;
      }

      var radius = this.capsuleCollider.radius;
      var targetHeight = Mathf.Max(radius * 2f, height);
      if (targetHeight <= Mathf.Epsilon) {
        return;
      }

      var ratio = Mathf.Clamp(targetHeight / this.baseHeight, 0.1f, 1f);
      if (!force && Mathf.Abs(ratio - this.lastAppliedRatio) < 0.001f) {
        return;
      }

      var baseBottomOffset = this.baseCenter.y - this.baseHeight * 0.5f;
      var targetCenter     = this.baseCenter;
      targetCenter.y       = baseBottomOffset + targetHeight * 0.5f;

      this.capsuleCollider.height = targetHeight;
      this.capsuleCollider.center = targetCenter;
      this.lastAppliedRatio       = ratio;
    }
  }
}
