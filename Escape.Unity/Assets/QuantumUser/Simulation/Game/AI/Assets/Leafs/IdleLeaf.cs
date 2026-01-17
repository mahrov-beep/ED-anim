namespace Quantum {
  using System;
  using Photon.Deterministic;
  using UnityEngine.Serialization;

  [Serializable]
  public unsafe class IdleLeaf : BTLeaf {
    public BTDataIndex endTimeIndex;

    public override void Init(BTParams p, ref AIContext c) {
      p.SetFPData(FP._0, endTimeIndex);
    }

    public override void OnEnter(BTParams p, ref AIContext c) {
      base.OnEnter(p, ref c);

      FrameThreadSafe f    = p.FrameThreadSafe;
      AIContextUser   data = c.Data();

      BotNavMeshHelper.Stop(f, p.Entity, data.Pathfinder);
      data.InputContainer->ResetAllInput();

      var config   = f.FindAsset(data.Bot->PatrolState);
      var duration = data.RNG.Next(config.PauseMin, config.PauseMax);
      var endTime  = f.GetSingleton<BotSDKGlobals>().Data.GameTime + duration;
      p.SetFPData(endTime, endTimeIndex);
    }

    protected override BTStatus OnUpdate(BTParams p, ref AIContext c) {
      var f    = p.FrameThreadSafe;
      var data = c.Data();

      var gameTime = f.GetSingleton<BotSDKGlobals>().Data.GameTime;
      return gameTime < p.GetFPData(endTimeIndex) ? BTStatus.Running : BTStatus.Success;
    }

    public override void OnExit(BTParams p, ref AIContext c) {
      base.OnExit(p, ref c);
      var data = c.Data();
      data.InputContainer->ResetAllInput();
    }

    public override void OnAbort(BTParams p, ref AIContext c, BTAbort abortType) {
      base.OnAbort(p, ref c, abortType);
      var data = c.Data();
      data.InputContainer->ResetAllInput();
    }
  }
}