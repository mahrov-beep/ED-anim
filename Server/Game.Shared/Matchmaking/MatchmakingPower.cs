namespace Game.Shared.Matchmaking {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Quantum;

    public static class MatchmakingPower {
        private static readonly IReadOnlyDictionary<CharacterLoadoutSlots, double> SlotWeights = new Dictionary<CharacterLoadoutSlots, double> {
            [CharacterLoadoutSlots.PrimaryWeapon]   = 1.00,
            [CharacterLoadoutSlots.SecondaryWeapon] = 0.60,
            [CharacterLoadoutSlots.MeleeWeapon]     = 0.40,
            [CharacterLoadoutSlots.Armor]           = 0.80,
            [CharacterLoadoutSlots.Helmet]          = 0.50,
            [CharacterLoadoutSlots.Backpack]        = 0.20,
            [CharacterLoadoutSlots.Skill]           = 0.30,
            [CharacterLoadoutSlots.Perk1]           = 0.30,
            [CharacterLoadoutSlots.Perk2]           = 0.30,
            [CharacterLoadoutSlots.Perk3]           = 0.30,
            [CharacterLoadoutSlots.Headphones]      = 0.10,
            
            /// <summary>
            /// Cosmetic-only slot used to maximize social visibility of paying players: when others clearly see premium skins in matches,
            /// they are more likely to want skins too. No gameplay advantage (cosmetic only).
            /// </summary>
            [CharacterLoadoutSlots.Skin]            = 0.20,
        };

        private static readonly IReadOnlyDictionary<ERarityType, double> RarityMultipliers = new Dictionary<ERarityType, double> {
            [ERarityType.Common]    = 1.00,
            [ERarityType.Uncommon]  = 1.10,
            [ERarityType.Rare]      = 1.25,
            [ERarityType.Epic]      = 1.50,
            [ERarityType.Legendary] = 1.80,
        };

        private const double Alpha = 0.6;

        public static int CalculatePlayerPower(IEnumerable<(CharacterLoadoutSlots Slot, int Level, ERarityType Rarity)> equipped) {
            double sum = 0;

            foreach (var x in equipped) {
                if (!SlotWeights.TryGetValue(x.Slot, out var w))
                    continue;

                var r = RarityMultipliers.GetValueOrDefault(x.Rarity, 1.0);

                var level  = Math.Max(0, x.Level);
                var scaled = Math.Pow(level, Alpha);

                sum += w * r * scaled;
            }

            var power   = (int)Math.Round(sum * 10.0, MidpointRounding.AwayFromZero);
            var clamped = Math.Clamp(power, 100, 5000);

            return clamped;
        }

        public static int CalculatePartyPower(IEnumerable<int> memberPowers) {
            var arr = memberPowers.Where(x => x > 0).OrderByDescending(x => x).ToArray();

            if (arr.Length == 0)
                return 0;

            var k      = Math.Max(1, (int)Math.Ceiling(arr.Length * 0.5));
            var avgTop = arr.Take(k).Average();
            var avgAll = arr.Average();
            var agg    = 0.7 * avgTop + 0.3 * avgAll;
            var result = (int)Math.Round(agg, MidpointRounding.AwayFromZero);

            return result;
        }
    }
}


