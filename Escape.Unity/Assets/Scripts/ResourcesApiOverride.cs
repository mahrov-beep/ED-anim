using System;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

public class ResourcesApiOverride : ResourcesAPI {
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
#else
    [RuntimeInitializeOnLoadMethod]
#endif
    private static void Setup() {
        ResourcesAPI.overrideAPI = new ResourcesApiOverride();
    }

    private bool loading; // Addressables can load from Resources -> recursion

    protected override Object Load(string path, Type systemTypeInstance) {
        var result = base.Load(path, systemTypeInstance);

        if (this.loading || result != null) {
            return result;
        }

        if (systemTypeInstance == typeof(TMP_Settings)) {
            return this.LoadFromAddressables<TMP_Settings>("TMP_Settings");
        }

        return null;
    }

    private T LoadFromAddressables<T>(string path) {
        try {
            this.loading = true;
            return Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
        }
        finally {
            this.loading = false;
        }
    }
}