namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine;

  public abstract unsafe class AbilityItemAsset : ItemAsset {
    [Header("Сколько удерживать инпут перед началом активации")]
    public FP inputBuffer = FP._0_10 + FP._0_05;
    [Header("Задержка перед активной фазой")]
    public FP delay = FP._0_10 + FP._0_05;
    [Header("Длительность активной фазы")]
    public FP durationSec = FP._0_25;
    public FP cooldownSec = FP._5;

    public EAbilityCastDirectionType castDirectionType       = EAbilityCastDirectionType.FacingDirection;
    public bool                      startCooldownAfterDelay = false;

    public Effect[] onActivateEffects;
    public Effect[] onStopEffects;

    [Space]
    [Header("Unity")]
    public FP animationDuration;
    public string skillName;

    public override ItemTypes ItemType => ItemTypes.Ability;

    public override ItemAssetGrouping Grouping => ItemAssetGrouping.Abilities;

    public override void Reset() {
      base.Reset();

      this.validSlots = new[] { CharacterLoadoutSlots.Skill };
    }

    public override EntityRef CreateItemEntity(Frame f, ItemAssetCreationData creationData) {
      var abilityRef = base.CreateItemEntity(f, creationData);

      var ability = f.GetOrAddPointer<Ability>(abilityRef);
      ability->Config = this;
      ability->CooldownTimer.Start(FP._0);

      return abilityRef;
    }

    public virtual Ability.AbilityState UpdateAbility(Frame f, EntityRef ownerRef, Ability* ability) {
      var state = ability->Update(f, ownerRef);
      if (state.IsActiveStartTick) {
        onActivateEffects.ApplyAll(f, ownerRef);
      }
      else
      if (state.IsActiveEndTick) {
        onStopEffects.ApplyAll(f, ownerRef);
      }

      return state;
    }

    public virtual void UpdateInput(Frame f, Ability* ability, bool inputWasPressed) {
      if (inputWasPressed) {
        ability->BufferInput(f);
      }
    }

    public virtual bool TryActivateAbility(Frame f, EntityRef entityRef, Ability* ability) {
      if (ability->HasBufferedInput) {
        if (ability->TryActivateAbility(f, entityRef)) {
          return true;
        }
      }

      return false;
    }
  }

}