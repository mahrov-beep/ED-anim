namespace Multicast.GameProperties {
    using System;

    public readonly struct SyncGamePropertiesCommand : ICommand {
    }

    internal class SyncGamePropertiesCommandHandler : ICommandHandler<SyncGamePropertiesCommand> {
        [Inject] private readonly GamePropertiesModel gamePropertiesModel;

        public void Execute(CommandContext context, SyncGamePropertiesCommand command) {
            var tries = 50;
            while (tries-- > 0) {
                if (this.gamePropertiesModel.IsSynced) {
                    break;
                }

                foreach (var property in this.gamePropertiesModel.AutoSyncedProperties) {
                    if (property.IsSynced) {
                        continue;
                    }

                    property.SyncIfRequired();
                }
            }

            if (!this.gamePropertiesModel.IsSynced) {
                throw new Exception("Failed to sync GameProperties");
            }
        }
    }
}