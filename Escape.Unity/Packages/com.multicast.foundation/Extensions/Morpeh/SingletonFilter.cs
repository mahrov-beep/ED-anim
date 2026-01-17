namespace Scellecs.Morpeh {
    using System;
    using System.Diagnostics;
    using JetBrains.Annotations;

    public class SingletonFilter {
        private readonly Filter filter;

        public SingletonFilter(Filter filter) {
            this.filter = filter;
        }

        [PublicAPI]
        public bool IsValid => !this.filter.IsEmpty();

        [PublicAPI]
        [NotNull]
        protected Entity Entity {
            get {
                AssertFilterContainsExactOneEntity(this.filter);
                // ReSharper disable once AssignNullToNotNullAttribute
                return this.filter.First();
            }
        }

        [PublicAPI]
        public bool TryGetEntity(out Entity entity) {
            if (this.IsValid) {
                entity = this.Entity;
                return true;
            }

            entity = default;
            return false;
        }

        [AssertionMethod]
        [Conditional("MORPEH_DEBUG")]
        [Conditional("UNITY_EDITOR")]
        private static void AssertFilterContainsExactOneEntity(Filter filter) {
            if (filter.IsEmpty()) {
                throw new InvalidOperationException($"Failed to get singleton: no entity found");
            }
        }
    }

    public class SingletonFilter<TComponent> : SingletonFilter where TComponent : struct, ISingletonComponent {
        private readonly Stash<TComponent> componentStash;
        
        public SingletonFilter(Filter filter) : base(filter) {
            this.componentStash = filter.world.GetStash<TComponent>();
        }

        [PublicAPI]
        public ref TComponent Instance => ref this.componentStash.Get(this.Entity);
    }

    public static class MorpehExtensions {
        [PublicAPI]
        public static SingletonFilter<T> FilterSingleton<T>(this World world) where T : struct, ISingletonComponent {
            return new SingletonFilter<T>(world.Filter.With<T>().Build());
        }

        [PublicAPI]
        public static SingletonFilter<T> Singleton<T>(this FilterBuilder filter) where T : struct, ISingletonComponent {
            return new SingletonFilter<T>(filter.With<T>().Build());
        }

        [PublicAPI]
        public static SingletonFilter Singleton(this FilterBuilder filter) {
            return new SingletonFilter(filter.Build());
        }
    }
}