namespace Quantum {
  public unsafe partial struct Unit {
    public bool HasAnyAbility(Frame f) {
      return f.Exists(AbilityRef) && f.Has<Ability>(AbilityRef);
    }    

    public bool HasAnyAbility(FrameThreadSafe f) {
      return f.Exists(AbilityRef) && f.Has<Ability>(AbilityRef);
    }

    public Ability* GetAbility(Frame f) {
      var ability = f.GetPointer<Ability>(AbilityRef);
      return ability;
    }

    public Ability* GetAbility(FrameThreadSafe f) {
      var ability = f.GetPointer<Ability>(AbilityRef);
      return ability;
    }

    public bool HasDelayedOrActiveAbility(Frame f) {
      if (!f.Exists(AbilityRef)) {
        return false;
      }

      var ability = f.GetPointer<Ability>(AbilityRef);
      return ability->IsDelayedOrActive;
    }

    public bool HasDelayedAbility(Frame f) {
      if (!f.Exists(AbilityRef)) {
        return false;
      }

      var ability = f.GetPointer<Ability>(AbilityRef);
      return ability->IsDelayed;
    }

    public bool HasDelayedOrActiveAbility(Frame f, out Ability* ability) {
      if (!f.Exists(AbilityRef)) {
        ability = null;
        return false;
      }

      ability = f.GetPointer<Ability>(AbilityRef);

      return ability->IsDelayedOrActive;
    }

    public bool HasDelayedOrActiveAbility(Frame f, out Ability* ability, out AbilityItemAsset config) {
      if (!f.Exists(AbilityRef)) {
        ability = null;
        config  = null;
        return false;
      }

      ability = f.GetPointer<Ability>(AbilityRef);
      config  = f.FindAsset<AbilityItemAsset>(ability->Config);

      return ability->IsDelayedOrActive;
    }

  }
}