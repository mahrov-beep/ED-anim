namespace Game.Domain.GameProperties {
    using Multicast.GameProperties;

    public static class DebugGameProperties {
        public static class Booleans {
            public static readonly BoolGamePropertyName DebugThirdPersonSpectatorMode = "Debug TP Spectator";
            public static readonly BoolGamePropertyName EnableAimAssist = new("Enable Aim Assist", true);
            public static readonly BoolGamePropertyName DebugAimAssist = "Debug Aim Assist";
        }
    }
}