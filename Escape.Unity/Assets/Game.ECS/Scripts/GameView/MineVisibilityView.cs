namespace _Project.Scripts.GameView {
  using Quantum;
  using UnityEngine;
  public class MineVisibilityView : TeamVisibilityViewBase {
    protected override void RegisterQuantumEventHandlers() {
      QuantumEvent.Subscribe<EventMineExploded>(this, OnMineExploded);
    }

    protected override unsafe bool EvaluateVisibility(Frame frame, EntityRef localPlayer) {
      if (!frame.Unsafe.TryGetPointer<Mine>(EntityRef, out var mine)) {
        return false;
      }

      var mineTransform = frame.Unsafe.GetPointer<Transform3D>(EntityRef);
      var playerTransform = frame.Unsafe.GetPointer<Transform3D>(localPlayer);
      var offset = mineTransform->Position - playerTransform->Position;
      var distanceSq = offset.SqrMagnitude;
      var visibilityDistSq = mine->VisibilityDistance * mine->VisibilityDistance;

      return distanceSq <= visibilityDistSq;
    }

    private void OnMineExploded(EventMineExploded e) {
      if (e.mineRef == EntityRef) {
        SetVisibility(false);        
      }
    }
  }
}
