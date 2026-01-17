namespace Quantum.Commands {
  using Photon.Deterministic;
  using Quantum;

  public sealed class CrouchCommand : CharacterCommandBase {    

    public override void Serialize(BitStream stream) { }

    public override unsafe void Execute(Frame f, EntityRef characterEntity) {
      if (EAttributeType.Set_LockMovement.IsValueSet(f, characterEntity)) {
        return;
      }

      if (CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, characterEntity) ||
          CharacterFsm.CurrentStateIs<CharacterStateDead>(f, characterEntity) ||
          CharacterFsm.CurrentStateIs<CharacterStateReviving>(f, characterEntity) ||
          CharacterFsm.CurrentStateIs<CharacterStateHealing>(f, characterEntity) ||
          CharacterFsm.CurrentStateIs<CharacterStateKnifeAttack>(f, characterEntity)) {
        return;
      }

      if (!f.TryGetPointers(characterEntity, out Unit* unit, out InputContainer* input)) {
        return;
      }

      bool isCrouchState = CharacterFsm.CurrentStateIs<CharacterStateCrouchIdle>(f, characterEntity) ||
                           CharacterFsm.CurrentStateIs<CharacterStateCrouchMove>(f, characterEntity);

      if (isCrouchState) {
        if (CrouchHelper.CanStand(f, characterEntity)) {
          CharacterFsm.TryEnterState(f, characterEntity, new CharacterStateIdle());
        }       
      }
      else {        
          CharacterFsm.TryEnterState(f, characterEntity, new CharacterStateCrouchIdle());             
      }      
    }
  }
}
