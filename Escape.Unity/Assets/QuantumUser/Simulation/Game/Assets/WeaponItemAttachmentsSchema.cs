namespace Quantum {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Sirenix.OdinInspector;

  [Serializable]
  public class WeaponItemAttachmentsSchema : AssetObject {
    public WeaponAttachmentSlots[] slots = Array.Empty<WeaponAttachmentSlots>();

    [RequiredRef, ValueDropdown(ItemAssetDropdown.ODIN_METHOD_ATTACHMENTS_ONLY)]
    [Help("Определяет какие обвесы можно добавлять на набор оружий, которые используют данную схему")]
    [InlinePingButton]
    public AssetRef<WeaponAttachmentItemAsset>[] attachments;

    public int itemsCountOnFirstLineInInventory = 1000;

    public const string ALL_SCHEMAS_ODIN_PATH = "@WeaponItemAttachmentsSchema.EditorOnlyAllSchemas";

#if UNITY_EDITOR
    public static IEnumerable<ValueDropdownItem<WeaponItemAttachmentsSchema>> EditorOnlyAllSchemas =>
      UnityEditor.AssetDatabase.FindAssets("t: " + typeof(WeaponItemAttachmentsSchema).FullName)
        .Select(UnityEditor.AssetDatabase.GUIDToAssetPath)
        .Select(UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponItemAttachmentsSchema>)
        .Select(it => new ValueDropdownItem<WeaponItemAttachmentsSchema>(it.name, it));
#endif
  }
}