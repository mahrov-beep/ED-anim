namespace InfimaGames.LowPolyShooterPack {
    using UnityEngine;

    public enum CharacterAudioLayers {
        Unknown = 0,

        [InspectorName("Fire (Выстрелы, броски гранаты)")]
        Fire = 1,

        [InspectorName("Reload (Перезарядка)")]
        Reload = 2,

        [InspectorName("Action (Смена оружия, ближний бой, прицеливание)")]
        Action = 3,

        [InspectorName("Env (Шаги, прыжки)")]
        Env = 4,

        [InspectorName("Voice (Крик при смерти)")]
        Voice = 5,
        
        [InspectorName("Hits (Локальные попадание)")]
        Hits = 6,
    }
}