namespace Multicast {
    using System;
    using System.Collections.Generic;
    using CodeWriter.ExpressionParser;
    using JetBrains.Annotations;
    using UnityEngine;

    public class AppSharedFormulaContext : ExpressionContext<int> {
        private readonly List<Func<string, Expression<int>>> resolvers;

        public AppSharedFormulaContext()
            : this(new List<Func<string, Expression<int>>>()) {
        }

        private AppSharedFormulaContext(List<Func<string, Expression<int>>> resolvers)
            : base(null, CreateCachedResolver(resolvers)) {
            this.resolvers = resolvers;
        }

        [PublicAPI]
        public void RegisterGlobalVariableResolver(Func<string, Expression<int>> func) {
            this.resolvers.Add(func);
        }

        private static Func<string, Expression<int>> CreateCachedResolver(List<Func<string, Expression<int>>> resolvers) {
            var cache = new Dictionary<string, Expression<int>>();

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

                cached = () => {
                    if (Application.isPlaying) {
                        Debug.LogError($"[AppSharedFormulaContext] Variable '{variableName}' not exists");
                    }

                    return 0;
                };

                cache.Add(variableName, cached);
                return cached;
            };
        }
    }
}