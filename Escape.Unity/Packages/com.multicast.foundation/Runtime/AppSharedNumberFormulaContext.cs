namespace Multicast {
    using System;
    using System.Collections.Generic;
    using CodeWriter.ExpressionParser;
    using JetBrains.Annotations;
    using Numerics;
    using UnityEngine;

    public class AppSharedNumberFormulaContext : ExpressionContext<BigDouble> {
        private readonly List<Func<string, Expression<BigDouble>>> resolvers;

        public AppSharedNumberFormulaContext(AppSharedFormulaContext parent)
            : this(parent, new List<Func<string, Expression<BigDouble>>>()) {
        }

        private AppSharedNumberFormulaContext(AppSharedFormulaContext parent, List<Func<string, Expression<BigDouble>>> resolvers)
            : base(null, CreateCachedResolver(parent, resolvers)) {
            this.resolvers = resolvers;
        }

        [PublicAPI]
        public void RegisterGlobalVariableResolver(Func<string, Expression<BigDouble>> func) {
            this.resolvers.Add(func);
        }

        private static Func<string, Expression<BigDouble>> CreateCachedResolver(AppSharedFormulaContext parent, List<Func<string, Expression<BigDouble>>> resolvers) {
            var cache = new Dictionary<string, Expression<BigDouble>>();

            return variableName => {
                if (cache.TryGetValue(variableName, out var cached)) {
                    return cached;
                }

                foreach (var resolver in resolvers) {
                    var result = resolver.Invoke(variableName);
                    if (result == null) {
                        continue;
                    }

                    cache.Add(variableName, result);
                    return result;
                }

                var parentVariable = parent.GetVariable(variableName, nullIsOk: true);

                if (parentVariable != null) {
                    cached = () => parentVariable.Invoke();
                }
                else {
                    cached = () => {
                        if (Application.isPlaying) {
                            Debug.LogError($"[AppSharedNumberFormulaContext] Variable '{variableName}' not exists");
                        }

                        return 0f;
                    };
                }

                cache.Add(variableName, cached);
                return cached;
            };
        }
    }
}