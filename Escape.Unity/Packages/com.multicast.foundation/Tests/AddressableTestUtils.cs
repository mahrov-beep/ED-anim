namespace Multicast {
    using UnityEngine;
    using Utilities;

    public static class AddressableTestUtils {
        public static void AssertAddressableExists(string path, string message) {
            var asset = EditorAddressablesUtils.LoadAddressable(path);
            if (asset == null) {
                Debug.LogError($"Addressable '{path}' not exists: {message}");
            }
        }
    }
}