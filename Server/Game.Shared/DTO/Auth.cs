namespace Game.Shared.DTO {
    using System;
    using MessagePack;
    using Multicast;

    [MessagePackObject, RequireFieldsInit]
    public class GuestAuthRequest : IServerRequest {
        [Key(0)] public string DeviceId;
    }

    [MessagePackObject, RequireFieldsInit]
    public class GuestAuthResponse : IServerResponse {
        [Key(0)] public Guid   UserId;
        [Key(1)] public string AccessToken;
        [Key(2)] public bool   IsNewUser;
    }
}