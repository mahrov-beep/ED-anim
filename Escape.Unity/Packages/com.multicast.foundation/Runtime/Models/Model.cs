namespace Multicast {
    using System.Diagnostics;
    using UniMob;

    public abstract class Model : ILifetimeScope {
        public Lifetime Lifetime { get; }

        protected Model(Lifetime lifetime) {
            this.Lifetime = lifetime;
        }

        public virtual void Initialize() {
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        [Conditional("MULTICAST_MODEL_SAFETY_CHECKS")]
        protected void EnsureAccessAllowed() {
            if (ModelSafety.TryGetAccessException(this, out var ex)) {
                throw ex;
            }
        }
    }

    public interface IModelWithUserDataConfigurator {
        void ConfigureUserData();
    }
}