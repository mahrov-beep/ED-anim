namespace Game.Shared.ServerEvents {
    using MessagePack;
    using Multicast;
    using System;
    using Game.Shared.DTO;

    [Union(1, typeof(DebugLogAppServerEvent))]
    [Union(2, typeof(GameEndedAppServerEvent))]
    [Union(3, typeof(UserProfileUpdatedAppServerEvent))]
    [Union(4, typeof(FriendRequestIncomingAppServerEvent))]
    [Union(5, typeof(FriendAddedAppServerEvent))]
    [Union(6, typeof(FriendRemovedAppServerEvent))]
    [Union(7, typeof(FriendStatusChangedAppServerEvent))]
    [Union(8, typeof(PartyInviteReceivedAppServerEvent))]
    [Union(9, typeof(PartyUpdatedAppServerEvent))]
    [Union(10, typeof(PartyGameStartedAppServerEvent))]
    [Union(11, typeof(PartyDisbandedAppServerEvent))]
    [Union(12, typeof(PartyMatchmakingStartedAppServerEvent))]
    [Union(13, typeof(PartyMatchmakingCanceledAppServerEvent))]
    [Union(14, typeof(PartyMatchmakingMatchedAppServerEvent))]
    public interface IAppServerEvent : IServerEvent {
    }

    [MessagePackObject, RequireFieldsInit] public sealed class DebugLogAppServerEvent : IAppServerEvent {
        [Key(0)] public string Message;
    }

    [MessagePackObject, RequireFieldsInit] public sealed class GameEndedAppServerEvent : IAppServerEvent {
        [Key(0)] public string GameId;
    }

    [MessagePackObject, RequireFieldsInit] public sealed class UserProfileUpdatedAppServerEvent : IAppServerEvent {
    }

    [MessagePackObject, RequireFieldsInit] public sealed class FriendRequestIncomingAppServerEvent : IAppServerEvent {
        [Key(0)] public Guid RequesterId;
    }

    [MessagePackObject, RequireFieldsInit] public sealed class FriendAddedAppServerEvent : IAppServerEvent {
        [Key(0)] public Guid FriendId;
    }

    [MessagePackObject, RequireFieldsInit] public sealed class FriendRemovedAppServerEvent : IAppServerEvent {
        [Key(0)] public Guid FriendId;
    }

    [MessagePackObject, RequireFieldsInit] public sealed class FriendStatusChangedAppServerEvent : IAppServerEvent {
        [Key(0)] public Guid        FriendId;
        [Key(1)] public EUserStatus Status;
    }

    [MessagePackObject, RequireFieldsInit] public sealed class PartyInviteReceivedAppServerEvent : IAppServerEvent {
        [Key(0)] public Guid LeaderUserId;
    }

    [MessagePackObject, RequireFieldsInit] public sealed class PartyUpdatedAppServerEvent : IAppServerEvent {
        [Key(0)] public Guid   LeaderUserId;
        [Key(1)] public Guid[] Members;
        [Key(2)] public Guid[] ReadyMembers;
    }

    [MessagePackObject, RequireFieldsInit] public sealed class PartyGameStartedAppServerEvent : IAppServerEvent {
        [Key(0)] public Guid   LeaderUserId;
        [Key(1)] public string GameId;
    }

    [MessagePackObject, RequireFieldsInit] public sealed class PartyDisbandedAppServerEvent : IAppServerEvent {
        [Key(0)] public Guid LeaderUserId;
    }

    [MessagePackObject, RequireFieldsInit] public sealed class PartyMatchmakingStartedAppServerEvent : IAppServerEvent {
        [Key(0)] public Guid   LeaderUserId;
        [Key(1)] public string GameModeKey;
    }

    [MessagePackObject, RequireFieldsInit] public sealed class PartyMatchmakingCanceledAppServerEvent : IAppServerEvent {
        [Key(0)] public Guid LeaderUserId;
        [Key(1)] public Guid CanceledByUserId;
    }

    [MessagePackObject, RequireFieldsInit] public sealed class PartyMatchmakingMatchedAppServerEvent : IAppServerEvent {
        [Key(0)] public Guid     LeaderUserId;
        [Key(1)] public string   RoomName;
        [Key(2)] public string   Region;
        [Key(3)] public Guid[]   ExpectedUsers;
        [Key(4)] public string   GameModeKey;
    }
}