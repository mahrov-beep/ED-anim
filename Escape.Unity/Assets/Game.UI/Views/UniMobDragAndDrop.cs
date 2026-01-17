namespace Game.UI.Views {
    using System.Collections.Generic;

    internal static class UniMobDragAndDrop {
        private static readonly List<UniMobDropZoneBehaviour> DropZones = new List<UniMobDropZoneBehaviour>();

        public static void RegisterDropZone(UniMobDropZoneBehaviour zone) {
            if (DropZones.Contains(zone)) {
                return;
            }

            DropZones.Add(zone);
        }

        public static void UnregisterDropZone(UniMobDropZoneBehaviour zone) {
            DropZones.Remove(zone);
        }

        public static void NotifyBeginDrag(object payload) {
            foreach (var zone in DropZones) {
                zone.OnDragAndDropBegin(payload);
            }
        }

        public static void NotifyEndDrag(object payload) {
            foreach (var zone in DropZones) {
                zone.OnDragAndDropEnd(payload);
            }
        }
    }
}