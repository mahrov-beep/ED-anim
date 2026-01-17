using JetBrains.Annotations;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack {
    public struct WeaponSetup {
        public GameObject WeaponPrefab; // Префаб оружия

        // Обвесы
        [CanBeNull] public GameObject ScopePrefab;    // Прицелы
        [CanBeNull] public GameObject GripPrefab;     // Рукояти
        [CanBeNull] public GameObject MuzzlePrefab;   // Ствол: компенсаторы и пламегасители
        [CanBeNull] public GameObject MagazinePrefab; // Магазин
        [CanBeNull] public GameObject StockPrefab;    // Приклад
        [CanBeNull] public GameObject LaserPrefab;    // Лазеры
    }
}