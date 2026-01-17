namespace Quantum {
  public static unsafe partial class AIParamExtensions {
    public static unsafe T ResolveFromBT<T>
            (this AIParamBase<T> aiParam, BTParams p, ref AIContext c) {

      if (aiParam.Source == AIParamSource.None) {
        p.LogError("AIParamSource is None");
      }

      var f = p.FrameThreadSafe;
      var e = p.Entity;

      AssetRef<AIConfigBase> aiConfigRef = aiParam.Source == AIParamSource.Config
              ? f.GetPointer<BTAgent>(e)->Config
              : default;

      return aiParam.Resolve(f, e, aiConfigRef, ref c);
    }

    public static unsafe T ResolveFromBT<T>
            (this AIParamBase<T> aiParam, BTParams p) {

      if (aiParam.Source == AIParamSource.None) {
        p.LogError("AIParamSource is None");
      }

      var f = p.FrameThreadSafe;
      var e = p.Entity;

      AssetRef<AIConfigBase> aiConfigRef = aiParam.Source == AIParamSource.Config
              ? f.GetPointer<BTAgent>(e)->Config
              : default;

      AIContext c = default;

      return aiParam.Resolve(f, e, aiConfigRef, ref c);
    }
  }
}