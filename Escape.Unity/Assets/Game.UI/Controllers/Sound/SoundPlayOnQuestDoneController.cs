namespace Game.UI.Controllers.Sound {
    using System;
    using Cysharp.Threading.Tasks;
    using Domain;
    using Multicast;
    using Quantum;
    using SoundEffects;

    public class SoundPlayOnQuestDoneController : FlowController<SoundPlayOnQuestDoneControllerArgs> {
        [Inject] private ISoundEffectService soundEffectService;

        protected override async UniTask Activate(Context context) {
            this.Lifetime.Register(QuantumEvent.SubscribeManual<EventQuestCounterTaskDone>(this.OnTaskDone));
        }

        private void OnTaskDone(EventQuestCounterTaskDone callback) {
            this.soundEffectService.PlayOneShot(CoreConstants.SoundEffectKeys.QUEST_DONE);
        }
    }

    [Serializable, RequireFieldsInit]
    public struct SoundPlayOnQuestDoneControllerArgs : IFlowControllerArgs {
    }
}