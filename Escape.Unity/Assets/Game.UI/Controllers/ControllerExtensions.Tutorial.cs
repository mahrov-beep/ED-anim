namespace Game.UI.Controllers {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using Tutorial;

    public static partial class ControllerExtensions {
        public static async UniTask<IControllerBase> RunTutorialOnFlow(this ControllerBase.Context context,
            Func<BaseTutorialSequence, Func<ControllerBase.Context, UniTask>> call) {
            return await context.RunChild(new TutorialRunOnFlowControllerArgs {
                Call = call,
            });
        }
    }
}