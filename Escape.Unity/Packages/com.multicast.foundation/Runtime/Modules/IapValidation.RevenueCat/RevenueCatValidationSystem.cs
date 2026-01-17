#if REVENUE_CAT_SDK
namespace Multicast.Modules.IapValidation.RevenueCat {
    using Scellecs.Morpeh;
    using UnityEngine;

    public class RevenueCatValidationSystem : SystemBase {
        public MulticastRevenueCatListener Listener { get; set; }

        public override void OnAwake() {
        }

        public override void OnUpdate(float deltaTime) {
            if (this.Listener == null) {
                return;
            }

            if (!this.Listener.NeedToValidatePurchases()) {
                return;
            }

            this.Listener.ValidatePurchases();
        }
    }
}
#endif