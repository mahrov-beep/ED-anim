namespace Game.UI.Controllers.Tutorial {
    using System.Collections.Generic;
    using UniMob;
    using UniMob.UI.Widgets;
    using UnityEngine;

    internal static class TutorialStatics {
        [RuntimeInitializeOnLoadMethod]
        private static void CleanupStatics() {
            RoutesAtom.Value.Clear();
        }
        
        private static readonly Atom<List<RouteSettings>> RoutesAtom = Atom.Value(new List<RouteSettings>());

        public static List<RouteSettings> RoutesWithActiveTutorial => RoutesAtom.Value;

        public static void AddActiveRoute(RouteSettings route) {
            RoutesAtom.Value.Add(route);
            RoutesAtom.Invalidate();
        }

        public static void RemoveActiveRoute(RouteSettings route) {
            RoutesAtom.Value.Remove(route);
            RoutesAtom.Invalidate();
        }
    }
}