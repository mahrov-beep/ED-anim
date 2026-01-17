namespace Quantum {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Photon.Deterministic;
  using Sirenix.OdinInspector;
  using UnityEngine;
  using UnityEngine.Pool;
  using UnityEngine.Serialization;
  using Random = System.Random;

  [Serializable]
  public unsafe class LoadoutConfig : AssetObject {
    [SerializeField]
    [TabGroup("Loadout", "Slots")]
    [ListDrawerSettings(ShowFoldout = false, ShowPaging = false)]
    // [TableList(AlwaysExpanded = true, ShowPaging = false)]
    SlotConfig[] slots = Array.Empty<SlotConfig>();
    
    [SerializeField]
    [TabGroup("Loadout", "Trash")]
    [ListDrawerSettings(ShowFoldout = false, ShowPaging = false)]
    // [TableList(AlwaysExpanded = true, ShowPaging = false)]
    TrashConfig[] trashItems = Array.Empty<TrashConfig>();

    public GameSnapshotLoadout BuildLoadoutWithoutGuids(Frame f) {
      return new GameSnapshotLoadout {
        SlotItems = MakeArray<GameSnapshotLoadoutItem>(CharacterLoadoutSlotsExtension.CHARACTER_LOADOUT_SLOTS, it => {
          foreach (var slotConfig in slots) {
            if (!slotConfig.hasProbability) {
              it[slotConfig.slot.ToInt()] = slotConfig.item.BuildItem(f);
            }
            else {
              var item = f.RNG->GetRandomElementWithIntervals(slotConfig.itemsWithProbability, static el => el.probabilityFP).item;
              it[slotConfig.slot.ToInt()] = item.BuildItem(f);
            }
          }
        }),
        
        TrashItems = Array.ConvertAll(
          trashItems.SelectMany(it => {
            if (!it.hasProbability) {
              return new ItemConfig[] {
                it.item,
              };
            }

            return f.RNG->GetRandomElementWithIntervals(it.trash, static el => el.probabilityFP).items;
          }).ToArray(), it => it.BuildItem(f)),
      };
    }

    static T[] MakeArray<T>(int length, Action<T[]> builder) {
      var array = new T[length];
      builder(array);
      return array;
    }

    [Serializable]
    struct SlotConfig {
      [HideLabel]
      [HorizontalGroup("head")]
      public CharacterLoadoutSlots slot;

      [ToggleLeft]
      [HorizontalGroup("head", Width = 130)]
      public bool hasProbability;

      [HideIf(nameof(hasProbability))]
      [InlineProperty, HideLabel]
      public ItemConfig item;

      [ShowIf(nameof(hasProbability))]
      [TableList(ShowPaging = false, AlwaysExpanded = true, DrawScrollView = false)]
      public ItemWithProbability[] itemsWithProbability;
    }

    [Serializable]
    struct ItemWithProbability {
      [TableColumnWidth(90, false)]
      public FP probabilityFP;
      
      public ItemConfig item;
    }
    
    [Serializable]
    struct TrashConfig {
      [ToggleLeft]
      public bool hasProbability;
      
      [HideIf(nameof(hasProbability))]
      [InlineProperty, HideLabel]
      public ItemConfig item;

      [ShowIf(nameof(hasProbability))]
      [TableList(ShowPaging = false, AlwaysExpanded = true, DrawScrollView = false)]
      public TrashWithProbability[] trash;
    }
    
    [Serializable]
    struct TrashWithProbability {
      [TableColumnWidth(90, false)]
      public FP probabilityFP;
      
      [ListDrawerSettings(ShowFoldout = false, ShowPaging = false)]
      public ItemConfig[] items;
    }

    [Serializable]
    public struct ItemConfig {
      [HideLabel, Required]
      [ValueDropdown(ItemAssetDropdown.ODIN_METHOD, AppendNextDrawer = true)]
      public AssetRef<ItemAsset> asset;

      [TableList(AlwaysExpanded = true, ShowPaging = false)]
      [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)]
      [ShowIf("@Quantum.QuantumUnityDB.GetGlobalAssetEditorInstance<ItemAsset>(this.asset) is Quantum.WeaponItemAsset")]
      public WeaponAttachmentSlotConfig[] attachments;

      public readonly GameSnapshotLoadoutItem BuildItem(Frame f) {
        var assetObj       = f.FindAsset(asset);
        var attachmentsRef = attachments;

        return new GameSnapshotLoadoutItem {
          ItemKey  = assetObj.ItemKey,
          ItemGuid = null,

          IndexI  = 0,
          IndexJ  = 0,
          Rotated = false,
          Used    = 0,
          SafeGuid = null,
          AddToLoadoutAfterFail = assetObj.addToLoadoutAfterFail,

          WeaponAttachments = MakeArray<GameSnapshotLoadoutWeaponAttachment>(WeaponAttachmentSlotsExtension.WEAPON_ATTACHMENT_SLOTS, it => {
            if (assetObj is not WeaponItemAsset) {
              return;
            }

            foreach (var attachmentConfig in attachmentsRef) {
              it[attachmentConfig.slot.ToInt()] = attachmentConfig.attachment.BuildAttachment(f);
            }
          }),
        };
      }
    }

    [Serializable]
    public struct WeaponAttachmentSlotConfig {
      [TableColumnWidth(150, false)]
      public WeaponAttachmentSlots slot;

      public WeaponAttachmentConfig attachment;
    }

    [Serializable]
    public struct WeaponAttachmentConfig {
      [HideLabel, Required]
      [ValueDropdown(ItemAssetDropdown.ODIN_METHOD, AppendNextDrawer = true)]
      public AssetRef<WeaponAttachmentItemAsset> asset;

      public readonly GameSnapshotLoadoutWeaponAttachment BuildAttachment(Frame f) {
        return new GameSnapshotLoadoutWeaponAttachment {
          ItemKey  = f.FindAsset(asset).ItemKey,
          ItemGuid = null,
          IndexI = 0,
          IndexJ = 0,
          Rotated = false,
          Used = 0,
        };
      }
    }
  }
}