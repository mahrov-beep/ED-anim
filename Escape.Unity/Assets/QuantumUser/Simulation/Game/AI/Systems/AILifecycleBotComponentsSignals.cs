namespace Quantum {
  using UnityEngine.Scripting;

  [Preserve]
  public unsafe class AILifecycleComponentsSignals : SystemSignalsOnly,
    ISignalOnComponentAdded<AIBlackboardComponent>,
    ISignalOnComponentRemoved<AIBlackboardComponent>,
    ISignalOnComponentAdded<BTAgent>,
    ISignalOnComponentRemoved<BTAgent> {
    public void OnAdded(Frame f, EntityRef e, AIBlackboardComponent* c) {
      var btConfig = f.FindAsset(f.GameMode.BotGlobalConfig).btAgentConfig;

      // Assert.Check(btConfig.BBInitializer, "Has not Blackboard config");
      //
      // AIBlackboardInitializer.InitializeBlackboard(f, c, btConfig.BBInitializer);
    }

    public void OnRemoved(Frame f, EntityRef e, AIBlackboardComponent* c) =>
      c->Free(f);

    public void OnAdded(Frame f, EntityRef e, BTAgent* c) {
      var btConfig = f.FindAsset(f.GameMode.BotGlobalConfig).btAgentConfig;

      if (c->Tree == default) {
        Assert.Check(btConfig.Tree.IsValid, "Has not set BT Root");
        BTRoot btRootAsset = f.FindAsset<BTRoot>(btConfig.Tree);
        BTManager.Init(f, e, btRootAsset);
      }
      else {
        BTRoot btRootAsset = f.FindAsset<BTRoot>(c->Tree);
        BTManager.Init(f, e, btRootAsset);
      }
    }

    public void OnRemoved(Frame f, EntityRef e, BTAgent* c) =>
      c->Free(f);
  }
}