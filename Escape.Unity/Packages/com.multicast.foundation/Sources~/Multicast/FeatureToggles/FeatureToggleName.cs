namespace Multicast.FeatureToggles {
    using JetBrains.Annotations;

    [PublicAPI]
    public readonly struct FeatureToggleName {
        public string Name { get; }

        public FeatureToggleName(string name) => this.Name = name;

        public static implicit operator FeatureToggleName(string name) => new FeatureToggleName(name);
    }
}