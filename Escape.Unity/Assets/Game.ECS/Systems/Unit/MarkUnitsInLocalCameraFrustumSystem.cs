namespace Game.ECS.Systems.Unit {
    using Game.ECS.Components.Camera;
    using Game.ECS.Components.Unit;
    using Camera;
    using Multicast;
    using Scellecs.Morpeh;
    using UnityEngine;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    /// <summary>
    /// AABB по всем юнитам, если юнит внутри фрустума - добавляем InLocalCameraRectComponent
    /// </summary>
    public class MarkUnitsInLocalCameraFrustumSystem : SystemBase {
        [Inject] private Stash<InLocalCameraFrustumMarker> inLocalCameraRectStash;

        [Inject] private Stash<CinemachineBrainComponent> brainStash;
        [Inject] private Stash<UnitComponent>             unitStash;

        private Filter unitsFilter;

        private SingletonFilter<CinemachineBrainComponent> brainFilter;

        private readonly Plane[] frustumPlanes = new Plane[6];
        //захардкодил, но вообще можно брать их из квантума с PhysicsCollider3D на каждом персонаже.
        const float CHARACTER_HEIGHT      = 2f;
        const float CHARACTER_HEIGHT_HALF = CHARACTER_HEIGHT * 0.5f;
        const float CHARACTER_RADIUS      = 1f;

        private static readonly Vector3 Size = new(CHARACTER_RADIUS, CHARACTER_HEIGHT, CHARACTER_RADIUS);

        private static Bounds bounds = new(Vector3.zero, Size);

        public override void Dispose() {
            inLocalCameraRectStash.RemoveAll();
        }

        public override void OnAwake() {
            // brainFilter = World.Filter
            //                 .With<CinemachineBrainComponent>()
            //                 .Build();
            brainFilter = World.Filter.Singleton<CinemachineBrainComponent>();

            unitsFilter = World.Filter
                            .With<UnitComponent>()
                            .Without<LocalCharacterMarker>()
                            .Build();
        }

        public override void OnUpdate(float deltaTime) {
            ref var brainComponent = ref brainFilter.Instance;

            var camera = brainComponent.brain.OutputCamera;
            if (ReferenceEquals(null, camera)) {
                return;
            }

            GeometryUtility.CalculateFrustumPlanes(
                            camera.projectionMatrix * camera.worldToCameraMatrix,
                            frustumPlanes);

            foreach (var entity in unitsFilter) {
                ref var unit = ref unitStash.Get(entity);

                var inFrustum = IsObjectVisible(unit.quantumEntityView.Transform);

                if (inFrustum) {
                    inLocalCameraRectStash.Set(entity, new InLocalCameraFrustumMarker());
                }
                else {
                    inLocalCameraRectStash.Remove(entity);
                }
            }
        }

        private bool IsObjectVisible(Transform objectTransform) {
            bounds.center = objectTransform.position + Vector3.up * CHARACTER_HEIGHT_HALF;

            // бенчмаркнул на 5000 кубиках, в 2.5 раза быстрее чем по вьюпорту проверку делать
            // можно еще сильнее ускорить если свой AABB написать
            var isVisibleByFrustum = GeometryUtility.TestPlanesAABB(frustumPlanes, bounds);

            return isVisibleByFrustum;
        }
    }

}