namespace Quantum {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using JetBrains.Annotations;
  using Photon.Deterministic;
  using Sirenix.OdinInspector;
  using UnityEngine;
  using UnityEngine.Serialization;

  [Serializable]
  public abstract unsafe class ItemAsset : AssetObject {
    public static QuantumDefCache<ItemAsset, IItemDef> DefCache;

    [PropertySpace]
    [SerializeField, Help("Маленькая квадратная иконка")]
    string icon;

    [SerializeField, Help("Большая иконка. В основном используется для отображения оружия в полном размере")]
    string iconLarge;

    // ReSharper disable once InconsistentNaming
    public ERarityType rarity => this.Def.Rarity;

    [PropertySpace]
    [Help("Выпадает ли данный предмет из персонажа при его смерти?")]
    [ToggleLeft]
    public bool dropOnDeath = true;
    
    [Help("Нужно ли вернуть предмет обратно в лоадаут игрока при неудачном extraction")]
    [ToggleLeft]
    public bool addToLoadoutAfterFail = false;

    [Help("Если включено, то этот предмет не будет выпадать из ItemDropBuilder. НЕ влияет на другие источники")]
    [ToggleLeft]
    public bool excludeFromDropBuilders = false;
    
    [Help("Если включено, то этот предмет не будет появляться в ItemBox в сцене")]
    [ToggleLeft]
    public bool excludeFromItemBoxes = false;

    // ReSharper disable once InconsistentNaming
    public FP weight => this.Def.Weight;

    public int Height => this.Def.CellsHeight;
    public int Width  => this.Def.CellsWidth;
    
    [PropertySpace]
    [FormerlySerializedAs("attachmentRarityEffects")]
    [Help("Постоянные эффекты которые накладываются на персонажа когда предмет экипирован")]
    public AssetRef<ItemRarityEffectsConfig> rarityEffects;

    [TableList(ShowPaging = false, AlwaysExpanded = true)]
    [Help("Постоянные эффекты которые накладываются на персонажа когда предмет экипирован")]
    public List<PersistentItemEffect> persistentEffects = new List<PersistentItemEffect>();

    [PropertySpace(8, 8)]
    [ListDrawerSettings(ShowFoldout = false, ShowPaging = false)]
    [Help("Слоты в которые можно положить этот предмет")]
    public CharacterLoadoutSlots[] validSlots = Array.Empty<CharacterLoadoutSlots>();

    public virtual ItemAssetGrouping Grouping => ItemAssetGrouping.Other;

    public abstract ItemTypes ItemType { get; }

    public virtual int MaxUsages => this.Def.MaxUsages;

    public IItemDef Def => DefCache.Get(this);

    [NonSerialized, ShowInInspector, ReadOnly, DisplayAsString, PropertyOrder(-1)] string cachedName;

    public string ItemKey   => this.cachedName ??= base.name;
    public string Icon      => !string.IsNullOrEmpty(icon) ? icon : ItemKey;
    public string IconLarge => !string.IsNullOrEmpty(iconLarge) ? iconLarge : Icon;

    // ReSharper disable once InconsistentNaming
    [Obsolete("Use ItemKey or Icon instead", true)]
    public new string name => base.name;

    public virtual bool CanBeAssignedToSlot(Frame f, EntityRef targetEntity, EntityRef itemEntity,
      CharacterLoadoutSlots slot, WeaponAttachmentSlots weaponSlot, out ItemAssetAssignFailReason reason) {
      // disallow assign to weapon slots by default
      if (weaponSlot != WeaponAttachmentSlots.Invalid) {
        reason = ItemAssetAssignFailReason.SlotNotValidForItem;
        return false;
      }

      if (Array.IndexOf(this.validSlots, slot) == -1) {
        reason = ItemAssetAssignFailReason.SlotNotValidForItem;
        return false;
      }

      reason = ItemAssetAssignFailReason.None;
      return true;
    }

