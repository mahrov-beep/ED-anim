namespace Game.UI.Views {
    using Multicast;
    using UniMob.UI;
    using UnityEngine;

    public class SlowDebuffView : AutoView<ISlowDebuffViewState> {
        [SerializeField]
        private Vector3 worldOffset = Vector3.zero;

        protected override AutoViewVariableBinding[] Variables => new[] {
                        this.Variable("has_debuff",
                                        () => State.HasDebuff),
                        this.Variable("stack_count",
                                        () => State.StackCount.ToString()),
        };
    }

    public interface ISlowDebuffViewState : IViewState {
        int     StackCount { get; }

        bool HasDebuff => StackCount > 0;
    }
}