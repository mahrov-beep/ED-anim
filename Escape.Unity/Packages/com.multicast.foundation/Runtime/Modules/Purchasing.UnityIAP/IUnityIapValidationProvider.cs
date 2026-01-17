#if UNITY_PURCHASING
namespace Multicast.Modules.Purchasing.UnityIAP {
    using UnityEngine.Purchasing;

    public interface IUnityIapValidationProvider {
        string Validate(Product product);
    }
}
#endif