namespace Quantum {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using JetBrains.Annotations;
  using Sirenix.OdinInspector;
  using UnityEngine;

  public static class ItemAssetDropdown {
    public const string ODIN_METHOD                  = "@Quantum.ItemAssetDropdown.GetItemAssets()";
    public const string ODIN_METHOD_ATTACHMENTS_ONLY = "@Quantum.ItemAssetDropdown.GetWeaponAttachmentsAssets()";

    [UsedImplicitly]
    public static IEnumerable<ValueDropdownItem<AssetRef<ItemAsset>>> GetItemAssets() {
      return GetAssets<ItemAsset>(asset => $"{asset.Grouping} / {asset.ItemKey}");
    }

    [UsedImplicitly]
    public static IEnumerable<ValueDropdownItem<AssetRef<WeaponAttachmentItemAsset>>> GetWeaponAttachmentsAssets() {
      return GetAssets<WeaponAttachmentItemAsset>(asset => $"{asset.inspectorShortName} - {asset.ItemKey}");
    }

    static IEnumerable<ValueDropdownItem<AssetRef<T>>> GetAssets<T>(Func<T, string> name) where T : AssetObject {
      if (Application.isEditor) {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.FindAssets("t: " + typeof(T).FullName)
                .Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
                .Select(UnityEditor.AssetDatabase.LoadAssetAtPath<T>)
                .Select(asset => new ValueDropdownItem<AssetRef<T>>(name(asset), asset));
#endif
      }

      return Array.Empty<ValueDropdownItem<AssetRef<T>>>();
    }
  }
}