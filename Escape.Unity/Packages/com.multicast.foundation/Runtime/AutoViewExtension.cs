namespace Multicast {
    using System;
    using CodeWriter.ViewBinding;
    using JetBrains.Annotations;
    using Numerics;
    using UniMob.UI;

    public static class AutoViewExtension {
        private static AutoViewVariableBinding Variable<T, TVariable>(
            ViewContextBase view, string name, Func<T> func, T defaultValue, TVariable variable, AutoViewSyncMode sync)
            where TVariable : ViewVariable<T, TVariable> {
            variable.SetName(name);
            variable.SetContext(view);
#if UNITY_EDITOR
            variable.SetValueEditorOnly(defaultValue);
#endif
            return new AutoViewVariableBinding<T, TVariable>(variable, func, sync);
        }

        private static AutoViewEventBinding Event<T, TEvent>(
            ViewContextBase view, string name, Action<T> call, TEvent evt)
            where TEvent : ViewParametrizedEvent<T, TEvent> {
            evt.SetName(name);
            evt.SetContext(view);
            return new AutoViewEventBinding<T, TEvent>(evt, call);
        }

        [PublicAPI]
        public static AutoViewVariableBinding Variable<T>(this AutoView<T> view, string name, Func<float> func, 
            float defaultValue = default, AutoViewSyncMode sync = AutoViewSyncMode.Auto)
            where T : class, IViewState {
            return Variable(view, name, func, defaultValue, new ViewVariableFloat(), sync);
        }

        [PublicAPI]
        public static AutoViewVariableBinding Variable<T>(this AutoView<T> view, string name, Func<string> func, 
            string defaultValue = default, AutoViewSyncMode sync = AutoViewSyncMode.Auto)
            where T : class, IViewState {
            return Variable(view, name, func, defaultValue, new ViewVariableString(), sync);
        }
        
        [PublicAPI]
        public static AutoViewVariableBinding Variable<T, TEnum>(this AutoView<T> view, string name, Func<string> func, 
            TEnum defaultValue, AutoViewSyncMode sync = AutoViewSyncMode.Auto)
            where T : class, IViewState
            where TEnum : Enum {
            var result = Variable(view, name, func, defaultValue.ToString(), new ViewVariableString(), sync);
#if UNITY_EDITOR
            if (result is AutoViewVariableBinding<string, ViewVariableString> typedResult) {
                typedResult.EditorPopupValues = Enum.GetNames(typeof(TEnum));
            }
#endif
            return result;
        }

        [PublicAPI]
        public static AutoViewVariableBinding Variable<T>(this AutoView<T> view, string name, Func<bool> func, 
        bool defaultValue = default, AutoViewSyncMode sync = AutoViewSyncMode.Auto)
            where T : class, IViewState {
            return Variable(view, name, func, defaultValue, new ViewVariableBool(), sync);
        }

        [PublicAPI]
        public static AutoViewVariableBinding Variable<T>(this AutoView<T> view, string name, Func<int> func, 
            int defaultValue = default, AutoViewSyncMode sync = AutoViewSyncMode.Auto)
            where T : class, IViewState {
            return Variable(view, name, func, defaultValue, new ViewVariableInt(), sync);
        }

        [PublicAPI]
        public static AutoViewVariableBinding Variable<T>(this AutoView<T> view, string name, Func<BigDouble> func, 
            BigDouble defaultValue = default, AutoViewSyncMode sync = AutoViewSyncMode.Auto)
            where T : class, IViewState {
            return Variable(view, name, func, defaultValue, new ViewVariableBigDouble(), sync);
        }

        [PublicAPI]
        public static AutoViewVariableBinding Variable<T>(this AutoView<T> view, string name, Func<Cost> func, 
            Cost defaultValue = null, AutoViewSyncMode sync = AutoViewSyncMode.Auto)
            where T : class, IViewState {
            return Variable(view, name, func, defaultValue ?? Cost.Empty, new ViewVariableCost(), sync);
        }

        [PublicAPI]
        public static AutoViewEventBinding EventInvoker<T>(this AutoView<T> view, string name, Action<Action> call)
            where T : class, IViewState {
            var evt = new ViewEventVoid();
            evt.SetName(name);
            evt.SetContext(view);
            return new AutoViewEventInvokerVoid(evt, call);
        }

        [PublicAPI]
        public static AutoViewEventBinding Event<T>(this AutoView<T> view, string name, Action call)
            where T : class, IViewState {
            var evt = new ViewEventVoid();
            evt.SetName(name);
            evt.SetContext(view);
            return new AutoViewEventBindingVoid(evt, call);
        }

        [PublicAPI]
        public static AutoViewEventBinding Event<T>(this AutoView<T> view, string name, Action<float> call)
            where T : class, IViewState {
            return Event(view, name, call, new ViewEventFloat());
        }

        [PublicAPI]
        public static AutoViewEventBinding Event<T>(this AutoView<T> view, string name, Action<string> call)
            where T : class, IViewState {
            return Event(view, name, call, new ViewEventString());
        }

        [PublicAPI]
        public static AutoViewEventBinding Event<T>(this AutoView<T> view, string name, Action<bool> call)
            where T : class, IViewState {
            return Event(view, name, call, new ViewEventBool());
        }

        [PublicAPI]
        public static AutoViewEventBinding Event<T>(this AutoView<T> view, string name, Action<int> call)
            where T : class, IViewState {
            return Event(view, name, call, new ViewEventInt());
        }
    }

    public enum AutoViewSyncMode {
        Auto,
        Once,
    }
}