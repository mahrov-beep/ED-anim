namespace Quantum {
    public static class NickNameGenerator {
        private static readonly string[] DefaultNames = {
            "Rookie",
            "Hawk",
            "CadetAlpha",
            "Trooper",
            "Bravo",
            "Steel",
            "ReconUnit",
            "DeltaGhost",
            "GreenTag",
            "LoadoutZero",
            "Scout",
            "Echo",
            "Gunner",
            "BootCamp",
            "SilentLine",
            "Patrol",
            "TaskUnit",
            "WatchPost",
        };

        public static string Generate(int nameIndex, int counter) {
            return DefaultNames[nameIndex % DefaultNames.Length] + (125 + (counter % 850));
        }
    }
}