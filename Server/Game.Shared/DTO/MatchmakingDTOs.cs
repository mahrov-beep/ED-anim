namespace Game.Shared.DTO {
    using MessagePack;
    using Multicast;
    using System;
    using System.Collections.Generic;

    public enum EMatchmakingJoinStatus {
        Enqueued,
        AlreadyQueued,
        AlreadyInGame,
        Error,
    }

    public enum EMatchmakingQueueStatus {
        Idle,
        Queued,
        Matched,
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class MatchJoinContract {
        [Key(0)]  public string              Region;
        [Key(1)]  public string              RoomName;
        [Key(2)]  public Guid[]              ExpectedUsers;
        [Key(3)]  public Dictionary<string, string> RoomPropsMinimal;
        [Key(4)]  public string              Ticket;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class MatchmakingJoinRequest : IServerRequest {
        [Key(0)] public string GameModeKey;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class MatchmakingJoinResponse : IServerResponse {
        [Key(0)] public EMatchmakingJoinStatus Result;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class MatchmakingCancelRequest : IServerRequest { }

    [MessagePackObject, RequireFieldsInit]
    public sealed class MatchmakingCancelResponse : IServerResponse { }

    [MessagePackObject, RequireFieldsInit]
    public sealed class MatchmakingStatusRequest : IServerRequest { }

    [MessagePackObject, RequireFieldsInit]
    public sealed class MatchmakingStatusResponse : IServerResponse {
        [Key(0)] public EMatchmakingQueueStatus Status;
        [Key(1)] public string                  GameModeKey;
        [Key(2)] public MatchJoinContract       Join;
    }
}