namespace Quantum {
  using System;
  [Serializable]
  public class RollAbilityItem : AbilityItemAsset {
    public override unsafe bool TryActivateAbility(Frame f, EntityRef entityRef, Ability* ability) {
      if (!CharacterFsm.CanEnterState<CharacterStateRoll>(f, entityRef)) {
        Log.Info("Roll cannot be activated in current character state");
        return false;
      }

      if (!base.TryActivateAbility(f, entityRef, ability)) {
        return false;
      }

      return CharacterFsm.TryEnterState(f, entityRef, new CharacterStateRoll {
        StateTimer = FrameTimer.FromSeconds(f, durationSec),
      });
    }

    public override unsafe Ability.AbilityState UpdateAbility(Frame f, EntityRef ownerRef, Ability* ability) {
      var state = base.UpdateAbility(f, ownerRef, ability);
        
      // if (state.IsDelayed) {
      //   f.LogTrace(ownerRef, "IsDelayed");
      //   
      //   var kcc  = f.GetPointer<KCC>(ownerRef);
      //   var unit = f.GetPointer<Unit>(ownerRef);
      //   kcc->SetLookRotation(unit->ActiveAbilityInfo.CastRotation);
      // }
      //
      // var animationTriggers = f.GetPointer<AnimationTriggers>(ownerRef);
      //
      // if (state.IsActiveStartTick) {
      //   animationTriggers->Roll = true;
      // }
      //
      // if (state.IsActiveEndTick) {
      //   animationTriggers->Roll = false;
      // }

      return state;
    }
  }
}