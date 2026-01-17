namespace Quantum.Bots {
  using System.Collections.Generic;
  using Photon.Deterministic;

  public unsafe class BotMarkInvisibleSystem : SystemMainThread {
    readonly List<FPVector3> nonBotPositions = new();

    const bool ALWAYS_VISIBLE_PLAYER_BOT = true;
    
    public override void Update(Frame f) {
      var gameMode = f.GameMode;

      nonBotPositions.Clear();

      var nonBotUnits = f.Filter<Transform3D, Unit>();
      while (nonBotUnits.NextUnsafe(out var unitEntity, out var transform, out _)) {
        if (f.TryGet<Bot>(unitEntity, out var bot)) {
          if (!bot.IsPlayerBot || !ALWAYS_VISIBLE_PLAYER_BOT) {
            continue;
          }
        }
        
        nonBotPositions.Add(transform->Position);
      }

      var botUnits = f.Filter<Transform3D, Unit, Bot>();
      while (botUnits.NextUnsafe(out var botEntity, out var transform, out _, out var bot)) {
        if (bot->IsPlayerBot && ALWAYS_VISIBLE_PLAYER_BOT) {
          f.Remove<BotInvisibleByPlayer>(botEntity);
        }
        else if (IsVisibleByAnyPlayer(transform, gameMode)) {
          f.Remove<BotInvisibleByPlayer>(botEntity);
        }
        else {
          f.Set(botEntity, new BotInvisibleByPlayer());
        }
      }
    }

    bool IsVisibleByAnyPlayer(Transform3D* transform, GameModeAsset gameMode) {
      var maxDistance    = gameMode.OptimizeBotsRange;
      var maxDistanceSqr = maxDistance * maxDistance;

      var myPosition = transform->Position;

      foreach (var otherPosition in nonBotPositions) {
        if (FPVector3.DistanceSquared(otherPosition, myPosition) < maxDistanceSqr) {
          return true;
        }
      }

      return false;
    }
  }
}