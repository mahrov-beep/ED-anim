using UnityEngine.Scripting;

namespace Quantum {
  using UnityEngine;
  [Preserve]
  public unsafe class AnimationSystem : SystemMainThreadFilter<AnimationSystem.Filter>,
          ISignalOnReloading,
          ISignalOnUnitHideWeapon,
          ISignalOnUnitGetWeapon {

    public struct Filter {
      public EntityRef Entity;

      public AnimationTriggers* AnimationTriggers;
    }

    public override void Update(Frame f, ref Filter filter) {
      filter.AnimationTriggers->HideWeapon     = filter.AnimationTriggers->HideWeapon != filter.AnimationTriggers->PrevHideWeapon;
      filter.AnimationTriggers->PrevHideWeapon = filter.AnimationTriggers->HideWeapon;

      filter.AnimationTriggers->GetWeapon     = filter.AnimationTriggers->GetWeapon != filter.AnimationTriggers->PrevGetWeapon;
      filter.AnimationTriggers->PrevGetWeapon = filter.AnimationTriggers->GetWeapon;

      filter.AnimationTriggers->Reloading     = filter.AnimationTriggers->Reloading != filter.AnimationTriggers->PrevReloading;
      filter.AnimationTriggers->PrevReloading = filter.AnimationTriggers->Reloading;
    }

    public void OnReloading(Frame f, EntityRef attackerRef, EntityRef weaponRef) {
      if (!f.TryGetPointer<AnimationTriggers>(attackerRef, out var triggers)) {
        return;
      }

      triggers->Reloading = true;
    }

    public void OnUnitHideWeapon(Frame f, EntityRef unitRef) {
      if (!f.TryGetPointer<AnimationTriggers>(unitRef, out var triggers)) {
        return;
      }

      triggers->HideWeapon = true;
    }

    public void OnUnitGetWeapon(Frame f, EntityRef unitRef) {
      if (!f.TryGetPointer<AnimationTriggers>(unitRef, out var triggers)) {
        return;
      }

      triggers->GetWeapon = true;
    }
  }
}