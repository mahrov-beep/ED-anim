namespace Game.Shared.DTO {
    using System;
    using MessagePack;
    using Multicast;
    using Quantum;

    [MessagePackObject, RequireFieldsInit]
    public class ReportGameQuestCounterTaskRequest : IServerRequest {
        [Key(0)] public string                    GameId;
        [Key(1)] public QuestCounterPropertyTypes Property;
        [Key(2)] public QuestTaskFilters[]        Filters;
        [Key(3)] public Guid[]                    TargetUserIds;
        [Key(4)] public int                       CounterValue;
    }

    [MessagePackObject, RequireFieldsInit]
    public class ReportGameQuestCounterTaskResponse : IServerResponse {
    }
}