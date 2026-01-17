namespace Game.ECS.Systems.Core {
    using Components.Camera;
    using Multicast;
    using Player;
    using Quantum;
    using Scellecs.Morpeh;
    using Services.Photon;
    using UnityEngine;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    public class QuantumPredictionCullingSystem : SystemBase {
        [Inject] private PhotonService           photonService;
        [Inject] private LocalPlayerSystem       localPlayerSystem;
        [Inject] private QuantumEntityViewSystem quantumEntityViewSystem;

        private SingletonFilter<CinemachineBrainComponent> cameraFilter;

        public override void OnAwake() {
            this.cameraFilter = this.World.Filter.Singleton<CinemachineBrainComponent>();
        }

        public override void OnUpdate(float deltaTime) {
            if (this.photonService.Runner?.Game is not { } game) {
                return;
            }

            if (this.photonService.PredictedFrame is not { } f) {
                return;
            }

            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            if (!this.quantumEntityViewSystem.TryGetEntityView(localRef, out var localCharacterView)) {
                return;
            }

            if (!this.cameraFilter.IsValid) {
                return;
            }

            var cam    = this.cameraFilter.Instance.camera;
            var ground = new Plane(Vector3.up, 0);

            var p1 = ViewportToGroundPosition(cam, ground, new Vector2(0, 0));
            var p2 = ViewportToGroundPosition(cam, ground, new Vector2(0, 1));
            var p3 = ViewportToGroundPosition(cam, ground, new Vector2(1, 0));
            var p4 = ViewportToGroundPosition(cam, ground, new Vector2(1, 1));

            Vector3 center;
            float   radius;

            if (f.GameModeAiming.UseTopDownQuantumPredictionCulling) {
                center = CircleCenter(p1, p2, p3);
                radius = Vector3.Distance(center, p1);
            }
            else {
                center = localCharacterView.transform.position;
                radius = 40f; // не ставить меньше чем дальность оружия
                //radius = Mathf.Max(radius, Vector3.Distance(center, p1));
                //radius = Mathf.Max(radius, Vector3.Distance(center, p2));
                //radius = Mathf.Max(radius, Vector3.Distance(center, p3));
                //radius = Mathf.Max(radius, Vector3.Distance(center, p4));
            }

            game.SetPredictionArea(center.ToFPVector3(), radius.ToFP());
        }

        private static Vector3 ViewportToGroundPosition(Camera cam, Plane groundPlane, Vector2 viewport) {
            var ray = cam.ViewportPointToRay(new Vector3(viewport.x, viewport.y, 0));
            return GetHitPoint(ray, groundPlane);
        }

        private static Vector3 GetHitPoint(Ray ray, Plane plane) {
            plane.Raycast(ray, out var distance);
            return ray.GetPoint(distance);
        }

        public static Vector3 CircleCenter(Vector3 aP0, Vector3 aP1, Vector3 aP2) {
            var v1 = aP1 - aP0;
            var v2 = aP2 - aP0;
            var n  = Vector3.Cross(v1, v2).normalized;
            var p1 = Vector3.Cross(v1, n).normalized;
            var p2 = Vector3.Cross(v2, n).normalized;
            var r  = (v1 - v2) * 0.5f;
            var c  = Vector3.Angle(p1, p2);
            var a  = Vector3.Angle(r, p1);
            var d  = r.magnitude * Mathf.Sin(a * Mathf.Deg2Rad) / Mathf.Sin(c * Mathf.Deg2Rad);
            return Vector3.Dot(v1, aP2 - aP1) > 0 ? aP0 + v2 * 0.5f - p2 * d : aP0 + v2 * 0.5f + p2 * d;
        }
    }
}