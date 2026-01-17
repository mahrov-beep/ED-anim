namespace Quantum {
  using JetBrains.Annotations;
  using Photon.Deterministic;

  public unsafe partial struct Unit {
    public bool IsWeaponChanging => HideWeaponTimer + GetWeaponTimer > 0;

    public FP WeaponChangingTimerLeft => HideWeaponTimer + GetWeaponTimer;

    public FP ConfigTimeToGetActiveWeapon(Frame f) => GetWeaponConfig(f, ActiveWeaponRef)?.timeToGetWeapon ?? FP._0;
    public FP ConfigTimeToHidePrevWeapon(Frame f)  => GetWeaponConfig(f, PrevWeaponRef)?.timeToHideWeapon ?? FP._0;
    public FP ConfigTimeToChangeWeapon(Frame f)    => this.ConfigTimeToGetActiveWeapon(f) + this.ConfigTimeToHidePrevWeapon(f);

    public bool NeedToStartWeaponHideSignal(Frame f) => HideWeaponTimer == ConfigTimeToHidePrevWeapon(f);
    public bool NeedToStartWeaponGetSignal(Frame f)  => GetWeaponTimer == ConfigTimeToGetActiveWeapon(f);

    public bool IsActiveWeaponReloading(Frame f) =>
      f.TryGetPointer(ActiveWeaponRef, out Weapon* weapon) && weapon->IsReloading;

    [CanBeNull] public WeaponItemAsset GetWeaponConfig(Frame f, EntityRef weaponRef) {
      if (!f.Exists(weaponRef)) {
        return null;
      }

      var weapon = f.GetPointer<Weapon>(weaponRef);
      var config = weapon->GetConfig(f);
      return config;
    }

    [CanBeNull] public WeaponItemAsset GetWeaponConfig(FrameThreadSafe f, EntityRef weaponRef) {
      if (!f.Exists(weaponRef)) {
        return null;
      }

      var weapon = f.GetPointer<Weapon>(weaponRef);
      var config = weapon->GetConfig(f);
      return config;
    }

    [CanBeNull] public WeaponItemAsset GetActiveWeaponConfig(FrameThreadSafe f) => GetWeaponConfig(f, ActiveWeaponRef);
    [CanBeNull] public WeaponItemAsset GetActiveWeaponConfig(Frame f)           => GetWeaponConfig(f, ActiveWeaponRef);
    [CanBeNull] public WeaponItemAsset GetPrevWeaponConfig(Frame f)             => GetWeaponConfig(f, PrevWeaponRef);
    [CanBeNull] public WeaponItemAsset GetPrimaryWeaponConfig(Frame f)          => GetWeaponConfig(f, PrimaryWeapon);
    [CanBeNull] public WeaponItemAsset GetSecondaryWeaponConfig(Frame f)        => GetWeaponConfig(f, SecondaryWeapon);
    [CanBeNull] public WeaponItemAsset GetMeleeWeaponConfig(Frame f)            => GetWeaponConfig(f, MeleeWeapon);

    public bool TryGetActiveWeapon(Frame f, out Weapon* weapon) => f.TryGetPointer(ActiveWeaponRef, out weapon);

    public EntityRef ValidWeaponRef =>
      PrimaryWeapon != EntityRef.None ? PrimaryWeapon
      : SecondaryWeapon != EntityRef.None ? SecondaryWeapon
      : MeleeWeapon;

    public void TryChangeWeapon(Frame f, EntityRef newWeaponRef, bool allowNull = false) {
      if (newWeaponRef == EntityRef.None && !allowNull) {
        return;
      }

      if (ActiveWeaponRef == newWeaponRef) {
        return;
      }

      if (ActiveWeaponRef != EntityRef.None && f.TryGetPointer<Weapon>(ActiveWeaponRef, out var activeWeapon) && activeWeapon->IsReloading) {
        activeWeapon->ResetReloadingTimer();
      }

      var isHidingWeapon = HideWeaponTimer > 0;
      if (!isHidingWeapon) {
        PrevWeaponRef   = ActiveWeaponRef;
        PrevWeaponSlot  = ActiveWeaponSlot;
        HideWeaponTimer = ConfigTimeToHidePrevWeapon(f);
      }

      ActiveWeaponRef = newWeaponRef;
      ActiveWeaponSlot = newWeaponRef == PrimaryWeapon ? WeaponSlot.Primary
        : newWeaponRef == SecondaryWeapon ? WeaponSlot.Secondary
        : newWeaponRef == MeleeWeapon ? WeaponSlot.Melee
        : WeaponSlot.None;
      GetWeaponTimer = ConfigTimeToGetActiveWeapon(f);
    }
  }
}