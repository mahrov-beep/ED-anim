namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine.Scripting;

  [Preserve]
  public unsafe class UnitMovementSystem : SystemMainThreadFilter<UnitMovementSystem.Filter> {
    public struct Filter {
      public EntityRef Entity;

      public Unit*           Unit;
      public KCC*            KCC;
      public InputContainer* InputContainer;
    }

    public override ComponentSet Without { get; } = ComponentSet.Create<CharacterStateDead, UnitExited>();

    public override void Update(Frame f, ref Filter filter) {
      var e     = filter.Entity;
      var kcc   = filter.KCC;
      var unit  = filter.Unit;
      var input = filter.InputContainer;

      if (EAttributeType.Set_LockMovement.IsValueSet(f, e)) {
        return;
      }

      var wishVelocity  = input->Input.MovementDirection.XOY * FPMath.Clamp01(input->Input.MovementMagnitude);
      var deltaVelocity = wishVelocity - input->InputAccelerated;

      var acceleration = FPVector3.Dot(wishVelocity, input->InputAccelerated) > FP._0 ? 3 : 6;

      input->InputAccelerated += FPVector3.ClampMagnitude(deltaVelocity, acceleration * f.DeltaTime);

      var speed = unit->CurrentStats.moveSpeed.AsFP;
      if (f.TryGetPointer(e, out Bot* bot)) {
        if (bot->StatsMultipliers != default) {
          var m = f.FindAsset(bot->StatsMultipliers);
          speed *= m.MoveSpeed;
        }
      }

      if (CharacterFsm.CurrentStateIs<CharacterStateSprint>(f, e, out var sprint)) {
        speed *= sprint->SprintSpeedMultiplier;
      }

      if (f.TryGetPointer(e, out CharacterStateKnocked* knocked)) {
        var crawlMultiplier = FPMath.Clamp01(knocked->CrawlSpeedMultiplier);
        speed *= crawlMultiplier;
      }

      if (f.TryGetPointer(e, out CharacterStateCrouchMove* crouch)) {
        var crouchMultiplier = crouch->CrouchSpeedMultiplier;    
        speed *= FPMath.Clamp01(crouchMultiplier);
      }
      
      kcc->SetInputDirection(kcc->Data.TransformRotation * input->InputAccelerated);
      kcc->SetKinematicSpeed(speed);

      unit->CurrentSpeed = input->InputAccelerated.Magnitude * speed;
    }
  }
}