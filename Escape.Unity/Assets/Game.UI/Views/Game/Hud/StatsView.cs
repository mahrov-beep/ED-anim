namespace Game.UI.Views.Game.Hud {
    using Multicast;
    using UniMob;
    using UniMob.UI;
    using UnityEngine;

    public class StatsView : AutoView<IStatsViewState> {
        private readonly MutableAtom<int> fpsAtom  = Atom.Value(60);
        private readonly MutableAtom<int> pingAtom = Atom.Value(0);

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("fps", () => this.fpsAtom.Value, 60),
            this.Variable("ping", () => this.pingAtom.Value, 0),
        };

        private void Update() {
            if (!this.HasState) {
                return;
            }

            var currentFps = Mathf.RoundToInt(1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f));
            var currentPing = this.State.Ping;

            if (currentFps != this.fpsAtom.Value) {
                this.fpsAtom.Value = currentFps;
            }

            if (currentPing != this.pingAtom.Value) {
                this.pingAtom.Value = currentPing;
            }
        }
    }

    public interface IStatsViewState : IViewState {
        int Ping { get; }
    }
}
