namespace Game.ECS.Systems.Core {
    using Multicast;
    using Scellecs.Morpeh;
    using UniMob;

    public class LifetimeSystem : SystemBase {
        private LifetimeController sceneLifetimeController;

        public Lifetime SceneLifetime {
            get {
                this.sceneLifetimeController ??= App.Lifetime.CreateNested();
                return this.sceneLifetimeController.Lifetime;
            }
        }

        public override void OnAwake() {
        }

        public override void Dispose() {
            base.Dispose();

            this.sceneLifetimeController?.Dispose();
            this.sceneLifetimeController = null;
        }

        public override void OnUpdate(float deltaTime) {
        }
    }
}