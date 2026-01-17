namespace Game.Shared.DTO {
    using System;
    using MessagePack;
    using Multicast;

    [MessagePackObject, RequireFieldsInit]
    public class UserDeleteRequest : IServerRequest {
    }

    [MessagePackObject, RequireFieldsInit]
    public class UserDeleteResponse : IServerResponse {
    }

    [MessagePackObject, RequireFieldsInit]
    public class UserGetInfoRequest : IServerRequest {
        [Key(0)] public Guid UserId;
    }

    [MessagePackObject, RequireFieldsInit]
    public class UserGetInfoResponse : IServerResponse {
        [Key(0)] public string NickName;
        [Key(1)] public int Level;
    }
    [MessagePackObject, RequireFieldsInit]
    public class TestSetLoadoutRequest : IServerRequest {
        [Key(0)] public Guid UserId;
        [Key(1)] public Quantum.GameSnapshotLoadout Loadout;
    }

    [MessagePackObject, RequireFieldsInit]
    public class TestSetLoadoutResponse : IServerResponse {
        [Key(0)] public bool Success;
    }
}