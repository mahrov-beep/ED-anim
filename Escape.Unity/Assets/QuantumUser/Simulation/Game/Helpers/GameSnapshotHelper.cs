namespace Quantum {
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public static unsafe class GameSnapshotHelper {
    public static GameSnapshot Make(Frame f) {
      var snapshot = new GameSnapshot {
        GameMode    = f.GameMode.gameModeKey,
        GameRule    = f.GameMode.rule,
        IsGameEnded = f.Global->GameState is EGameStates.BeforeExit or EGameStates.Exited,
        Teams       = new List<GameSnapshotTeam>(),
        Users       = new List<GameSnapshotUser>(),
      };

      var teamIndices = new HashSet<int>();

      var charactersFilter = f.Filter<Unit, CharacterLoadout, Team, Health>();
      while (charactersFilter.Next(out var characterEntity, out _, out _, out Team team, out _)) {
        snapshot.Users.Add(MakeUser(f, characterEntity));
        teamIndices.Add(team.Index);
      }

      snapshot.Teams.AddRange(teamIndices.Select(ind => new GameSnapshotTeam {
        TeamNumber = ind,
      }));

      return snapshot;
    }

    static GameSnapshotUser MakeUser(Frame f, EntityRef characterEntity) {
      var unit        = f.Get<Unit>(characterEntity);
      var actorNumber = f.PlayerToActorId(unit.PlayerRef).GetValueOrDefault(-1);
      var gameTeamId  = f.Get<Team>(characterEntity).Index;
      var isDead      = f.Get<Health>(characterEntity).IsDead;
      var frags       = unit.Frags;

      return new GameSnapshotUser {
        ActorNumber = actorNumber,
        GameTeamId  = gameTeamId,
        Frags       = frags,
        IsDead      = isDead,
        Loadout     = MakeLoadout(f, characterEntity),
        UserId = Guid.Empty, // будет проставлено в контроллере т.к. здесь нет доступа к ID
      };
    }

    public static GameSnapshotLoadout MakeLoadout(Frame f, EntityRef characterEntity) {
      return new GameSnapshotLoadout {
        SlotItems  = MakeLoadoutSlotsSnapshot(f, characterEntity),
        TrashItems = MakeLoadoutTrashSnapshot(f, characterEntity),
      };
    }

    static GameSnapshotLoadoutItem[] MakeLoadoutSlotsSnapshot(Frame f, EntityRef characterEntity) {
      if (!f.TryGet(characterEntity, out CharacterLoadout loadout)) {
        return Array.Empty<GameSnapshotLoadoutItem>();
      }

      var resultItems = new GameSnapshotLoadoutItem[CharacterLoadoutSlotsExtension.CHARACTER_LOADOUT_SLOTS];
      foreach (var slot in CharacterLoadoutSlotsExtension.AllValidSlots) {
        resultItems[slot.ToInt()] = MakeItem(f, loadout.ItemAtSlot(slot));
      }

      return resultItems;
    }

    static GameSnapshotLoadoutItem[] MakeLoadoutTrashSnapshot(Frame f, EntityRef characterEntity) {
      if (!f.TryGet(characterEntity, out CharacterLoadout loadout)) {
        return Array.Empty<GameSnapshotLoadoutItem>();
      }

      var trashList = loadout.GetTrashItems(f);
      var safeList = loadout.GetSafeItems(f);
      
      var resultItems = new GameSnapshotLoadoutItem[trashList.Count + safeList.Count];
      var index = 0;

      for (var i = 0; i < trashList.Count; i++) {
        resultItems[index++] = MakeItem(f, trashList[i]);
      }

      for (var i = 0; i < safeList.Count; i++) {
        resultItems[index++] = MakeItem(f, safeList[i]);
      }

      return resultItems;
    }

    public static GameSnapshotLoadoutItem MakeItem(Frame f, EntityRef itemEntity) {
      if (!f.Exists(itemEntity)) {
        return null;
      }

      var item = f.Get<Item>(itemEntity);

      var result = new GameSnapshotLoadoutItem {
        ItemGuid = item.MetaGuid,
        ItemKey  = f.FindAsset(item.Asset).ItemKey,
        IndexI   = item.IndexI,
        IndexJ   = item.IndexJ,
        Rotated  = item.Rotated,
        Used     = item.Used,
        SafeGuid = item.SafeGuid,
        AddToLoadoutAfterFail = item.AddToLoadoutAfterFail, 

        WeaponAttachments = null,
      };

      if (f.TryGet(itemEntity, out WeaponItem weaponItem) && weaponItem.HasAnyAttachment(f)) {
        result.WeaponAttachments = new GameSnapshotLoadoutWeaponAttachment[WeaponAttachmentSlotsExtension.WEAPON_ATTACHMENT_SLOTS];
        foreach (var weaponSlot in WeaponAttachmentSlotsExtension.AllValidSlots) {
          result.WeaponAttachments[weaponSlot.ToInt()] = MakeWeaponAttachment(f, weaponItem.AttachmentAtSlot(weaponSlot));
        }
      }

      return result;
    }

    static GameSnapshotLoadoutWeaponAttachment MakeWeaponAttachment(Frame f, EntityRef weaponAttachmentEntity) {
      if (!f.Exists(weaponAttachmentEntity)) {
        return null;
      }

      var item = f.Get<Item>(weaponAttachmentEntity);

      return new GameSnapshotLoadoutWeaponAttachment {
        ItemGuid = item.MetaGuid,
        ItemKey  = f.FindAsset(item.Asset).ItemKey,
        IndexI   = item.IndexI,
        IndexJ   = item.IndexJ,
        Used     = item.Used,
        Rotated  = item.Rotated,
      };
    }
  }
}