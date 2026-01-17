namespace Quantum {
  using System;

  [Serializable]
  public unsafe class HasNoEnemy : BTDecorator {
    public override bool CheckConditions(BTParams p, ref AIContext c) {
      var f = p.FrameThreadSafe;
      var data = c.Data();

      var hasVisible = BotPerceptionHelper.HasVisibleEnemies(f, data.Bot);
      var hasHeard = BotPerceptionHelper.HasHeardEnemies(f, data.Bot);

      return !hasVisible && !hasHeard;
    }
  }
}
