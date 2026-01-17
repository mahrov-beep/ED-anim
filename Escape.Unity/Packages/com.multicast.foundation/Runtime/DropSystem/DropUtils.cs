namespace Multicast.DropSystem {
    using System.Collections.Generic;
    using JetBrains.Annotations;

    public static class DropUtils {
        [PublicAPI]
        public static Drop Combine(List<Drop> drops, string itemType = "list", string itemKey = "list") {
            return drops.Count == 1
                ? drops[0]
                : Drop.LootBox(itemType, itemKey, drops.ToArray());
        }

        [PublicAPI]
        public static Drop Combine(Drop[] drops, string itemType = "list", string itemKey = "list") {
            return drops.Length == 1
                ? drops[0]
                : Drop.LootBox(itemType, itemKey, drops);
        }

        [PublicAPI]
        public static List<Drop> GetAllDrops(Drop drop) {
            var list = new List<Drop>();
            GetAllDrops(list, drop);
            return list;
        }

        private static void GetAllDrops(List<Drop> list, Drop drop) {
            if (drop.AmountType == DropAmountType.LootBox) {
                foreach (var innerDrop in drop.LootBoxDrops) {
                    GetAllDrops(list, innerDrop);
                }
            }
            else {
                list.Add(drop);
            }
        }
    }
}