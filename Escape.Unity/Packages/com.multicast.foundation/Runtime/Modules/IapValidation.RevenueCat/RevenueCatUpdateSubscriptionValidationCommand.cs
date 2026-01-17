namespace Multicast.Modules.IapValidation.RevenueCat {
    using Multicast;

    public readonly struct RevenueCatUpdateSubscriptionValidationCommand : ICommand {
        public string TrialReceipts   { get; }
        public long   LastUpdateTicks { get; }

        private RevenueCatUpdateSubscriptionValidationCommand(string trialReceipts, long lastUpdateTicks) {
            this.TrialReceipts   = trialReceipts;
            this.LastUpdateTicks = lastUpdateTicks;
        }

        public static RevenueCatUpdateSubscriptionValidationCommand SetNewTrialReceipt(string trialReceipts)
            => new(trialReceipts, 0);

        public static RevenueCatUpdateSubscriptionValidationCommand SetSubscriptionTicks(long updateTicks)
            => new(null, updateTicks);
    }

    [SkipInstallWithoutDependency(typeof(SdkInitializationMarkers.RevenueCat))]
    public class RevenueCatUpdateSubscriptionValidationCommandHandler : ICommandHandler<RevenueCatUpdateSubscriptionValidationCommand> {
        [Inject] private readonly UdRevenueCatValidationRepo validation;

        public void Execute(CommandContext context, RevenueCatUpdateSubscriptionValidationCommand command) {
            this.validation.ValidatedTrialReceipts = command.TrialReceipts;

            if (command.LastUpdateTicks > 0) {
                this.validation.LastSubscriptionSentTicks = command.LastUpdateTicks;
            }
        }
    }
}