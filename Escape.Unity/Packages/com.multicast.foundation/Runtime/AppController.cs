namespace Multicast {
    using System;
    using Cysharp.Threading.Tasks;
    using Multicast;

    [Serializable, RequireFieldsInit]
    public struct AppMainControllerArgs : IFlowControllerArgs {
    }

    [Serializable, RequireFieldsInit]
    public struct AppBootControllerArgs : IFlowControllerArgs {
    }

    [Serializable, RequireFieldsInit]
    public struct AppQuitControllerArgs : IResultControllerArgs {
    }
}