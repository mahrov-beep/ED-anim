namespace Game.Shared.DTO {
    using System;
    using MessagePack;
    using Multicast;

    public enum EPartyInviteActionStatus {
        Sent,
        AlreadyInParty,
        AlreadyInvited,
        UserBusy,
        NotFriends,
        Error,
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class PartyInviteRequest : IServerRequest {
        [Key(0)] public Guid TargetUserId;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class PartyInviteResponse : IServerResponse {
        [Key(0)] public EPartyInviteActionStatus Result;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class PartyAcceptInviteRequest : IServerRequest {
        [Key(0)] public Guid LeaderUserId;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class PartyAcceptInviteResponse : IServerResponse {
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class PartyDeclineInviteRequest : IServerRequest {
        [Key(0)] public Guid LeaderUserId;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class PartyDeclineInviteResponse : IServerResponse {
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class PartyLeaveRequest : IServerRequest {
        [Key(0)] public Guid LeaderUserId;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class PartyLeaveResponse : IServerResponse {
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class PartyKickRequest : IServerRequest {
        [Key(0)] public Guid LeaderUserId;
        [Key(1)] public Guid TargetUserId;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class PartyKickResponse : IServerResponse {
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class PartyMakeLeaderRequest : IServerRequest {
        [Key(0)] public Guid LeaderUserId;
        [Key(1)] public Guid TargetUserId;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class PartyMakeLeaderResponse : IServerResponse {
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class PartyStartGameRequest : IServerRequest {
        [Key(0)] public string GameModeKey;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class PartyStartGameResponse : IServerResponse {
        [Key(0)] public string GameId;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class PartyStatusRequest : IServerRequest {
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class PartyStatusResponse : IServerResponse {
        [Key(0)] public Guid   LeaderUserId;
        [Key(1)] public Guid[] Members;
        [Key(2)] public Guid[] ReadyMembers;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class PartySetReadyRequest : IServerRequest {
        [Key(0)] public Guid LeaderUserId;
        [Key(1)] public bool IsReady;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class PartySetReadyResponse : IServerResponse {
    }
}


