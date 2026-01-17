namespace Multicast.Balance {
    using UnityEngine;

    public abstract class InternalBalancePage {
        public virtual string Path => this.GetType().Name;

        internal abstract void LoadAndInitialize(bool forceReload);
    }

    public abstract class BalancePage<TGameDef> : InternalBalancePage
        where TGameDef : class {
        public TGameDef Def { get; private set; }

        internal sealed override void LoadAndInitialize(bool forceReload) {
            if (this.Def == null || forceReload) {
                this.Def = this.Load(EditorAddressablesCache<TextAsset>.Instance);
            }

            this.Initialize();
        }

        protected abstract void Initialize();

        protected abstract TGameDef Load(IEnumerableCache<TextAsset> cache);
    }
}