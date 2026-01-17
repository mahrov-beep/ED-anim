namespace Game.UI.Controllers.Features.Friends {
    using System;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using Multicast;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    public struct FriendsFeatureControllerArgs : IFlowControllerArgs { }

    public class FriendsFeatureController : FlowController<FriendsFeatureControllerArgs> {
        [CanBeNull] private IControllerBase controller;

        protected override async UniTask Activate(Context context) {
            FriendsFeatureEvents.Open.Listen(this.Lifetime, () => this.RequestFlow(this.Open));
            FriendsFeatureEvents.OpenIncoming.Listen(this.Lifetime, () => this.RequestFlow(this.OpenIncoming));
        }

        private async UniTask Open(Context context) {
            if (this.controller is { IsRunning: true }) {
                return;
            }

            this.controller = await context.RunChild(new FriendsControllerArgs());
        }

        private async UniTask OpenIncoming(Context context) {
            if (this.controller is { IsRunning: true }) {
                return;
            }

            this.controller = await context.RunChild(new FriendsControllerArgs());
        }

    }
}