namespace Game.ECS.Utilities {
    using UnityEngine;

    public static class ScreenSpaceHelper {
        private const float EPSILON = 1e-4f;

        public static bool TryGetScreenDirection(
            Vector3 worldDirection, 
            Transform cameraTransform, 
            out Vector2 screenDirection) {
            
            var dirNorm = worldDirection.normalized;
            
            var x = Vector3.Dot(dirNorm, cameraTransform.right);
            var y = Vector3.Dot(dirNorm, cameraTransform.forward);
            
            screenDirection = new Vector2(x, y);
            
            if (screenDirection.sqrMagnitude < EPSILON) {
                screenDirection = Vector2.zero;
                return false;
            }
            
            screenDirection.Normalize();
            return true;
        }
       
        public static bool IsPositionOnScreen(Vector3 screenPos) {
            return screenPos.z > 0 &&
                   screenPos.x > 0 && screenPos.x < Screen.width &&
                   screenPos.y > 0 && screenPos.y < Screen.height;
        }

        public static bool IsWithinDistance(Vector3 from, Vector3 to, float maxDistance) {
            var distanceSqr = (to - from).sqrMagnitude;
            var maxDistanceSqr = maxDistance * maxDistance;
            return distanceSqr <= maxDistanceSqr;
        }

        public static Vector2 CalculateEdgePosition(
            Vector2 screenPos,
            Vector2 screenCenter,
            float halfWidth,
            float halfHeight,
            float margin) {
            
            var dir = (screenPos - screenCenter).normalized;
            
            var maxX = halfWidth - margin;
            var maxY = halfHeight - margin;
            
            float tX = dir.x != 0 ? Mathf.Abs(maxX / dir.x) : float.MaxValue;
            float tY = dir.y != 0 ? Mathf.Abs(maxY / dir.y) : float.MaxValue;
            float t = Mathf.Min(tX, tY);
            
            return new Vector2(dir.x * t, dir.y * t);
        }
    }
}

