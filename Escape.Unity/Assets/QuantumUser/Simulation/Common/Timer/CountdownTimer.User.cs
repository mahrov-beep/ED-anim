using Photon.Deterministic;

namespace Quantum {
  public unsafe partial struct CountdownTimer {
    public bool IsRunning => IsStarted && TimeLeftRaw > FP._0;
    public bool IsDone    => IsStarted && !IsRunning;

    public FP TimeLeft              => IsDone ? FP._0 : !IsStarted ? StartTime : TimeLeftRaw;
    public FP TimeElapsed           => StartTime - TimeLeftRaw;
    public FP NormalizedTimeLeft    => IsDone ? FP._0 : !IsStarted ? FP._1 : TimeLeftRaw / StartTime;
    public FP NormalizedTimeElapsed => FP._1 - NormalizedTimeLeft;

    public void Start(FP time) {
      time = FPMath.Max(time, FP.Epsilon);

      IsStarted   = true;
      StartTime   = time;
      TimeLeftRaw = time;
    }

    public void Tick(FP deltaTime) {
      if (!IsStarted || IsDone) {
        return;
      }

      TimeLeftRaw -= deltaTime;
    }

    public void Reset() {
      IsStarted   = false;
      StartTime   = FP._0;
      TimeLeftRaw = FP._0;
    }
  }
}