using UnityEngine.Scripting;

namespace Quantum {
  using UnityEngine;
  using Photon.Deterministic;
  using Byte = System.Byte;
  [Preserve]
  public unsafe class PlayerJoiningSystem : SystemMainThread,
          ISignalOnPlayerAdded,
          ISignalOnPlayerDisconnected {

    public void OnPlayerAdded(Frame f, PlayerRef playerRef, bool firstTime) {
      if (!firstTime) {
        return;
      }

      var runtimePlayer = f.GetPlayerData(playerRef);
      var characterRef = f.Global->CreatePlayerCharacter(f,
              loadoutSnapshot: runtimePlayer.Loadout,
              out _
      );

      var unit = f.GetPointer<Unit>(characterRef);
      unit->PlayerRef = playerRef;
      var teamIndex = MapPartyAndSpawn(f, runtimePlayer.PartyKeyHash, characterRef);
      f.Set(characterRef, new Team { Index = (byte)teamIndex });

      var navMeshAgentConfig = f.FindAsset<NavMeshAgentConfig>(f.SimulationConfig.Navigation.DefaultNavMeshAgent);
      var pathfinder         = NavMeshPathfinder.Create(f, characterRef, navMeshAgentConfig);
      f.Set(characterRef, pathfinder);
    }

    public void OnPlayerDisconnected(Frame f, PlayerRef player) {
      if (Application.isEditor) {
        Debug.LogError($"Player {player} disconnected");
      }
    }

    public override void OnInit(Frame f) {
      var teamsData = f.AllocateList<TeamData>();
      f.Global->TeamsData = teamsData;
    }

    public override void Update(Frame f) { }

    static int MapPartyAndSpawn(Frame f, ulong partyKeyHash, EntityRef characterRef) {
      var teams = f.ResolveList(f.Global->TeamsData);
      for (int i = 0; i < teams.Count; i++) {
        if (teams[i].PartyKeyHash == partyKeyHash) {
          var spawnRef = teams[i].TeamSpawnPoint;
          var ordinal  = CountTeamMembers(f, (byte)i);
          TeleportAroundSpawn(f, characterRef, spawnRef, ordinal);
          SetExitZone(f, characterRef, teams[i].TeamExitZoneRef);
          return i;
        }
      }

      var chosenSpawn = FindSpawnByUnitPosition(f, characterRef);
      var exitRef     = f.Get<SpawnPoint>(chosenSpawn).exitZoneRef;

      teams.Add(new TeamData { PartyKeyHash = partyKeyHash, TeamSpawnPoint = chosenSpawn, TeamExitZoneRef = exitRef });
      var idx = teams.Count - 1;

      TeleportAroundSpawn(f, characterRef, chosenSpawn, 0);
      SetExitZone(f, characterRef, exitRef);

      return idx;
    }

    static EntityRef FindSpawnByUnitPosition(Frame f, EntityRef unitRef) {
      var uT = f.GetPointer<Transform3D>(unitRef);
      var spawns = f.Filter<SpawnPoint, Transform3D>();
      while (spawns.NextUnsafe(out var e, out _, out var sT)) {
        if (sT->Position == uT->Position) {
          return e;
        }
      }
      var any = f.Filter<SpawnPoint, Transform3D>();
      if (any.NextUnsafe(out var e2, out _, out _)) {
        return e2;
      }
      return EntityRef.None;
    }

    static void TeleportToSpawn(Frame f, EntityRef unitRef, EntityRef spawnRef) {
      if (spawnRef == EntityRef.None) {
        return;
      }
      TransformHelper.CopyPositionAndRotation(f, spawnRef, unitRef);
    }

    static void SetExitZone(Frame f, EntityRef unitRef, EntityRef exitZoneRef) {
      var unit = f.GetPointer<Unit>(unitRef);
      unit->TargetExitZone = exitZoneRef;
    }

    static int CountTeamMembers(Frame f, byte teamIndex) {
      var filter = f.Filter<Unit, Team, Transform3D>();
      var count = 0;
      while (filter.NextUnsafe(out _, out _, out Team* team, out _)) {
        if (team->Index == teamIndex) {
          count++;
        }
      }
      return count;
    }

    static void TeleportAroundSpawn(Frame f, EntityRef unitRef, EntityRef spawnRef, int ordinal) {
      if (spawnRef == EntityRef.None) return;

      var sT = f.GetPointer<Transform3D>(spawnRef);
      var uT = f.GetPointer<Transform3D>(unitRef);

      TransformHelper.CopyPositionAndRotation(f, spawnRef, unitRef);

      var slots  = QConstants.MAX_TEAM_SIZE;
      var idx    = ordinal % slots;
      var angle  = (FP)idx * (FP)1 / (FP)slots * FP.PiTimes2;
      var radius = FP._1_50;

      var dir2 = new FPVector2(FPMath.Cos(angle), FPMath.Sin(angle));
      var desiredXZ = new FPVector2(sT->Position.X, sT->Position.Z) + dir2 * radius;

      var navPos = NavMeshHelper.FindNearestPointOnNavMesh(f, desiredXZ.XOY);
      uT->Teleport(f, navPos);
    }
  }

}