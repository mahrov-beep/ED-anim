namespace Multicast.SafeArea {
    using UnityEngine;
    using UnityEngine.EventSystems;

    [ExecuteInEditMode]
    [RequireComponent(typeof(RectFilter))]
    public abstract class RectFilterExtender : UIBehaviour {
        private RectFilter filter;

        protected RectFilter Filter => this.filter ? this.filter : (this.filter = this.GetComponent<RectFilter>());

        public abstract void ApplyOffset(SafeAreaOffset offset);
        public abstract bool LockX { get; }
        public abstract bool LockY { get; }

        protected override void OnEnable() {
            base.OnEnable();

            this.Filter.RefreshExtenders();
            this.Filter.UpdateRect();
        }

        protected override void OnDisable() {
            base.OnDisable();

            this.Filter.RefreshExtenders();
            this.Filter.UpdateRect();
        }

#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();

            this.Filter.RefreshExtenders();
            this.Filter.UpdateRect();
        }

        protected override void Reset() {
            base.Reset();

            this.Filter.RefreshExtenders();
            this.Filter.UpdateRect();
        }
#endif
    }
}