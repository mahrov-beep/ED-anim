namespace _Project.Scripts.Unit {
    using GameView;
    using Photon.Deterministic;
    using Quantum;
    using UnityEngine;

    public class CinemachineImpulseView : QuantumViewComponent<CustomViewContext> {
        private void OnEnable() {
            QuantumEvent.Subscribe(this, (EventOnShoot evt) => this.OnShootPredicted(evt), onlyIfActiveAndEnabled: true);
            QuantumEvent.Subscribe(this, (EventAttackHitSynced evt) => this.OnHitVerified(evt), onlyIfActiveAndEnabled: true);
        }

        private void OnShootPredicted(EventOnShoot evt) {
            var f = evt.Game.Frames.Predicted;

            if (!f.TryGet(evt.unitRef, out Unit unit)) {
                return;
            }

            if (!f.Context.IsLocalPlayer(unit.PlayerRef)) {
                return;
            }

            if (unit.GetActiveWeaponConfig(f) is not { } weaponConfig) {
                return;
            }

            if (!weaponConfig.shotImpulse.IsValid) {
                return;
            }

            var shotImpulse = f.FindAsset(weaponConfig.shotImpulse);
            var position    = UnitHelper.GetPosition(f, evt.unitRef);
            var impulse     = shotImpulse.impulsePower * (FP._1 * unit.CurrentStats.shotImpulse);

            shotImpulse.impulse.CreateEvent(position.ToUnityVector3(), impulse.ToUnityVector3());
        }

        private void OnHitVerified(EventAttackHitSynced evt) {
            var f = evt.Game.Frames.Predicted;

            if (!f.TryGet(evt.targetRef, out Unit unit)) {
                return;
            }

            if (!f.Context.IsLocalPlayer(unit.PlayerRef)) {
                return;
            }

            if (!evt.attackAsset.hitImpulse.IsValid) {
                return;
            }

            var hitImpulse = f.FindAsset(evt.attackAsset.hitImpulse);
            var position = evt.hitPoint.ToUnityVector3();

            hitImpulse.impulse.CreateEvent(position, hitImpulse.impulsePower.ToUnityVector3());
        }
    }
}