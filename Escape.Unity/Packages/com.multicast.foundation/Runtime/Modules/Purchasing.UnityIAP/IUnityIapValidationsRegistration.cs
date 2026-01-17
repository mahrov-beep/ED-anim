#if UNITY_PURCHASING
namespace Multicast.Modules.Purchasing.UnityIAP {
    public interface IUnityIapValidationsRegistration {
        void RegisterValidator(IUnityIapValidationProvider validationProvider);
    }
}
#endif