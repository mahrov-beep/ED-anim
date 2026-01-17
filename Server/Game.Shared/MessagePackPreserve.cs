namespace Game.Shared {
    using System.Collections.Generic;
    using MessagePack;
    using Quantum;

    [MessagePackObject]
    public class MessagePackPreserve {
        [Key(0)] public GameResults             GameResults;
        [Key(1)] public GameSnapshot            GameSnapshot;
        [Key(2)] public Dictionary<string, int> DictStringInt;
    }
}