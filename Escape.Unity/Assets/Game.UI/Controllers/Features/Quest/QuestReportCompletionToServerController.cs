namespace Game.UI.Controllers.Features.Quest {
    using System;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Multicast;
    using Quantum;
    using Services.Photon;
    using Shared.DTO;

    [Serializable, RequireFieldsInit]
    public struct QuestReportCompletionToServerControllerArgs : IFlowControllerArgs {
    }

    public class QuestReportCompletionToServerController : FlowController<QuestReportCompletionToServerControllerArgs> {
        [Inject] private PhotonService photonService;

        protected override async UniTask Activate(Context context) {
            this.Lifetime.Register(QuantumEvent.SubscribeManual<EventQuestCounterTaskDone>(this.OnTaskDone));
        }

        private void OnTaskDone(EventQuestCounterTaskDone evt) {
            App.Server.GameReportQuestCounterTask(new ReportGameQuestCounterTaskRequest {
                GameId = this.photonService.CurrentGameId,
                TargetUserIds = evt.PhotonActorNr is { } actorNr
                    ? new[] { this.photonService.GetUserIdByActorId(actorNr) }
                    : this.photonService.CurrentGameUsers.Keys.Select(this.photonService.GetUserIdByActorId).ToArray(),
                Property     = evt.Property,
                CounterValue = evt.Value,
                Filters      = evt.Filters,
            }, ServerCallRetryStrategy.Throw).Forget();
        }
    }
}