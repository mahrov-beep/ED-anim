namespace Multicast {
    using Collections;
    using UnityEngine;
    using Utilities;

    public class EditorAddressablesCache<T>
        where T : Object {
        public static readonly IEnumerableCache<T> Instance = new FuncEnumerableCache<T>(
            EditorAddressablesUtils.LoadAddressable<T>,
            EditorAddressablesUtils.EnumeratePaths
        );
    }
}