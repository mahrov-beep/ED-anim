namespace InfimaGames.LowPolyShooterPack {
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UnityEngine;

    internal static class LayersUtil {
        public static void SetLayerRecursively(Transform obj, int layer) {
            obj.gameObject.layer = layer;

            for (var i = 0; i < obj.childCount; i++) {
                SetLayerRecursively(obj.GetChild(i), layer);
            }
        }

        public static bool ValidateLayer(int layer, ref string error, ref InfoMessageType type) {
            var layerName = LayerMask.LayerToName(layer);

            if (string.IsNullOrEmpty(layerName)) {
                type  = InfoMessageType.Error;
                error = $"Layer '{layer}' does not exist";
                return false;
            }

            return true;
        }

        public static IEnumerable<ValueDropdownItem<int>> GetLayerDropdown() {
            for (var i = 0; i < 32; i++) {
                var layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName)) {
                    yield return new ValueDropdownItem<int>(layerName, i);
                }
            }
        }
    }
}