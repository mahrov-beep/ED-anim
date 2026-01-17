namespace Quantum {
  using Photon.Deterministic;

  public unsafe class UnitFeatureSprintWithStaminaSystem : SystemMainThreadFilter<UnitFeatureSprintWithStaminaSystem.Filter> {
    public struct Filter {
      public EntityRef Entity;

      public Unit*                         Unit;
      public UnitFeatureSprintWithStamina* Stamina;
      public InputContainer*               Input;
    }

    public override void Update(Frame f, ref Filter filter) {
      var unit    = filter.Unit;
      var stamina = filter.Stamina;
      var input   = filter.Input->Input;

      var settings = f.FindAsset(unit->Asset).sprintSettings;

      if (input.SprintButton.IsDown) {
        CharacterFsm.TryEnterState(f, filter.Entity, new CharacterStateSprint {
          SprintSpeedMultiplier = settings.sprintSpeedMult,
        });
      }

      if (CharacterFsm.CurrentStateIs<CharacterStateSprint>(f, filter.Entity)) {
        stamina->current    = FPMath.Max(FP._0, stamina->current - settings.drainRate * f.DeltaTime);
        stamina->regenTimer = settings.regenDelay;
      }
      else {
        var regenTimeoutEnded = stamina->regenTimer.ProcessTimer(f);
        if (regenTimeoutEnded) {
          stamina->current = FPMath.Min(settings.maxStamina, stamina->current + settings.regenRate * f.DeltaTime);
        }
      }
    }
  }
}