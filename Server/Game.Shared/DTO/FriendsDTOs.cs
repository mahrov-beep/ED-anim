namespace Game.Shared.DTO {
    using System;
    using JetBrains.Annotations;
    using MessagePack;
    using Multicast;

    public enum EFriendActionStatus {
        PendingCreated,
        AlreadyPendingOutgoing,
        AlreadyPendingIncoming,
        AlreadyFriends,
        SelfNotAllowed,
        NotFound,
        Blocked,
        Removed,
        Error,
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class FriendInfoDto {
        [Key(0)] public Guid        Id;
        [Key(1)] public string      NickName;
        [Key(2)] public EUserStatus Status;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class FriendsListRequest : IServerRequest { }

    [MessagePackObject, RequireFieldsInit]
    public sealed class FriendsListResponse : IServerResponse {
        [Key(0)][CanBeNull]
        public FriendInfoDto[] Friends;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class IncomingRequestsRequest : IServerRequest { }

    [MessagePackObject, RequireFieldsInit]
    public sealed class IncomingRequestsResponse : IServerResponse {
        [Key(0)][CanBeNull]
        public FriendInfoDto[] IncomingRequests;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class FriendAddRequest : IServerRequest {
        [Key(0)] public Guid Id;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class FriendAddByNicknameRequest : IServerRequest {
        [Key(0)] public string NickName;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class FriendAddByNicknameResponse : IServerResponse {
        [Key(0)] public EFriendActionStatus Result;
        [Key(1)] public bool                Created;
        [Key(2)] public Guid                ResolvedUserId;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class FriendAddResponse : IServerResponse {
        [Key(0)] public EFriendActionStatus Result;
        [Key(2)] public bool                Created;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class FriendAcceptRequest : IServerRequest {
        [Key(0)] public Guid Id;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class FriendAcceptResponse : IServerResponse {
        [Key(0)] public EFriendActionStatus Result;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class FriendDeclineRequest : IServerRequest {
        [Key(0)] public Guid Id;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class FriendDeclineResponse : IServerResponse {
        [Key(0)] public EFriendActionStatus Result;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class FriendRemoveRequest : IServerRequest {
        [Key(0)] public Guid Id;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class FriendRemoveResponse : IServerResponse {
        [Key(0)] public EFriendActionStatus Result;
    }

    public enum EFriendBulkAction {
        Accept  = 1,
        Decline = 2,
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class FriendsIncomingBulkRequest : IServerRequest {
        [Key(0)] public EFriendBulkAction Action;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class FriendsIncomingBulkResponse : IServerResponse { }
}