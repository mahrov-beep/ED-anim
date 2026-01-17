namespace Quantum {
  using Photon.Deterministic;

  public unsafe class BotMovementInputSystem : SystemMainThreadFilter<BotMovementInputSystem.Filter> {
    public struct Filter {
      public EntityRef EntityRef;
      public Bot* Bot;
      public InputContainer* InputContainer;
      public CharacterSpectatorCamera* SpectatorCamera;
    }

    /*public struct Local {
      public EntityRef EntityRef;
      public InputContainer* InputContainer;
    }*/

    public override void Update(Frame f, ref Filter filter) {
      if (CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, filter.EntityRef)) {
        return;
      }
      
      /*f.FilterStruct(out Local local, without: ComponentSet.Create<Bot>()).Next(&local);
      var isAim = local.InputContainer->Input.AimButton;*/
      
      var input = filter.InputContainer;
      var camera = filter.SpectatorCamera;
      var bot = filter.Bot;

      bool isInCombat = bot->Intent.AttackTarget != EntityRef.None;
      bool isMoving = input->DesiredDirection != FPVector2.Zero;

      bool shouldAim = isInCombat && !isMoving;
      input->Input.AimButton = input->Input.AimButton.Update(f.Number, shouldAim);

      bool shouldSprint = isMoving && !isInCombat;
      if (shouldSprint && bot->PatrolState != default) {
        var patrolConfig = f.FindAsset(bot->PatrolState);
        shouldSprint = patrolConfig.PressSprintButton;
      }
      input->Input.SprintButton = input->Input.SprintButton.Update(f.Number, shouldSprint);

      if (!isMoving) {
        input->Input.MovementDirection = FPVector2.Zero;
        input->Input.MovementMagnitude = FP._0;
        return;
      }

      var rotatedDir = FPVector2.Rotate(
        input->DesiredDirection,
        camera->SpectatorCameraCurrentRotation.Yaw);

      input->Input.MovementDirection = rotatedDir;

      if (isInCombat && bot->CombatState != default) {
        var combatConfig = f.FindAsset(bot->CombatState);
        input->Input.MovementMagnitude = combatConfig.RepositionMagnitude;
      }
      else {
        input->Input.MovementMagnitude = FP._1;
      }
    }
  }
}
