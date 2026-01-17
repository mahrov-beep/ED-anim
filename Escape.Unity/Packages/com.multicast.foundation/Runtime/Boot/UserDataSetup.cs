namespace Multicast.Boot {
    using Numerics;
    using UnityEngine.Scripting;
    using UserData;

    internal static class UserDataSetup {
        [Preserve]
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        private static void RegisterDefaultValue() {
            UdValue<BigDouble>.DefaultValue          = () => BigDouble.Zero;
            UdValue<ProtectedBigDouble>.DefaultValue = () => BigDouble.Zero;
            UdValue<ProtectedInt>.DefaultValue       = () => 0;

            UdValue<BigDouble>.CustomToString          = it => BigString.ToString(it.Value);
            UdValue<ProtectedBigDouble>.CustomToString = it => BigString.ToString(it.Value);
            UdValue<ProtectedInt>.CustomToString       = it => it.Value.ToString();
        }
    }
}