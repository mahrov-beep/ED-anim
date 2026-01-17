namespace Multicast {
    using System;
    using System.Linq;
    using Multicast;
    using Multicast.Diagnostics;
    using Multicast.GameProperties;
    using UnityEngine;

    [Serializable, RequireFieldsInit] internal struct ConfigureUserDataForModelsCommand : ICommand {
    }

    internal class ConfigureUserDataForModelsCommandHandler : ICommandHandler<ConfigureUserDataForModelsCommand> {
        public void Execute(CommandContext context, ConfigureUserDataForModelsCommand command) {
            var models = App.Current.Container.OfType<IModelWithUserDataConfigurator>().ToList();

            foreach (var model in models) {
                model.ConfigureUserData();
            }
        }
    }
}