namespace Game.ECS.Systems.Unit {
    using UnityEngine;
    internal static class TimerExtensions {
        public static bool ProcessTimer(this ref float timer, float deltaTime) {
            timer = Mathf.Max(0, timer - deltaTime);

            return timer <= float.Epsilon;
        }
    }
}