namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine.Scripting;

  [Preserve]
  public unsafe class ReadPlayerInputSystem : SystemMainThread, ISignalOnGameEnd {
    public override void Update(Frame f) {
      if (f.Global->GameState != EGameStates.Game) {
        return;
      }

      var filter = f.Filter<InputContainer, Unit>(
              without: ComponentSet.Create<Bot, CharacterStateDead, UnitExited, Turret>());

      while (filter.NextUnsafe(out EntityRef e, out var inputContainer, out var unit)) {
        if (unit->PlayerRef == PlayerRef.None) {
          continue;
        }

        Input* input = f.GetPlayerInput(unit->PlayerRef);
        inputContainer->Input = *input;
      }
    }

    public void OnGameEnd(Frame f) {
      var filter = f.Filter<InputContainer>();

      while (filter.NextUnsafe(out var e, out var container)) {
        container->Input = default;

        EAttributeType.Set_LockMovement.ChangeAttribute(f, e,
                EModifierAppliance.Temporary,
                EModifierOperation.Add,
                FP._1,
                FP._10);

        EAttributeType.Set_LockRotation.ChangeAttribute(f, e,
                EModifierAppliance.Temporary,
                EModifierOperation.Add,
                FP._1,
                FP._10);
      }
    }
  }

}