namespace Game.UI.Controllers.Sound {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;

    public class SoundController : FlowController<SoundControllerArgs> {
        protected override async UniTask Activate(Context context) {
            await context.RunChild(new SoundPlayOnQuestDoneControllerArgs());
        }
    }

    [Serializable, RequireFieldsInit]
    public struct SoundControllerArgs : IFlowControllerArgs {
    }
}