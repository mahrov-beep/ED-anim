using VisDict = Quantum.Collections.QDictionary<int, Quantum.GlobalVisibilityData>;

namespace Quantum {

internal unsafe class VisionSystem : SystemMainThread {
  public override void OnInit(Frame f) {
    base.OnInit(f);

    f.Global->VisibleUnitsByTeam = f.AllocateDictionary<int, GlobalVisibilityData>();
  }

  public override void Update(Frame f) {
    var visibilityDict = f.ResolveDictionary(f.Global->VisibleUnitsByTeam);

    CleanupVisibilityDict(f, visibilityDict);
    AddNewTeamsToVisibilityDict(f, visibilityDict);
    ProcessForceVisibility(f);
    ProcessDirectVisibility(f, visibilityDict);
    ProcessVisibilityByAnyEnemy(f, visibilityDict);
  }

  void CleanupVisibilityDict(Frame f, VisDict visibilityDict) {
    foreach (var kvp in visibilityDict) {
      if (!visibilityDict.TryGetValuePointer(kvp.Key, out GlobalVisibilityData* data)) {
        continue;
      }

      var visibleCharactersHashset = f.ResolveHashSet(data->visibleUnits);
      visibleCharactersHashset.Clear();
    }
  }

  void AddNewTeamsToVisibilityDict(Frame f, VisDict visibilityDict) {
    var teams = f.Filter<Unit, Team>();
    while (teams.NextUnsafe(out _, out _, out Team* team)) {
      if (!visibilityDict.TryGetValuePointer(team->Index, out GlobalVisibilityData* _)) {
        visibilityDict.Add(team->Index, new GlobalVisibilityData {
                visibleUnits = f.AllocateHashSet<EntityRef>(),
        });
      }
    }
  }

  void ProcessForceVisibility(Frame f) {
    var players = f.Filter<Vision, Team>();
    while (players.NextUnsafe(out var playerEntity, out var vision, out _)) {
      vision->IsForceVisibleInInvisibilityZone_Out = IsPlayerForceVisibleInInvisibilityZone(f, playerEntity);
    }
  }

  void ProcessDirectVisibility(Frame f, VisDict visibilityDict) {
    var playersA = f.Filter<Transform3D, Unit, Team>();
    while (playersA.NextUnsafe(out var playerEntityA, out _, out _, out var teamA)) {

      var playersB = f.Filter<Transform3D, Unit, Team>();
      while (playersB.NextUnsafe(out var playerEntityB, out _, out _, out var teamB)) {
        var visibilitySetA = f.ResolveHashSet(visibilityDict[teamA->Index].visibleUnits);

        if (visibilitySetA.Contains(playerEntityB)) {
          continue;
        }

        if (!IsPlayerVisible(f, visibilityDict, playerEntityA, playerEntityB)) {
          continue;
        }

        visibilitySetA.Add(playerEntityB);
      }
    }
  }

  void ProcessVisibilityByAnyEnemy(Frame f, VisDict visibilityDict) {
    var players = f.Filter<Vision, Team>();
    while (players.NextUnsafe(out var playerEntity, out var vision, out _)) {
      vision->IsVisibleByAnyEnemy_Out = IsVisibleByAnyEnemy(f, visibilityDict, playerEntity);
    }
  }

  bool IsPlayerForceVisibleInInvisibilityZone(Frame f, EntityRef player) {
    // на враге какой-то бафф который делает его видимым
    // например, враг выстрелил из оружия или использовал скилл
    if (AttributesHelper.IsValueSet(f, player, EAttributeType.Set_ForceVisibleInInvisibilityZone)) {
      return true;
    }

    Vision* vision = f.Unsafe.GetPointer<Vision>(player);

    // враг прицеливается джойстиком или скилом
    if (vision->IsInAim_Out) {
      return true;
    }

    return false;
  }

  // А - игрок, Б - враг
  // ! результат не симметричный, то есть если А видит Б, это не значит что Б видит А
  bool IsPlayerVisible(Frame f, VisDict visibilityDict, EntityRef playerA, EntityRef playerB) {
    Team* teamA = f.Unsafe.GetPointer<Team>(playerA);
    Team* teamB = f.Unsafe.GetPointer<Team>(playerB);

    // если игроки в одной команде, они всегда видят друг друга
    if (teamA->Index == teamB->Index) {
      return true;
    }

    Transform3D* transformA = f.Unsafe.GetPointer<Transform3D>(playerA);
    Transform3D* transformB = f.Unsafe.GetPointer<Transform3D>(playerB);

    Vision* visionA = f.GetPointer<Vision>(playerA);
    Vision* visionB = f.GetPointer<Vision>(playerB);

    if (!IsInVisibilityArea(f, playerA, visionA, transformA, transformB)) {
      return false;
    }

    var hit = f.Physics3D.Linecast(transformA->Position, transformB->Position,
            PhysicsHelper.GetStaticLayerMask(f), QueryOptions.HitStatics);

    // враг за стеной, не видим
    if (hit.HasValue) {
      return false;
    }

    // враг в какой-то зоне невидимости и мы не в этой зоне, не видим
    // например, игрока Б в кустах, а А не в тех же кустах
    if (visionB->IsForceVisibleInInvisibilityZone_Out == false &&
            visionB->InvisibilityZone != EntityRef.None &&
            visionA->InvisibilityZone != visionB->InvisibilityZone) {
      return false;
    }

    return true;
  }

  bool IsInVisibilityArea(Frame f, EntityRef playerA, Vision* visionA, Transform3D* transformA, Transform3D* transformB) {
    var distanceTarget = TransformHelper.Distance(transformA, transformB);

    var weaponVision  = visionA->WeaponVisibilityArea;
    var visionForward = visionA->VisibilityForwardArea;
    var vision360     = visionA->Visibility360Area;

    bool isVisiblyInAnyArea =
            CheckVisibilityInArea(weaponVision) ||
            CheckVisibilityInArea(visionForward) ||
            CheckVisibilityInArea(vision360);

    return isVisiblyInAnyArea;

    bool CheckVisibilityInArea(VisibilityArea vision) {
      if (vision.IsEnable) {
        var visionDistance = vision.Radius;

        if (f.TryGetPointer(playerA, out Unit* unit)) {
          visionDistance *= unit->CurrentStats.visionDistanceMultiplier;
        }

        if (distanceTarget < visionDistance) {
          if (TransformHelper.IsLookAtTarget(transformA, transformB, vision.Angle)) {
            return true;
          }
        }
      }
      return false;
    }

  }

  bool IsVisibleByAnyEnemy(Frame f, VisDict visibilityDict, EntityRef playerEntity) {
    int teamId = f.Get<Team>(playerEntity).Index;

    foreach (var visibilityPair in visibilityDict) {
      if (visibilityPair.Key == teamId) {
        continue;
      }

      var enemyVisibilitySet = f.ResolveHashSet(visibilityPair.Value.visibleUnits);

      if (enemyVisibilitySet.Contains(playerEntity)) {
        return true;
      }
    }

    return false;
  }
}
}