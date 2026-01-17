namespace Game.UI.Controllers.Tutorial {
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using Multicast;
    using Multicast.Analytics;
    using Shared.UserProfile.Commands.Tutorials;
    using Shared.UserProfile.Data;

    public abstract class BaseMultiStepTutorialSequence<TStep> : BaseTutorialSequence where TStep : struct, Enum {
        [Inject] protected IAnalytics    Analytics;
        [Inject] protected SdUserProfile UserProfile;

        private readonly TutorialStepHighlighter<TStep> highlighter;

        [PublicAPI] protected TStep? CurrentStep            => this.highlighter.CurrentStep;
        [PublicAPI] protected bool   IsTutorialCompleted    => this.UserProfile.Tutorials.IsCompleted(this.TutorialKey);
        [PublicAPI] protected bool   IsTutorialNotCompleted => !this.IsTutorialCompleted;

        public override bool IsActive => this.highlighter.CurrentStep.HasValue;

        protected abstract string TutorialKey { get; }

        protected abstract Dictionary<TStep, TutorialStepHighlight> Steps { get; }

        protected BaseMultiStepTutorialSequence() {
            // ReSharper disable once VirtualMemberCallInConstructor
            this.highlighter = new TutorialStepHighlighter<TStep>(this.Steps);
        }

        public override void ForceComplete() {
            this.highlighter.ChangeStep(null);
        }

        [PublicAPI]
        protected void ChangeStep(TStep? newStep) {
            this.highlighter.ChangeStep(newStep);

            this.Analytics.Send("tutorial_step",
                new AnalyticsArg("tutorial_key", this.TutorialKey) {
                    new AnalyticsArg("step", newStep is { } s ? EnumNames<TStep>.GetName(s) : "none"),
                }
            );
        }

        [PublicAPI]
        protected async UniTask ChangeStepAndShowPopup(ControllerBase.Context context, TStep newStep) {
            this.ChangeStep(newStep);

            await context.RootNavigator.PushTutorialPopup(this.TutorialKey, newStep);
        }

        [PublicAPI]
        protected async UniTask SetTutorialCompleted(ControllerBase.Context context) {
            this.Analytics.Send("tutorial_complete",
                new AnalyticsArg("tutorial_key", this.TutorialKey)
            );

            await context.Server.ExecuteUserProfile(new UserProfileCompleteTutorialCommand {
                TutorialId = this.TutorialKey,
            }, ServerCallRetryStrategy.RetryWithUserDialog);
        }
    }
}