namespace Multicast {
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    internal struct ModelSafety : IDisposable {
        private static readonly Stack<ModelSafety> Stack = new();

        public ActionType Type;
        public Type       ModelType;

        [RuntimeInitializeOnLoadMethod]
        private static void Setup() {
            Stack.Clear();
        }

        public static bool TryGetAccessException(Model accessedModel, out Exception ex) {
            if (!Stack.TryPeek(out var it)) {
                ex = default;
                return false;
            }

            var name = accessedModel.GetType().Name;

            var message = it.Type switch {
                ActionType.ModelCtor => $"{name} cannot be accessed during execution of ctor of another model ({it.ModelType.Name})",
                ActionType.ModelDispose => $"{name} cannot be accessed during execution of Dispose of another model ({it.ModelType.Name})",
                ActionType.ModulesInstallPhase => $"{name} cannot be accessed during modules install phase",
                _ => "{modelName} cannot be accessed: unknown reason",
            };

            ex = new ModelAccessNotAllowedException(message);
            return true;
        }

        public static ModelSafety EnterModelCtor(Type createdType) {
            var it = new ModelSafety {
                Type      = ActionType.ModelCtor,
                ModelType = createdType,
            };

            Stack.Push(it);

            return it;
        }

        public static ModelSafety EnterModelDispose(Type disposedType) {
            var it = new ModelSafety {
                Type      = ActionType.ModelDispose,
                ModelType = disposedType,
            };

            Stack.Push(it);

            return it;
        }

        public static ModelSafety EnterModulesInstallPhase() {
            var it = new ModelSafety {
                Type = ActionType.ModulesInstallPhase,
            };

            Stack.Push(it);

            return it;
        }

        public void Dispose() {
            Stack.Pop();
        }

        public enum ActionType {
            ModulesInstallPhase,
            ModelCtor,
            ModelDispose,
        }
    }
}