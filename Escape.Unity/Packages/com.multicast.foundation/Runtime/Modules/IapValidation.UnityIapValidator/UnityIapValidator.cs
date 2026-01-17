#if UNITY_PURCHASING
namespace Multicast.Modules.IapValidation.UnityIapValidator {
    using System.Linq;
    using Purchasing.UnityIAP;
    using UnityEngine;
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Security;

    public class UnityIapValidator : IUnityIapValidationProvider {
        private readonly CrossPlatformValidator crossPlatformValidator;

        public UnityIapValidator(UnityIapValidatorTangle tangle) {
            this.crossPlatformValidator = new CrossPlatformValidator(
                tangle.GooglePlayTangle,
                tangle.AppleTangle,
                Application.identifier
            );
        }

        public string Validate(Product product) {
            try {
                var result = this.crossPlatformValidator.Validate(product.receipt);
                var valid  = result.Any(o => o.productID == product.definition.storeSpecificId);

                return valid ? null : "invalid product";
            }
            catch (IAPSecurityException) {
                return "iap security";
            }
        }
    }
}
#endif