namespace Multicast.DirtyDataEditor {
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Serialization;

    public static class DirtyDataTypeActivator {
        private static readonly Dictionary<Type, Func<object>> Activators = new Dictionary<Type, Func<object>>();
        private static readonly Dictionary<Type, ConstructorInfo> Constructors = new Dictionary<Type, ConstructorInfo>();

        public static void Register<T>(Func<T> activator) {
            Activators[typeof(T)] = () => activator.Invoke();
        }

        public static object CreateInstance(Type type) {
            return Activators.TryGetValue(type, out var activator)
                ? activator.Invoke()
                : CreateInstanceSlow(type);
        }

        private static object CreateInstanceSlow(Type type) {
            return CreateInstanceIl2Cpp(type);
        }

        private static object CreateInstanceIl2Cpp(Type type) {
            if (!Constructors.TryGetValue(type, out var constructor)) {
                constructor = Constructors[type] = type.IsValueType
                    ? null
                    : type.GetConstructor(Type.EmptyTypes);
            }

            var instance = FormatterServices.GetUninitializedObject(type);
            constructor?.Invoke(instance, Array.Empty<object>());
            return instance;
        }
    }
}