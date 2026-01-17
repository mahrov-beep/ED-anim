namespace Quantum {
  using System;
  using Photon.Deterministic;
  public static unsafe class BTParamsExtensions {
    // public static void AddFPData(this BTParams p, FP data) {
    //   p.Agent->AddFPData(p.FrameThreadSafe, data);
    // }
    //
    // public static void AddIntData(this BTParams p, int data = 0) {
    //   p.Agent->AddIntData(p.FrameThreadSafe, data);
    // }

    public static void SetFPData(this BTParams p, FP data, BTDataIndex index) {
      p.Agent->SetFPData(p.FrameThreadSafe, data, index);
    }

    public static bool ProcessFPDataTimer(this BTParams p, BTDataIndex index) {
      var  timer     = p.GetFPData(index);
      
      var isExpired = timer.ProcessTimer(p.FrameThreadSafe.DeltaTime);
      
      p.SetFPData(timer, index);

      return isExpired;
    }

    public static void SetIntData(this BTParams p, int data, BTDataIndex index) {
      p.Agent->SetIntData(p.FrameThreadSafe, data, index);
    }
    
    public static void SetIntData<T>(this BTParams p, T data, BTDataIndex index) where T : Enum{
      p.Agent->SetIntData(p.FrameThreadSafe, data.ToInt(), index);
    }

    public static FP GetFPData(this BTParams p, BTDataIndex index) {
      return p.Agent->GetFPData(p.FrameThreadSafe, index);
    }

    public static int GetIntData(this BTParams p, BTDataIndex index) {
      return p.Agent->GetIntData(p.FrameThreadSafe, index);
    }

  }
}