    public virtual bool CanBeUnAssignedFromSlot(Frame f, EntityRef itemEntity,
      CharacterLoadoutSlots slot, WeaponAttachmentSlots weaponSlot, out ItemAssetUnassignFailReason reason) {

      reason = ItemAssetUnassignFailReason.None;
      return true;
    }

    public void ChangeItemOwner(Frame f, EntityRef itemEntity, EntityRef newOwner) {
      var currentOwnerUnit = f.TryGet(itemEntity, out Item itemOwnerUnit)
        ? itemOwnerUnit.Owner
        : EntityRef.None;

      this.ChangeItemOwner(f, itemEntity, currentOwnerUnit, newOwner);
    }

    protected virtual void ChangeItemOwner(Frame f, EntityRef itemEntity, EntityRef currentOwnerUnit, EntityRef newOwner) {
      if (currentOwnerUnit != EntityRef.None) {
        f.Unsafe.GetPointer<Item>(itemEntity)->Owner = EntityRef.None;
        f.Remove<ItemOwnerIsUnit>(itemEntity);
        f.Remove<ItemOwnerIsWeapon>(itemEntity);
        f.Remove<ItemOwnerIsItemBox>(itemEntity);
      }

      if (newOwner != EntityRef.None) {
        f.Unsafe.GetPointer<Item>(itemEntity)->Owner = newOwner;

        if (f.Has<ItemBox>(newOwner)) {
          f.Add<ItemOwnerIsItemBox>(itemEntity);
        }
        else if (f.Has<Unit>(newOwner)) {
          f.Add<ItemOwnerIsUnit>(itemEntity);
        }
        else if (f.Has<Weapon>(newOwner)) {
          f.Add<ItemOwnerIsWeapon>(itemEntity);
        }
      }
    }

    public virtual EntityRef CreateItemEntity(Frame f, ItemAssetCreationData creationData) {
      var entity = f.Create();

      f.Add(entity, new Item {
        Asset                 = this.Guid,
        MetaGuid              = creationData.MetaGuid,
        IndexI                = creationData.IndexI,
        IndexJ                = creationData.IndexJ,
        Used                  = creationData.Used,
        SafeGuid              = creationData.SafeGuid,
        AddToLoadoutAfterFail = creationData.AddToLoadoutAfterFail,
      });

      f.GetOrAddPointer<Attributes>(entity);

      foreach (var effect in persistentEffects) {
        AttributesHelper.ChangeAttribute(f, entity, effect.attributeType, EModifierAppliance.OneTime, effect.operation, effect.value,
          duration: FP._0);
      }

      if (rarityEffects.IsValid) {
        var rarityEffectsConfig = f.FindAsset(rarityEffects);
        foreach (var effects in rarityEffectsConfig.rarityEffects) {
          if (effects.rarity == rarity) {
            foreach (var effect in effects.itemEffects) {
              AttributesHelper.ChangeAttribute(f, entity, effect.attributeType,
                EModifierAppliance.OneTime, effect.operation, effect.value,
                duration: FP._0);
            }
          }
        }
      }

      return entity;
    }

    public virtual void DestroyItemEntity(Frame f, EntityRef itemEntity) {
      f.Destroy(itemEntity);
    }

    [Serializable]
    public struct PersistentItemEffect {
      public EAttributeType attributeType;

      [TableColumnWidth(100, false)]
      public FP value;

      [TableColumnWidth(100, false)]
      public EModifierOperation operation;

      public static IEnumerable<PersistentItemEffect> Merge(
        IEnumerable<PersistentItemEffect> a,
        IEnumerable<PersistentItemEffect> b) {
        return a.Concat(b).GroupBy(it => it.attributeType).Select(it => new PersistentItemEffect {
          attributeType = it.Key,
          operation     = EModifierOperation.Add,
          value = it.Aggregate(FP._0, (sum, v) => v.operation switch {
            EModifierOperation.Add => sum + v.value,
            EModifierOperation.Subtract => sum - v.value,
            _ => sum,
          }),
        });
      }
    }
  }

  public enum ItemAssetGrouping {
    Other = 0,

