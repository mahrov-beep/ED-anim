namespace Game.Shared.DTO {
    using System;
    using MessagePack;
    using Multicast;
    using Quantum;

    [MessagePackObject, RequireFieldsInit]
    public sealed class LoadoutGetByUserRequest : IServerRequest {
        [Key(0)] public Guid UserId;
    }

    [MessagePackObject, RequireFieldsInit]
    public sealed class LoadoutGetByUserResponse : IServerResponse {
        [Key(0)] public GameSnapshotLoadout Loadout;
    }
}


