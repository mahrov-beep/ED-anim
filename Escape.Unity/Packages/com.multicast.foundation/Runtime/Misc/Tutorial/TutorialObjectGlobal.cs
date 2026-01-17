#if TUTORIAL_MASK

namespace Multicast.Misc.Tutorial {
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using UnityEngine;
    using UnityEngine.Pool;

    public static class TutorialObjectGlobal {
        private static readonly List<TutorialObjectID>                              EnabledObjectIDs     = new();
        private static readonly Dictionary<TutorialObjectID, HashSet<Action<bool>>> RegisteredActivators = new();

        [RuntimeInitializeOnLoadMethod]
        private static void Cleanup() {
            EnabledObjectIDs.Clear();
            RegisteredActivators.Clear();
        }

        public static void Register(TutorialObjectID key, Action<bool> activator) {
            GetOrCreateActivators(key).Add(activator);

            activator.Invoke(IsEnabled(key));
        }

        public static void Unregister(TutorialObjectID key, Action<bool> activator) {
            GetOrCreateActivators(key).Remove(activator);
        }

        [PublicAPI]
        public static void Enable(TutorialObjectID key) {
            SetEnabled(key, true);
        }

        [PublicAPI]
        public static void Disable(TutorialObjectID key) {
            SetEnabled(key, false);
        }

        // Internal

        private static bool IsEnabled(TutorialObjectID key) {
            return EnabledObjectIDs.Contains(key);
        }

        private static void SetEnabled(TutorialObjectID key, bool enabled) {
            if (enabled) {
                EnabledObjectIDs.Add(key);
            }
            else {
                EnabledObjectIDs.Remove(key);
            }

            if (RegisteredActivators.TryGetValue(key, out var list)) {
                using (ListPool<Action<bool>>.Get(out var temp)) {
                    temp.AddRange(list);

                    foreach (var activation in temp) {
                        activation.Invoke(enabled);
                    }
                }
            }
        }

        private static HashSet<Action<bool>> GetOrCreateActivators(TutorialObjectID key) {
            if (!RegisteredActivators.TryGetValue(key, out var list)) {
                RegisteredActivators.Add(key, list = new HashSet<Action<bool>>());
            }

            return list;
        }
    }
}

#endif