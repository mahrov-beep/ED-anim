namespace Multicast.Install {
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    internal class ServicesContainer {
        private readonly Dictionary<Type, object> instances = new();
        private readonly List<Func<Type, object>> builders  = new();

        internal void RegisterBuilder(Func<Type, object> builder) {
            this.builders.Add(builder);
        }

        internal void Register(Type type, [NotNull] object service) {
            if (service == null) {
                throw new ArgumentNullException(nameof(service));
            }

            if (this.instances.ContainsKey(type)) {
                throw new ServiceAlreadyRegisteredException(type);
            }

            this.instances.Add(type, service);
        }

        internal object Get(Type type) {
            if (this.instances.TryGetValue(type, out var untypedInstance)) {
                return untypedInstance;
            }

            foreach (var builder in this.builders) {
                var service = builder.Invoke(type);
                if (service == null) {
                    continue;
                }

                this.instances.Add(type, service);
                return service;
            }

            throw new ServiceNotRegisteredException(type);
        }

        internal IEnumerable<T> OfType<T>() {
            foreach (var kvp in this.instances) {
                if (kvp.Value is T typed) {
                    yield return typed;
                }
            }
        }
    }

    internal class ServiceNotRegisteredException : Exception {
        public ServiceNotRegisteredException(Type serviceType) : base($"Service of type '{serviceType}' not registered") {
        }
    }

    internal class ServiceAlreadyRegisteredException : Exception {
        public ServiceAlreadyRegisteredException(Type serviceType) : base($"Service of type '{serviceType}' already registered") {
        }
    }
}