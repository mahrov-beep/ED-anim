namespace Game.Shared.DTO {
    using System;
    using MessagePack;
    using Multicast;
    [MessagePackObject, RequireFieldsInit]
    public class ChangeNickNameRequest : IServerRequest {
        [Key(0)] public string newNick;
    }

    [MessagePackObject, RequireFieldsInit]
    public class ChangeNickNameResponse : IServerResponse {
        [Key(0)] public bool success;
    }
}