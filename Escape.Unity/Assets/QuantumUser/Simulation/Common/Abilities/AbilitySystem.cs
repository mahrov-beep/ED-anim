namespace Quantum {
  public unsafe class AbilitySystem : SystemMainThreadFilter<AbilitySystem.Filter>,
          ISignalOnActiveAbilityStopped,
          ISignalOnCooldownsReset {

    public struct Filter {
      public EntityRef       EntityRef;
      public InputContainer* InputContainer;
      public Unit*           Unit;
    }

    public override void Update(Frame f, ref Filter filter) {
      var e = filter.EntityRef;

      var unit = filter.Unit;
      
      if (!f.Exists(unit->AbilityRef)) {
        return;
      }

      var ability     = unit->GetAbility(f);
      var abilityItem = f.FindAsset(ability->Config);

      abilityItem.UpdateAbility(f, e, ability);

      if (unit->IsWeaponChanging) {
        return;
      }   

      if (CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, e) ||
          CharacterFsm.CurrentStateIs<CharacterStateReviving>(f, e) ||
          CharacterFsm.CurrentStateIs<CharacterStateHealing>(f, e) ||
          CharacterFsm.CurrentStateIs<CharacterStateKnifeAttack>(f, e)) {
        return;
      }   
      
      if (unit->IsActiveWeaponReloading(f)) {
        return;
      }
      
      abilityItem.UpdateInput(f, ability, filter.InputContainer->ButtonAbilityWasReleased);
      abilityItem.TryActivateAbility(f, e, ability);
    }

    public void OnActiveAbilityStopped(Frame f, EntityRef playerEntityRef) { }

    public void OnCooldownsReset(Frame f, EntityRef playerEntityRef) { }
  }
}
