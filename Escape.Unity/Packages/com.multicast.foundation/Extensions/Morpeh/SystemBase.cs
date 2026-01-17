namespace Scellecs.Morpeh {
    public abstract class SystemBase : ISystem {
        public World World { get; set; }

        public abstract void OnAwake();

        public abstract void OnUpdate(float deltaTime);

        public virtual void Dispose() {
        }
    }
}