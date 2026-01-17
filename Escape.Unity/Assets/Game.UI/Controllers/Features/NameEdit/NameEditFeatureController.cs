namespace Game.UI.Controllers.Features.NameEdit {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;

    [Serializable, RequireFieldsInit]
    public struct NameEditFeatureControllerArgs : IFlowControllerArgs {
    }

    public class NameEditFeatureController : FlowController<NameEditFeatureControllerArgs> {
        protected override async UniTask Activate(Context context) {
            NameEditFeatureEvents.Prompt.Listen(this.Lifetime, () => this.RequestFlow(this.OpenEditNamePrompt));
        }

        private async UniTask OpenEditNamePrompt(Context context) {
            await context.RunChild(new NameEditPromptControllerArgs());
        }
    }
}