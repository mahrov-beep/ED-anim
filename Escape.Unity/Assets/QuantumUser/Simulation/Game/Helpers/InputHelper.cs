namespace Quantum {
  using Photon.Deterministic;

  public static unsafe class InputHelper {
    public static void ResetMovementInput(Frame f, EntityRef entity) {
      if (!f.TryGetPointer(entity, out InputContainer* input)) {
        return;
      }

      input->Input.MovementMagnitude = FP._0;
      input->Input.MovementDirection = FPVector2.Zero;
      input->InputAccelerated        = FPVector3.Zero;
      input->DesiredDirection        = FPVector2.Zero;
    }

    public static void ResetMovementAndSprintInput(Frame f, EntityRef entity) {
      if (!f.TryGetPointer(entity, out InputContainer* input)) {
        return;
      }

      input->Input.MovementMagnitude = FP._0;
      input->Input.MovementDirection = FPVector2.Zero;
      input->InputAccelerated        = FPVector3.Zero;
      input->DesiredDirection        = FPVector2.Zero;
      input->Input.SprintButton      = default;
    }
  }
}

