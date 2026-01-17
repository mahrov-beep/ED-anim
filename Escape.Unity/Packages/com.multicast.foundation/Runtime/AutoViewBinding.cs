namespace Multicast {
    using System;
    using CodeWriter.ViewBinding;
    using Sirenix.OdinInspector;
    using UniMob;
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
    using Sirenix.Utilities.Editor;
#endif

    [HideReferenceObjectPicker]
    public abstract class AutoViewVariableBinding {
        public abstract ViewVariable Variable { get; }

        public abstract void Bind();
        public abstract void Unbind();
        public abstract void ForceSync();
    }

    public class AutoViewVariableBinding<T, TVariable> : AutoViewVariableBinding
        where TVariable : ViewVariable<T, TVariable> {
        private readonly TVariable        variable;
        private readonly Func<T>          func;
        private readonly AutoViewSyncMode syncMode;

        private Atom<T> atom;

        [field: NonSerialized] internal T[] EditorPopupValues { get; set; }

        public AutoViewVariableBinding(TVariable variable, Func<T> func, AutoViewSyncMode syncMode) {
            this.variable = variable;
            this.func     = func;
            this.syncMode = syncMode;
        }

#if UNITY_EDITOR
        [ShowIf("@EditorPopupValues == null")]
        [ShowInInspector, HorizontalGroup, LabelText("@variable.Name"), DisableInPlayMode]
        private T InspectorValue {
            get => this.variable.Value;
            set => this.variable.SetValueEditorOnly(value);
        }

        [ShowIf("@EditorPopupValues != null")]
        [ValueDropdown(nameof(EditorPopupValues))]
        [ShowInInspector, HorizontalGroup, LabelText("@variable.Name"), DisableInPlayMode]
        private T DropdownInspectorValue {
            get => this.variable.Value;
            set => this.variable.SetValueEditorOnly(value);
        }

        [ShowInInspector, HorizontalGroup(120), HideLabel, DisplayAsString]
        private string TypeDisplayName => this.variable.TypeDisplayName;

        [OnInspectorGUI, HorizontalGroup(40)]
        private void InspectorSyncLabel() {
            var color = this.syncMode == AutoViewSyncMode.Once ? new Color(1f, 0.4f, 0f) : Color.white;
            var label = this.syncMode == AutoViewSyncMode.Once ? "ONCE" : "AUTO";

            GUIHelper.PushColor(color);
            GUILayout.Label(label, EditorStyles.helpBox, GUILayout.Width(40), GUILayout.Height(18));
            GUIHelper.PopColor();
        }

#endif
        public override ViewVariable Variable => this.variable;

        public sealed override void Bind() {
            if (this.syncMode == AutoViewSyncMode.Once) {
                this.atom ??= Atom.Computed(Lifetime.Eternal, () => {
                    using (Atom.NoWatch) {
                        return this.func.Invoke();
                    }
                });
            }
            else {
                this.atom ??= Atom.Computed(Lifetime.Eternal, this.func);
            }

            this.atom.Invalidate();
            this.variable.SetSource(this.atom);
        }

        public sealed override void Unbind() {
            this.variable.SetSource(null);
            this.atom.Deactivate();
        }

        public sealed override void ForceSync() {
            this.atom.Invalidate();
        }
    }

    [HideReferenceObjectPicker]
    public abstract class AutoViewEventBinding {
        public abstract ViewEvent Evt { get; }

        public abstract void Bind();
        public abstract void Unbind();
    }

    public class AutoViewEventBinding<T, TEvent> : AutoViewEventBinding
        where TEvent : ViewParametrizedEvent<T, TEvent> {
        private readonly TEvent    evt;
        private readonly Action<T> call;

        public AutoViewEventBinding(TEvent evt, Action<T> call) {
            this.evt  = evt;
            this.call = call;
        }

#if UNITY_EDITOR
        [ShowInInspector, LabelText("@evt.Name"), EnableGUI, DisplayAsString]
        private string InspectorValue => this.evt.TypeDisplayName;
#endif

        public override ViewEvent Evt => this.evt;

        public override void Bind()   => this.evt.AddListener(this.call);
        public override void Unbind() => this.evt.RemoveListener(this.call);
    }

    public class AutoViewEventInvokerVoid : AutoViewEventBinding {
        private readonly ViewEventVoid  evt;
        private readonly Action<Action> callback;

        public AutoViewEventInvokerVoid(ViewEventVoid evt, Action<Action> callback) {
            this.evt      = evt;
            this.callback = callback;
        }

#if UNITY_EDITOR
        [ShowInInspector, LabelText("@evt.Name"), EnableGUI, DisplayAsString]
        [InlineButton(nameof(InspectorInvoke), "Invoke")]
        private string InspectorValue => this.evt.TypeDisplayName;

        private void InspectorInvoke() {
            this.evt.Invoke();
        }
#endif

        public override ViewEvent Evt => this.evt;

        public override void Bind() {
            this.callback.Invoke(this.evt.Invoke);
        }

        public override void Unbind() {
            this.callback.Invoke(null);
        }
    }

    public class AutoViewEventBindingVoid : AutoViewEventBinding {
        private readonly ViewEventVoid evt;
        private readonly Action        call;

        public AutoViewEventBindingVoid(ViewEventVoid evt, Action call) {
            this.evt  = evt;
            this.call = call;
        }

#if UNITY_EDITOR
        [ShowInInspector, LabelText("@evt.Name"), EnableGUI, DisplayAsString]
        [InlineButton(nameof(InspectorInvoke), "Invoke")]
        private string InspectorValue => this.evt.TypeDisplayName;

        private void InspectorInvoke() {
            this.evt.Invoke();
        }
#endif

        public override ViewEvent Evt => this.evt;

        public override void Bind()   => this.evt.AddListener(this.call);
        public override void Unbind() => this.evt.RemoveListener(this.call);
    }
}