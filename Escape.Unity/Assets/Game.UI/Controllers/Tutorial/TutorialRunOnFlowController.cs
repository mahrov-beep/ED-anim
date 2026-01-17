namespace Game.UI.Controllers.Tutorial {
    using System;
    using System.Reflection;
    using Cysharp.Threading.Tasks;
    using Multicast;

    [RequireFieldsInit]
    public struct TutorialRunOnFlowControllerArgs : IFlowControllerArgs {
        public Func<BaseTutorialSequence, Func<ControllerBase.Context, UniTask>> Call;
    }

    public class TutorialRunOnFlowController : FlowController<TutorialRunOnFlowControllerArgs> {
        [Inject] private TutorialService tutorialService;

        private Func<Context, UniTask> tutorialCall;

        private string debugName;

#if UNITY_EDITOR
        public override string DebugName {
            get {
                if (this.debugName != null) {
                    return this.debugName;
                }

                var call = this.Args.Call(this.tutorialService);

                return this.debugName = $"OnFlow Tutorial: {call.GetMethodInfo().Name}";
            }
        }
#endif

        protected override async UniTask Activate(Context context) {
            this.tutorialCall = this.Args.Call(this.tutorialService);
        }

        protected override async UniTask OnFlow(Context context) {
            await this.tutorialCall(context);
        }
    }
}