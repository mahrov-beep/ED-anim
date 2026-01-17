namespace Game.UI.Controllers.Features.Quest {
    using System;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using Multicast;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    public struct QuestFeatureControllerArgs : IFlowControllerArgs {
    }

    public class QuestFeatureController : FlowController<QuestFeatureControllerArgs> {
        [CanBeNull] private IControllerBase questsScreenController;

        protected override async UniTask Activate(Context context) {
            await context.RunChild(new QuestReportCompletionToServerControllerArgs());

            QuestFeatureEvents.Open.Listen(this.Lifetime, () => this.RequestFlow(this.OpenQuestsScreen));
        }

        private async UniTask OpenQuestsScreen(Context context) {
            if (this.questsScreenController is { IsRunning: true }) {
                Debug.LogError($"[{this}] QuestsScreen already opened");
                return;
            }

            this.questsScreenController = await context.RunChild(new QuestMenuControllerArgs());
        }
    }
}