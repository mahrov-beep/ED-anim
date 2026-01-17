namespace Game.Shared.DTO {
    using MessagePack;
    using Multicast;
    using Quantum;

    [MessagePackObject, RequireFieldsInit]
    public class ReportGameSnapshotRequest : IServerRequest {
        [Key(0)] public string       GameId;
        [Key(1)] public GameSnapshot GameSnapshot;
    }

    [MessagePackObject, RequireFieldsInit]
    public class ReportGameSnapshotResponse : IServerResponse {
        [Key(0)] public bool ShouldWaitForUserPlayerResults;
    }
}