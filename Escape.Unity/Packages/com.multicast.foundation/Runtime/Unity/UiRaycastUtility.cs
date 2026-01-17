namespace Multicast.Unity {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public static class UiRaycastUtility {
        public static bool Raycast(Vector3 mousePosition, out GameObject hitObject, Func<GameObject, bool> filter = null) {
            if (EventSystem.current is var eventSystem && eventSystem != null) {
                var eventData = new PointerEventData(eventSystem) {
                    position = mousePosition,
                };

                var results = new List<RaycastResult>();
                eventSystem.RaycastAll(eventData, results);

                foreach (var result in results) {
                    if (filter != null && !filter.Invoke(result.gameObject)) {
                        continue;
                    }

                    hitObject = result.gameObject;
                    return true;
                }
            }

            hitObject = default;
            return false;
        }
    }
}