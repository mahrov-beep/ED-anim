namespace Game.UI.Views.Common {
    using System;
    using UniMob.UI;
    using Multicast;
    using UnityEngine;

    public class ProgressScreenView : AutoView<IProgressScreenState> {
        private readonly DotsAnimator dots = new DotsAnimator();

        protected override AutoViewVariableBinding[] Variables => new[] {
            this.Variable("dots", () => this.State.AnimateDots ? this.dots.Value : "", "..."),
            this.Variable("progress", () => this.State.Progress, 0.75f),
            this.Variable("message", () => this.State.Message, "LOADING"),
            this.Variable("parameters", () => this.State.Parameters, ""),
        };

        protected override void Activate() {
            base.Activate();

            this.dots.Activate(this.HasState && this.State.AnimateDots);
        }

        private void Update() {
            if (!this.HasState) {
                return;
            }

            this.dots.Update(this.State.AnimateDots);
        }
    }

    public interface IProgressScreenState : IViewState {
        float  Progress    { get; }
        string Message     { get; }
        string Parameters  { get; }
        bool   AnimateDots { get; }
    }
}