namespace Game.Shared.ServerEvents {
    using MessagePack;
    using Multicast;

    [Union(1, typeof(SnapshotGameServerEvent))]
    public interface IGameServerEvent : IServerEvent {
    }

    [MessagePackObject, RequireFieldsInit] public sealed class SnapshotGameServerEvent : IGameServerEvent {
        [Key(0)] public int  SnapshotCount;
        [Key(1)] public int  RequiredSnapshotCount;
    }
}