    Weapons           = 100,
    WeaponAttachments = 50,
    Health            = 40,
    Ammo              = 39,
    Equipment         = 30,
    Abilities         = 20,
    Perks             = 15,
    Skins             = 10,
  }

  public enum ItemAssetAssignFailReason {
    None                = 0,
    SlotNotValidForItem = 1,
  }

  public enum ItemAssetUnassignFailReason {
    None                    = 0,
    SlotNotValidForItem     = 1,
    InventoryWeightOverflow = 2,
  }

  public class ItemAssetCreationData {
    public QGuid                   MetaGuid;
    public AssetRef<ItemAsset>     Asset;
    public ItemAssetCreationData[] WeaponAttachments;
    public byte                    IndexI;
    public byte                    IndexJ;
    public bool                    Rotated;
    public bool                    AddToLoadoutAfterFail;
    public ushort                  Used;
    public string                  SafeGuid;

    public ItemAssetCreationData(QGuid metaGuid, AssetRef<ItemAsset> asset) {
      MetaGuid = metaGuid;
      Asset    = asset;
    }

    public static ItemAssetCreationData FromGameSnapshotLoadoutItem(Frame f, [CanBeNull] GameSnapshotLoadoutItem runtime) {
      if (runtime == null || string.IsNullOrEmpty(runtime.ItemKey)) {
        return new ItemAssetCreationData(string.Empty, default);
      }

      var itemAsset = FindAssetByItemKey(f, runtime.ItemKey);

      if (itemAsset == null) {
        Log.Error("Failed to create item: asset for key not exist");
        return new ItemAssetCreationData(runtime.ItemGuid, default);
      }

      if (string.IsNullOrEmpty(runtime.ItemGuid)) {
        Log.Error("Failed to create item: ItemGuid must be non empty");
        return new ItemAssetCreationData(runtime.ItemGuid, default);
      }

      return new ItemAssetCreationData(runtime.ItemGuid, itemAsset) {
        WeaponAttachments = runtime.WeaponAttachments != null
          ? Array.ConvertAll(runtime.WeaponAttachments, a => FromGameSnapshotWeaponAttachment(f, a))
          : null,
        IndexI                = runtime.IndexI,
        IndexJ                = runtime.IndexJ,
        Rotated               = runtime.Rotated,
        Used                  = runtime.Used,
        SafeGuid              = runtime.SafeGuid,
        AddToLoadoutAfterFail = itemAsset.addToLoadoutAfterFail,
      };
    }

    public static ItemAssetCreationData FromGameSnapshotWeaponAttachment(Frame f, [CanBeNull] GameSnapshotLoadoutWeaponAttachment runtime) {
      if (runtime == null || string.IsNullOrEmpty(runtime.ItemKey)) {
        return new ItemAssetCreationData(string.Empty, default);
      }

      var itemAsset = FindAssetByItemKey(f, runtime.ItemKey);

      if (itemAsset == null) {
        Log.Error($"Failed to create weapon attachment: asset for key '{runtime.ItemKey}' not exist");
        return new ItemAssetCreationData(runtime.ItemGuid, default);
      }

      if (string.IsNullOrEmpty(runtime.ItemGuid)) {
        Log.Error($"Failed to create weapon attachment with itemKey '{runtime.ItemKey}': ItemGuid must be non empty");
        return new ItemAssetCreationData(runtime.ItemGuid, default);
      }

      return new ItemAssetCreationData(runtime.ItemGuid, itemAsset) {
        IndexI                = runtime.IndexI,
        IndexJ                = runtime.IndexJ,
        Rotated               = runtime.Rotated,
        Used                  = runtime.Used,
        AddToLoadoutAfterFail = itemAsset.addToLoadoutAfterFail,
      };
    }

    public static ItemAsset FindAssetByItemKey(Frame f, string itemKey) {
      if (string.IsNullOrEmpty(itemKey)) {
        return null;
      }

      return f.FindAsset<ItemAsset>(GetItemAssetPath(itemKey));
    }

    public static string GetItemAssetPath(string itemKey) {
      return "QuantumUser/Resources/Configs/Items/" + itemKey;
    }
  }
}