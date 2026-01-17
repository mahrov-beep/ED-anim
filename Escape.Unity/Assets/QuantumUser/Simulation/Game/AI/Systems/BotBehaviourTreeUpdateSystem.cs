namespace Quantum {
  using Photon.Deterministic;
  using static BotBehaviourTreeUpdateSystem;

  public unsafe class BotBehaviourTreeUpdateSystem : SystemMainThreadFilter<Filter>,
    ISignalOnComponentAdded<Bot> {
    public struct Filter {
      public EntityRef    EntityRef;
      public Transform3D* Transform;

      public Bot*            Bot;
      public InputContainer* InputContainer;
      public BTAgent*        Agent;
    }

    public override ComponentSet Without { get; } = ComponentSet.Create<CharacterStateDead>();

    public void OnAdded(Frame f, EntityRef e, Bot* bot) {
      var config   = f.FindAsset(f.GameMode.BotGlobalConfig);
      var interval = config.BTUpdateTickInterval;
      bot->UpdateFrameOffset = f.Global->BotSpawnCounter % interval;

      f.Global->BotSpawnCounter++;
    }

    public override void Update(Frame f, ref Filter filter) {
      if (f.IsPredicted) {
        return;
      }

      var botsConfig = f.FindAsset(f.GameMode.BotGlobalConfig);
      int interval   = botsConfig.BTUpdateTickInterval;

      bool isScheduledUpdate = f.Number % interval == filter.Bot->UpdateFrameOffset % interval;
      bool shouldUpdate      = isScheduledUpdate || filter.Bot->ForceBTUpdate;

      if (!shouldUpdate) {
        return;
      }

      filter.Bot->ForceBTUpdate = false;

      EntityRef       e     = filter.EntityRef;
      InputContainer* input = filter.InputContainer;

      if (CharacterFsm.CurrentStateIs<CharacterStateKnocked>(f, e)) {
        input->ResetAllInput();
        return;
      }

      if (CharacterFsm.CurrentStateIs<CharacterStateReviving>(f, e)) {
        input->ResetAllInput();
        return;
      }

      if (CharacterFsm.CurrentStateIs<CharacterStateHealing>(f, e)) {
        input->ResetAllInput();
        return;
      }

      BTParamsUser p = new BTParamsUser();
      AIContext    c = new AIContext();

      AIContextUser data = default;
      BotContextHelper.FillBotUserContext(f, e, ref data);
      c.UserData = &data;

      SystemMetrics.Begin("BTManager.Update");
      BTManager.Update(f, e, ref p, ref c, blackboard: null);
      SystemMetrics.End("BTManager.Update");

      DebugDrawHelper.DrawCircle(
        f,
        filter.Transform->Position,
        FP._2,
        filter.Transform->Rotation,
        ColorRGBA.Yellow,
        FP._0_25);
    }
  }
}