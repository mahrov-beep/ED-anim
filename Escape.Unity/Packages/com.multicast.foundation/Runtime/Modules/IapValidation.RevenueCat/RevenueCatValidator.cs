#if REVENUE_CAT_SDK
namespace Multicast.Modules.IapValidation.RevenueCat {
    using Purchasing.UnityIAP;
    using UnityEngine.Purchasing;

    public class RevenueCatValidator : IUnityIapValidationProvider {
        private readonly MulticastRevenueCatListener listener;

        public RevenueCatValidator(MulticastRevenueCatListener listener) {
            this.listener = listener;
        }

        public string Validate(Product product) {
            return this.listener.Validate(
                storeSpecificId: product.definition.storeSpecificId,
                receipt: product.receipt
            );
        }
    }
}
#endif