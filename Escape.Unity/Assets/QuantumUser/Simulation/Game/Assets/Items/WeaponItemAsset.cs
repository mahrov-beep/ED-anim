namespace Quantum {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Photon.Deterministic;
  using Prototypes;
  using Sirenix.OdinInspector;
  using Unity.Cinemachine;
  using UnityEngine;
  using UnityEngine.Serialization;

  [Serializable]
  public unsafe class WeaponItemAsset : ItemAsset, ISelfValidator {
    [ShowInInspector]
    [PreviewField(100, ObjectFieldAlignment.Left), HideLabel, PropertyOrder(-900)]
    GameObject InspectorPrefabForPreview => this.visualPrefab;
    
    [TabGroup("General")] // Свойство с TabGroup=General должно быть первым чтобы эта вкладка была основной, не добавлять ничего выше
    [Help("Префаб оружия")]
    public GameObject visualPrefab;

    [TabGroup("Attachments")]
    [ListDrawerSettings(ShowFoldout = false, ShowPaging = false)]
    [RequiredRef, ValueDropdown(ItemAssetDropdown.ODIN_METHOD_ATTACHMENTS_ONLY)]
    [Help("Определяет какие обвесы можно добавлять на оружие. В отличие от схемы, этот список уникален для каждого оружия")]
    public AssetRef<WeaponAttachmentItemAsset>[] additionalAllowedAttachments;

    [TabGroup("Attachments")]
    [ListDrawerSettings(ShowFoldout = false, ShowPaging = false)]
    [RequiredRef, ValueDropdown(ItemAssetDropdown.ODIN_METHOD_ATTACHMENTS_ONLY)]
    [Help("Определяет какие обвесы НЕЛЬЗЯ добавлять на оружие (нельзя даже если оно указано в schema)")]
    public AssetRef<WeaponAttachmentItemAsset>[] disallowedAttachments;

    [PropertySpace]
    [TabGroup("Attachments")]
    [Required, ValueDropdown(WeaponItemAttachmentsSchema.ALL_SCHEMAS_ODIN_PATH)]
    [Help("Определяет какие обвесы можно добавлять на оружие. Одна схема может быть использована для нескольких оружий")]
    [InlinePingButton]
    public WeaponItemAttachmentsSchema attachmentsSchema;

    [PropertySpace]
    [TabGroup("Attachments")]
    [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
    [ValueDropdown(ItemAssetDropdown.ODIN_METHOD_ATTACHMENTS_ONLY)]
    public List<AssetRef<WeaponAttachmentItemAsset>> AllValidAttachments =>
      (additionalAllowedAttachments ?? Array.Empty<AssetRef<WeaponAttachmentItemAsset>>())
      .Concat(attachmentsSchema ? attachmentsSchema.attachments : Array.Empty<AssetRef<WeaponAttachmentItemAsset>>())
      .Where(it => disallowedAttachments == null || disallowedAttachments.All(disallowed => disallowed != it))
      .ToList();

    public bool IsAttachmentAllowed(WeaponAttachmentItemAsset attachmentAsset) {
      // Чтобы не добавлять все вариации патрон в список, разрешаем использовать все патроны подходящего типа
      if (attachmentAsset is AmmoBoxItemAsset ammoBoxItemAsset && ammoBoxItemAsset.AmmoType == this.AmmoType) {
        return true;
      }

      var allowed = ContainsAssetRef(attachmentsSchema.attachments, attachmentAsset) ||
                    ContainsAssetRef(additionalAllowedAttachments, attachmentAsset);
      return allowed && !ContainsAssetRef(disallowedAttachments, attachmentAsset);
    }

    [TabGroup("Stats")]
    [Help("Конфиг описывающий нанесение урона")]
    public AssetRef<WeaponBasicAttackData> attackData;

    [TabGroup("Stats")]
    [Help("минимальный урон от выстрела")]
    public FP minBaseDamage;

    [TabGroup("Stats")]
    [Help("максимальный урон от выстрела")]
    public FP maxBaseDamage;

    [Obsolete]
    [FormerlySerializedAs("damage")]
    public FP baseDamage;
    
    [TabGroup("Stats")]
    // [RangeEx(1, 100)]
    [Help("Множитель урона при попадании в голову")]
    public FP headshotDamageMultiplier = FP._1;

    [Help("Коэффициент для выбора приоритетных целей вблизи Distance * Coeff")]
    [TabGroup("General")]
    // [RangeEx(0, 1)]
    public FP priorityAimDistanceCoefficient;

    // [RangeEx(2, 50)]
    [Space]
    [Help("Угол отклонения направления выстрела относительно направления точно в точку прицела")]
    [FormerlySerializedAs("spreadAngle")]
    [TabGroup("Stats")]
    public FP baseSpreadAngle;
    
    // [RangeEx(2, 50)]
    [Help("Множитель разброса от цели в режиме прицеливания, 0.5 это 50% от baseSpreadAngle")]
    [FormerlySerializedAs("spreadAngle")]
    [TabGroup("Stats")]
    public FP spreadCoefficientInAimState = FP._0_50;
    
    [Help("Доп. % разброса в зависимости от текущей скорости")]
    [TabGroup("Stats")]
    [Tooltip("Если равно 1, то при макс. скорости разброс будет равен spreadAngle * 2")]
    // [RangeEx(0, 2)]
    public FP spreadSpeedCoefficient;

    /*
    [Help("Доп. % разброса в зависимости от текущей скорости в состоянии аима")]
    [TabGroup("Stats")]
    [Tooltip("Если равно 1, то при макс. скорости разброс будет равен spreadAngle * 2")]
    // [RangeEx(0, 2)]
    public FP spreadSpeedCoefficientInAimState;
    */

    [Space]
    [FoldoutGroup("LEGACY")] // устарело и больше не должно использоваться
    // [RangeEx(-4, 7)]
    public FP cameraOffsetZ = FP._0;

    [Space]
    [TabGroup("Stats")]
    [Help("Число выстрелов в минуту")]
    [SuffixLabel("@" + nameof(InspectorFireInterval), SdfIconType.Info, true)]
    public int fireRate;

    string InspectorFireInterval => $"each {Math.Round(60f / fireRate, 2)} sec";

    [Space]
    [Help("Время перезарядки патрон")]
    [TabGroup("Stats")]
    // [RangeEx(0.4f, 3f)]
    [FormerlySerializedAs("reloadingTime")]
    public FP baseReloadingTime;

    [Space]
    [TabGroup("Stats")]
    [Help("Дистанция обнаружения цели")]
    // [RangeEx(0.001d, 30)]
    [FormerlySerializedAs("triggerDistance")]
    public FP baseTriggerDistance;
    
    [TabGroup("Stats")]
    [Help("УГол по горизонтальной относительно центра точки прицеливания в котором система авто стрельбы начнет стрельбу")]
    // [RangeEx(0, 45)]
    [FormerlySerializedAs("triggerAngle")]
    public FP baseTriggerAngle;
  
    [TabGroup("Stats")]
    [Help("УГол по вертикальной относительно центра точки прицеливания в котором система авто стрельбы начнет стрельбу")]
    // [RangeEx(0, 45)]
    public FP baseTriggerAngleY = 20;

    [Space][TabGroup("Stats")]
    [Help("Время в течение которого нужно держать цель в прицеле для начала стрельбы")]
    [FormerlySerializedAs("preShotAimingSeconds")]
    public FP basePreShotAimingSeconds = 0;

    [TabGroup("Stats")]
    [Help("Задержка между последним выстрелом и началом перезарядки")]
    public FP preReloadAmmoSeconds = FP._0_20;

    // [RangeEx(0, 1)]
    [FoldoutGroup("LEGACY")] // устарело
    public FP zoomMultOnAim = 1;

    // [RangeEx(1, 10)]
    [TabGroup("Stats")]
    [Help("Сколько пуль вылетает при одном выстреле. Значение больше 1 можно использовать для дробовиков")]
    public short bulletsPerShot;
    
    [TabGroup("Stats")]
    [FormerlySerializedAs("bulletsCount")]
    // [RangeEx(0, 200)] 
    [Help("Число патронов в магазине")]
    public short magazineAmmoCount;

    [TabGroup("General")]
    [Help("Отступ точки выстрела пули. Должна быть примерно на уровне дула оружия. " +
          "Настраивать можно во время игры, в сцене отображается риг персонажа в котором видна эта точка")]
    public FPVector3 shotOriginOffset = new FPVector3(FP._0_10, FP._1 + FP._0_33, FP._1 - FP._0_05);
    
    [TabGroup("General")]
    [Help("Точка которая означает 'центр' цели. Авто стрельба начнется если целиться в эту точку")]
    public FP shotTargetOffsetY = FP._1 + FP._0_33;

    [Help("Отдача при стрельбе (вращение камеры на случайный угол из диапазона)")]
    [FoldoutGroup("LEGACY")] // Старая система, больше не используется
    public FPVector2 recoilCameraAnglesMin = FPVector2.Zero;
    
    [FoldoutGroup("LEGACY")] // Старая система, больше не используется
    public FPVector2 recoilCameraAnglesMax = FPVector2.Zero;
    
    [Help("Множитель отдачи в аиме, 0.5 == 50% от recoilCameraAnglesMin и recoilCameraAnglesMax")]
    [FoldoutGroup("LEGACY")] // Старая система, больше не используется
    public FP recoilCoefficientInAimState = FP._0_50;

    [Space]
    [TabGroup("Stats")]
    [Help("Время чтобы спрятать оружие. Должно примерно совпадать с длительностью анимации смены оружия")]
    public FP timeToHideWeapon;

    [TabGroup("Stats")]
    [Help("Время чтобы достать оружие. Должно примерно совпадать с длительностью анимации смены оружия")]
    public FP timeToGetWeapon;

    [TabGroup("General")]
    [Help("Доп фильтры которые используются системой квестов чтобы уточнить детали убийства. " +
          "Например, если добавим Killed_ByPistol то система будет знать что убийство было из пистолета " +
          "и на эту механику можно будет сделать квест")]
    [ListDrawerSettings(ShowFoldout = false, ShowPaging = false)]
    public QuestTaskFilters[] questEnemyKilledFilters = Array.Empty<QuestTaskFilters>();

    [TabGroup("General")]
    [Help("Импульс Cinemachine камеры на камере того, кто совершил выстрел. Старый способ создания отдачи, можно оставить пустым")]
    public AssetRef<CinemachineImpulseAsset> shotImpulse;

    [TabGroup("Critical")]
    [FormerlySerializedAs("baseCritChancePersent")]
    [Help("Вероятность нанести критический урон (от 0 до 100)")]
    // [RangeEx(0, 100)]
    public FP baseCritChancePercent;

    [TabGroup("Critical")]
    [FormerlySerializedAs("baseCritMultiplierPersent")]
    [Help("На сколько умножить урон при критическом уроне. От 1 и больше")]
    // [RangeEx(1, 400)]
    public FP baseCritMultiplierPercent;

    [TabGroup("slowDebuff")]
    [Help("Дебаф более высокого приоритета перебивает более слабое замедление")]
    // [RangeEx(1, 3)]
    public int slowDebuffPriority;

    [TabGroup("slowDebuff")]
    // [RangeEx(1, 7)]
    public int slowStackLimit;

    [TabGroup("slowDebuff")]
    // [RangeEx(0.01, 0.30)]
    public FP slowMultiplierPerStack;

    [TabGroup("slowDebuff")]
    // [RangeEx(1, 10)]
    public FP slowDuration;

    [TabGroup("FireBuff")]
    [FormerlySerializedAs("fireChancePersent")]
    // [RangeEx(0, 100)]
    public int fireChancePercent;
    
    [TabGroup("FireBuff")]
    public FP fireDuration;
    
    [TabGroup("FireBuff")]
    public FP fireDamage;

    [TabGroup("Cue")]
    [Help("Как далеко высветится маркер")]
    public FP baseShotSoundRange = 46;

    [TabGroup("Minimap")]
    [Help("Дополнительное время видимости на карте после выстрела, суммируется с MinimapConfig")]
    // [RangeEx(0, 2)]
    public FP minimapShotVisiblyDuration;

    public override ItemTypes ItemType => ItemTypes.Weapon;

    public override ItemAssetGrouping Grouping => ItemAssetGrouping.Weapons;

    public AmmoTypes AmmoType => this.Def.AmmoType;

    public override void Reset() {
      base.Reset();

      this.validSlots = new[] {
              CharacterLoadoutSlots.MeleeWeapon,
              CharacterLoadoutSlots.PrimaryWeapon,
              CharacterLoadoutSlots.SecondaryWeapon,
      };
    }

    protected override void ChangeItemOwner(Frame f, EntityRef itemEntity, EntityRef currentOwnerUnit, EntityRef newOwner) {
      base.ChangeItemOwner(f, itemEntity, currentOwnerUnit, newOwner);

      var weapon = f.GetPointer<WeaponItem>(itemEntity);

      foreach (var weaponSlot in WeaponAttachmentSlotsExtension.AllValidSlots) {
        var attachmentEntity = weapon->AttachmentAtSlot(weaponSlot);
        if (attachmentEntity == EntityRef.None) {
          continue;
        }

        var attachment      = f.GetPointer<Item>(attachmentEntity);
        var attachmentAsset = f.FindAsset(attachment->Asset);
        attachmentAsset.ChangeItemOwner(f, attachmentEntity, newOwner);
      }
    }

    public override EntityRef CreateItemEntity(Frame f, ItemAssetCreationData creationData) {
      var weaponEntity = base.CreateItemEntity(f, creationData);

      SetupWeaponItem(f, weaponEntity, creationData);
      SetupWeapon(f, weaponEntity, creationData);

      return weaponEntity;
    }

    unsafe void SetupWeaponItem(Frame f, EntityRef weaponEntity, ItemAssetCreationData creationData) {
      WeaponItem* weaponItem = f.GetOrAddPointer<WeaponItem>(weaponEntity);
      // WeaponItem* weaponItem = f.GetPointer<WeaponItem>(weaponEntity);

      weaponItem->SelfWeaponEntity = weaponEntity;

      if (creationData.WeaponAttachments is { } attachments) {
        foreach (var slotType in WeaponAttachmentSlotsExtension.AllValidSlots) {
          var slotIndex = (int)slotType;
          if (slotIndex < attachments.Length && attachments[slotIndex] != null) {
            var item = CreateWeaponAttach(attachments[slotIndex]);
            if (item != EntityRef.None) {
              weaponItem->AssignAttachmentToSlot(f, slotType, item);
            }
          }
        }
      }

      return;

      EntityRef CreateWeaponAttach(ItemAssetCreationData data) {
        return data.Asset.IsValid ? f.FindAsset(data.Asset).CreateItemEntity(f, data) : EntityRef.None;
      }
    }

    unsafe void SetupWeapon(Frame f, EntityRef weaponEntity, ItemAssetCreationData creationData) {
      var weapon = f.GetOrAddPointer<Weapon>(weaponEntity);
      // var weapon = f.GetPointer<Weapon>(weaponEntity);

      weapon->Config = this;
      
      // Создаем оружие без патрон
      // патроны нужно отдельно докладывать в слот Ammo и перезарядить оружие
      weapon->BulletsCount = 0;
    }

    public FPVector3 CalculateShotOrigin(Frame f, EntityRef unitEntity) {
      var aiming = f.GameModeAiming;
      UnitHelper.CalculateRig(f, unitEntity,
              aiming.characterTorsoOffset, aiming.characterShoulderOffset, this.shotOriginOffset,
              out _, out _, out var shotOrigin);
      return shotOrigin;
    }

    public WeaponStats GetBaseStats() {
      return new WeaponStats {
        minDamage   = this.minBaseDamage,
        maxDamage   = this.maxBaseDamage,
        spreadAngle = this.baseSpreadAngle,

        reloadingTime  = this.baseReloadingTime,
        attackDistance = this.baseTriggerDistance,
        triggerAngleX  = this.baseTriggerAngle,
        triggerAngleY  = this.baseTriggerAngleY,
        maxAmmo        = (FP)this.magazineAmmoCount,

        critChance = this.baseCritChancePercent,
        critDamage = this.baseCritMultiplierPercent,

        preShotAimingSeconds = this.basePreShotAimingSeconds,

        weaponShotSoundRange = this.baseShotSoundRange,
      };
    }

    public void Validate(SelfValidationResult result) {
      if (baseDamage > FP._0 && minBaseDamage == FP._0 && maxBaseDamage == FP._0) {
        result.AddWarning($"'{this.ItemKey}' использует устаревшее baseDamage")
          .WithFix("Конвертировать в minDamage/maxDamage", () => {
            maxBaseDamage = baseDamage;
            minBaseDamage = FPMath.Max(baseDamage - 5, FP._1);
            baseDamage = FP._0;
          });
      }
    }
    
    static bool ContainsAssetRef<T>(AssetRef<T>[] array, AssetRef<T> element) where T : AssetObject {
      if (array == null) {
        return false;
      }

      foreach (var it in array) {
        if (it == element) {
          return true;
        }
      }

      return false;
    }
  }
}