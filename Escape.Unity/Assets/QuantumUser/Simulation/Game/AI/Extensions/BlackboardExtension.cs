namespace Quantum {
  using Photon.Deterministic;
  public static unsafe class BlackboardExtension {

    /// <summary>
    /// Set value and trigger reactive decorators
    /// </summary>
    public static void SetReactive(
            this AIBlackboardValueKey bbValueKey, EntityRef value, BTParams p, ref AIContext c) {

      // p.Frame.LogTrace($"SetAndTrigger {bbValueKey.Key} : {value} for {p.Entity}", Frame.ELogTag.AI);

      var entry = p.Blackboard->Set(p.Frame, bbValueKey.Key, value);

      entry->TriggerDecorators(p, ref c);
    }

    /// <summary>
    /// Set value and trigger reactive decorators
    /// </summary>
    public static void Set(
            this AIBlackboardValueKey bbValueKey, EntityRef value, BTParams p, ref AIContext c) {

      // p.Frame.LogTrace($"SetAndTrigger {bbValueKey.Key} : {value} for {p.Entity}", Frame.ELogTag.AI);

      p.Blackboard->Set(p.Frame, bbValueKey.Key, value);
    }

    /// <summary>
    /// Set value and trigger reactive decorators
    /// </summary>
    public static void SetReactive(
            this AIBlackboardValueKey bbValueKey, FPVector2 value, BTParams p, ref AIContext c) {

      // p.Frame.LogTrace($"SetAndTrigger {bbValueKey.Key} : {value} for {p.Entity}, Frame.ELogTag.AI");

      var entry = p.Blackboard->Set(p.FrameThreadSafe, bbValueKey.Key, value);
      entry->TriggerDecorators(p, ref c);
    }

    public static EntityRef GetEntityRef(
            this AIBlackboardValueKey bbValueKey,
            BTParams p,
            ref AIContext c) {

      return p.Blackboard->GetEntityRef(p.FrameThreadSafe, bbValueKey.Key);
    }
  }
}