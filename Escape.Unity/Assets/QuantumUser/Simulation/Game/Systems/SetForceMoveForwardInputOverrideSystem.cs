namespace Quantum {
  using Photon.Deterministic;
  public unsafe class SetForceMoveForwardInputOverrideSystem : SystemMainThreadFilter<SetForceMoveForwardInputOverrideSystem.Filter> {
    public struct Filter {
      public EntityRef       Entity;
      public InputContainer* InputContainer;
    }

    public override void Update(Frame f, ref Filter filter) {
      if (EAttributeType.Set_ForceMoveForward.IsValueSet(f, filter.Entity)) {

        Input* i = &filter.InputContainer->Input;

        i->MovementDirection = FPVector2.Up;
        i->MovementMagnitude = FP._1;
      }
    }
  }
}