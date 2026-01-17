namespace Quantum {
  using Photon.Deterministic;

  public static unsafe class BotPerceptionHelper {
    public static bool HasVisibleEnemies(FrameThreadSafe f, Bot* bot) {
      return f.ResolveList(bot->VisibleEnemies).Count > 0;
    }

    public static bool HasHeardEnemies(FrameThreadSafe f, Bot* bot) {
      return f.ResolveList(bot->HeardEnemies).Count > 0;
    }

    public static EntityRef GetFirstReactedEnemy(FrameThreadSafe f, Bot* bot, PerceptionMemory* memory) {
      var visibleEnemies = f.ResolveList(bot->VisibleEnemies);
      if (visibleEnemies.Count == 0) {
        return EntityRef.None;
      }

      var config = f.FindAsset(bot->VisionModule);
      if (config == null) {
        return visibleEnemies.Count > 0 ? visibleEnemies[0] : EntityRef.None;
      }

      var knownTargets = f.ResolveDictionary(memory->KnownTargets);

      EntityRef firstKnocked = EntityRef.None;

      foreach (var enemy in visibleEnemies) {
        if (!knownTargets.TryGetValue(enemy, out var entry)) {
          continue;
        }

        if (entry.ContinuousVisionTime < config.reactionDelay) {
          continue;
        }

        if (f.Has<CharacterStateKnocked>(enemy)) {
          if (firstKnocked == EntityRef.None) {
            firstKnocked = enemy;
          }
          continue;
        }

        return enemy;
      }

      return firstKnocked;
    }

    public static EntityRef GetClosestReactedEnemy(FrameThreadSafe f, Bot* bot, PerceptionMemory* memory, FPVector3 position) {
      var visibleEnemies = f.ResolveList(bot->VisibleEnemies);
      if (visibleEnemies.Count == 0) {
        return EntityRef.None;
      }

      var config = f.FindAsset(bot->VisionModule);
      var knownTargets = f.ResolveDictionary(memory->KnownTargets);

      EntityRef closestAlive = EntityRef.None;
      EntityRef closestKnocked = EntityRef.None;
      FP closestAliveDistSqr = FP.MaxValue;
      FP closestKnockedDistSqr = FP.MaxValue;

      foreach (var enemy in visibleEnemies) {
        if (config != null && knownTargets.TryGetValue(enemy, out var entry)) {
          if (entry.ContinuousVisionTime < config.reactionDelay) {
            continue;
          }
        }

        if (!f.TryGetPointer(enemy, out Transform3D* enemyTransform)) {
          continue;
        }

        var distSqr = FPVector3.DistanceSquared(position, enemyTransform->Position);
        var isKnocked = f.Has<CharacterStateKnocked>(enemy);

        if (isKnocked) {
          if (distSqr < closestKnockedDistSqr) {
            closestKnockedDistSqr = distSqr;
            closestKnocked = enemy;
          }
        }
        else {
          if (distSqr < closestAliveDistSqr) {
            closestAliveDistSqr = distSqr;
            closestAlive = enemy;
          }
        }
      }

      return closestAlive != EntityRef.None ? closestAlive : closestKnocked;
    }

    public static EntityRef GetClosestVisibleEnemy(FrameThreadSafe f, Bot* bot, FPVector3 position) {
      var visibleEnemies = f.ResolveList(bot->VisibleEnemies);
      if (visibleEnemies.Count == 0) {
        return EntityRef.None;
      }

      EntityRef closest        = EntityRef.None;
      FP        closestDistSqr = FP.MaxValue;

      foreach (var enemy in visibleEnemies) {
        if (!f.TryGetPointer(enemy, out Transform3D* enemyTransform)) {
          continue;
        }

        var distSqr = FPVector3.DistanceSquared(position, enemyTransform->Position);
        if (distSqr < closestDistSqr) {
          closestDistSqr = distSqr;
          closest        = enemy;
        }
      }

      return closest;
    }

    public static bool IsEnemyInAttackRange(FrameThreadSafe f, FPVector3 position, EntityRef targetRef, Weapon* weapon) {
      if (weapon == null) {
        return false;
      }

      if (!f.TryGetPointer(targetRef, out Transform3D* targetTransform)) {
        return false;
      }

      var attackDistance = weapon->CurrentStats.attackDistance.AsFP;
      var distSqr        = FPVector3.DistanceSquared(position, targetTransform->Position);

      return distSqr <= attackDistance * attackDistance;
    }

    public static bool TryGetFirstAlertEnemy(FrameThreadSafe f, Bot* bot, PerceptionMemory* memory,
        out EntityRef enemy) {
      enemy = EntityRef.None;

      var visibleEnemies = f.ResolveList(bot->VisibleEnemies);
      var knownTargets   = f.ResolveDictionary(memory->KnownTargets);

      foreach (var kvp in knownTargets) {
        if (visibleEnemies.Contains(kvp.Key)) {
          continue;
        }

        enemy = kvp.Key;
        return true;
      }

      return false;
    }

    public static EntityRef GetBestAttackTarget(FrameThreadSafe f, EntityRef botRef, Bot* bot, Weapon* weapon, FPVector3 position) {
      if (weapon == null) {
        return EntityRef.None;
      }

      var visibleEnemies = f.ResolveList(bot->VisibleEnemies);
      if (visibleEnemies.Count == 0) {
        return EntityRef.None;
      }

      EntityRef best        = EntityRef.None;
      FP        bestDistSqr = FP.MaxValue;

      foreach (var enemy in visibleEnemies) {
        if (!weapon->CanShoot(f, botRef, enemy)) {
          continue;
        }

        if (!f.TryGetPointer(enemy, out Transform3D* enemyTransform)) {
          continue;
        }

        var distSqr = FPVector3.DistanceSquared(position, enemyTransform->Position);
        if (distSqr < bestDistSqr) {
          bestDistSqr = distSqr;
          best        = enemy;
        }
      }

      return best;
    }
  }
}