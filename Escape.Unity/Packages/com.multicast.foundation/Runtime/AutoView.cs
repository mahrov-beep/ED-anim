namespace Multicast {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CodeWriter.ViewBinding;
    using JetBrains.Annotations;
    using Sirenix.OdinInspector;
    using UniMob;
    using UniMob.Core;
    using UniMob.UI;
    using UnityEngine;
    using UnityEngine.Profiling;
#if UNITY_EDITOR
    using Sirenix.Utilities.Editor;
    using UnityEditor;

#endif

    [RequireComponent(typeof(RectTransform))]
    public abstract partial class AutoView<TState> : AutoView, IView, IViewTreeElement
        where TState : class, IViewState {
        [SerializeField, ReadOnly, PropertySpace(0, 15)]
        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = false, DraggableItems = false, ShowPaging = false)]
        private List<ViewBindingBehaviour> listeners = new List<ViewBindingBehaviour>();

#if UNITY_EDITOR
        private bool listenersIsMissingError;

        [OnInspectorInit]
        private void OnInspectorInit() {
            this.VariablesView = this.GetViewVariableBindings();
            this.EventsView    = this.GetViewEventBindings();

            this.RevalidateListeners();
        }

        [Button(ButtonSizes.Large), PropertyOrder(-1), GUIColor(1.0f, 0.65f, 0.5f), ShowIf(nameof(listenersIsMissingError))]
        [InfoBox("Some listeners is missing", InfoMessageType.Error, visibleIfMemberName: nameof(listenersIsMissingError))]
        private void RefreshListeners() {
            this.listeners.Clear();
            this.listeners.AddRange(this.SearchListeners());

            this.RevalidateListeners();
        }

        private void ApplyVariablesNextFrame() {
            EditorApplication.delayCall += this.ApplyVariables;
        }

        private void ApplyVariables() {
            var applyMethod = typeof(ApplicatorBase).GetMethod("Apply", BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var it in this.listeners) {
                if (it is ApplicatorBase) {
                    applyMethod?.Invoke(it, null);
                }
            }
        }

        private IEnumerable<ViewBindingBehaviour> SearchListeners() {
            foreach (var it in this.gameObject.GetComponentsInChildren<ViewBindingBehaviour>(true)) {
                if (it == null || it == this || it.GetComponentInParent<AutoView>(true) != this) {
                    continue;
                }

                yield return it;
            }
        }

        private void RevalidateListeners() {
            if (Application.isPlaying) {
                return;
            }

            this.listenersIsMissingError = !this.listeners.SequenceEqual(this.SearchListeners());
        }

        [Title("Programmers", "Real data that is set from the script", TitleAlignment = TitleAlignments.Centered, HorizontalLine = true, Bold = true)]
        [ShowInInspector, LabelText("Variables")]
        [ListDrawerSettings(ShowFoldout = false, ShowPaging = false, DraggableItems = false, HideAddButton = true, HideRemoveButton = true, OnTitleBarGUI = nameof(DrawVariablesTitleBar))]
        [OnValueChanged(nameof(ApplyVariablesNextFrame), includeChildren: true)]
        private AutoViewVariableBinding[] VariablesView { get; set; }

        [ShowInInspector, LabelText("Events")]
        [ListDrawerSettings(ShowFoldout = false, ShowPaging = false, DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        [OnValueChanged(nameof(ApplyVariablesNextFrame), includeChildren: true)]
        private AutoViewEventBinding[] EventsView { get; set; }

        private void DrawVariablesTitleBar() {
            if (GUILayout.Button("Reset To Defaults", SirenixGUIStyles.ToolbarButton, GUILayout.Width(120))) {
                this.viewVariableBindings = null;
                this.viewEventBindings    = null;

                this.VariablesView = this.GetViewVariableBindings();
                this.EventsView    = this.GetViewEventBindings();

                this.RevalidateListeners();
                this.ApplyVariables();
            }

            if (GUILayout.Button("Apply", SirenixGUIStyles.ToolbarButton, GUILayout.Width(50))) {
                this.RevalidateListeners();
                this.ApplyVariables();
            }

            GUIHelper.PushColor(Color.gray);
            GUILayout.Label("Sync", GUILayout.Width(36));
            GUIHelper.PopColor();
        }
#endif

        [NotNull] private readonly ViewRenderScope        renderScope            = new ViewRenderScope();
        [NotNull] private readonly List<IViewTreeElement> children               = new List<IViewTreeElement>();
        private readonly           LifetimeController     viewLifetimeController = new LifetimeController();

        private TState currentState;
        private TState nextState;

        private Atom<TState> doRebind;
        private Atom<object> doRender;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private CustomSampler renderSampler;
#endif
        public    bool   HasState => this.currentState != null;
        protected TState State    => this.currentState;

        // ReSharper disable once InconsistentNaming
        public RectTransform rectTransform => (RectTransform) this.transform;

        bool IView.IsDestroyed => this == null;

        private AutoViewVariableBinding[] viewVariableBindings;
        private AutoViewEventBinding[]    viewEventBindings;

        private AutoViewVariableBinding[] GetViewVariableBindings() {
            return this.viewVariableBindings ??= this.Variables;
        }

        private AutoViewEventBinding[] GetViewEventBindings() {
            return this.viewEventBindings ??= this.Events;
        }

        protected virtual AutoViewVariableBinding[] Variables => Array.Empty<AutoViewVariableBinding>();
        protected virtual AutoViewEventBinding[]    Events    => Array.Empty<AutoViewEventBinding>();

        protected sealed override int VariablesCount => this.GetViewVariableBindings().Length + this.designerVariables.Count;
        protected sealed override int EventCount     => this.GetViewEventBindings().Length + this.designerEvents.Count;

        protected sealed override ViewVariable GetVariable(int index) {
            if (index < this.designerVariables.Count) {
                return this.designerVariables[index];
            }

            index -= this.designerVariables.Count;

            return this.GetViewVariableBindings()[index].Variable;
        }

        protected sealed override ViewEvent GetEvent(int index) {
            if (index < this.designerEvents.Count) {
                return this.designerEvents[index];
            }

            index -= this.designerEvents.Count;
            
            return this.GetViewEventBindings()[index].Evt;
        }

        public Lifetime ViewLifetime => this.viewLifetimeController.Lifetime;

        [PublicAPI]
        public void Render(TState state, bool link = false) {
            var self = (IView) this;
            self.SetSource(state, link);
        }

        void IView.SetSource(IViewState newSource, bool link) {
            if (this.doRebind == null) {
                this.doRebind = Atom.Computed(this.ViewLifetime, this.DoRebind, keepAlive: true);
                this.doRender = Atom.Computed(this.ViewLifetime, this.DoRender, keepAlive: true);

                using (Atom.NoWatch) {
                    foreach (var listener in this.listeners) {
                        ViewBindingBehaviour.Setup(listener, this.ViewLifetime);
                    }
                }
            }

            this.renderScope.Link(this);

            var doRebindAtom = ((AtomBase) this.doRebind);

            if (!ReferenceEquals(newSource, this.currentState)) {
                if (newSource is TState typedState) {
                    this.nextState = typedState;
                }
                else {
                    var expected = typeof(TState).Name;
                    var actual   = newSource.GetType().Name;
                    Debug.LogError($"Wrong model type at '{this.name}': expected={expected}, actual={actual}");
                    return;
                }

                doRebindAtom.Actualize(true);
            }

            if (link) {
                this.doRebind.Get();
            }
            else {
                doRebindAtom.Actualize();
            }
        }

        private void Unmount() {
            using (Atom.NoWatch) {
                this.doRebind?.Deactivate();
                this.doRender?.Deactivate();

                if (this.currentState != null) {
                    try {
                        this.currentState.DidViewUnmount(this);
                    }
                    catch (Exception ex) {
                        Debug.LogException(ex);
                    }

                    try {
                        this.Deactivate();
                    }
                    catch (Exception ex) {
                        Debug.LogException(ex);
                    }
                }
            }

            foreach (var child in this.children) {
                child.Unmount();
            }

            this.nextState    = null;
            this.currentState = null;
        }

        void IView.ResetSource() {
            this.Unmount();
        }

        void IViewTreeElement.AddChild(IViewTreeElement view) {
            this.children.Add(view);
        }

        void IViewTreeElement.Unmount() {
            this.Unmount();
        }

        private TState DoRebind() {
            if (ReferenceEquals(this.currentState, this.nextState)) {
                this.doRender.Get();
                return this.nextState;
            }

            using (Atom.NoWatch) {
                if (this.currentState != null) {
                    try {
                        this.currentState.DidViewUnmount(this);
                    }
                    catch (Exception ex) {
                        Debug.LogException(ex);
                    }

                    try {
                        this.Deactivate();
                    }
                    catch (Exception ex) {
                        Debug.LogException(ex);
                    }
                }

                this.currentState = this.nextState;

                try {
                    this.Activate();
                }
                catch (Exception ex) {
                    Debug.LogException(ex);
                }
            }

            ((AtomBase) this.doRender).Actualize(true);
            this.doRender.Get();

            using (Atom.NoWatch) {
                try {
                    this.currentState.DidViewMount(this);
                }
                catch (Exception ex) {
                    Debug.LogException(ex);
                }
            }

            return this.nextState;
        }

        private object DoRender() {
            if (this.currentState == null) {
                return null;
            }

            using (this.renderScope.Enter(this)) {
                this.children.Clear();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (this.renderSampler == null) {
                    this.renderSampler = CustomSampler.Create($"Render {this.name}");
                }

                this.renderSampler.Begin();
#endif

                try {
                    this.Render();
                }
                catch (Exception ex) {
                    Debug.LogException(ex);
                }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                this.renderSampler.End();
#endif
            }

            return null;
        }

        protected virtual void Awake() {
        }

        protected virtual void OnDestroy() {
            this.viewLifetimeController.Dispose();
        }

        protected virtual void Activate() {
            foreach (var binding in this.GetViewVariableBindings()) {
                binding.Bind();
            }

            foreach (var binding in this.GetViewEventBindings()) {
                binding.Bind();
            }

            foreach (var viewBindingBehaviour in this.listeners) {
                if (viewBindingBehaviour is IAutoViewListener listener) {
                    listener.Activate();
                }
            }
        }

        protected virtual void Deactivate() {
            foreach (var viewBindingBehaviour in this.listeners) {
                if (viewBindingBehaviour is IAutoViewListener listener) {
                    listener.Deactivate();
                }
            }

            foreach (var binding in this.GetViewEventBindings()) {
                binding.Unbind();
            }

            foreach (var binding in this.GetViewVariableBindings()) {
                binding.Unbind();
            }
        }

        protected virtual void Render() {
            foreach (var listener in this.listeners) {
                if (listener == null) {
                    continue;
                }

                ViewBindingBehaviour.LinkToRender(listener);
            }
        }

        public sealed override void ForceSyncVariable(ViewVariable viewVariable) {
            foreach (var binding in this.GetViewVariableBindings()) {
                if (binding.Variable.IsRootVariableFor(viewVariable)) {
                    binding.ForceSync();
                }
            }
        }
    }

    public abstract class AutoView : ViewContextBase {
        public abstract void ForceSyncVariable(ViewVariable viewVariable);
    }

    public interface IAutoViewListener {
        void Activate();
        void Deactivate();
    }
}