namespace Multicast.GameProperties {
    public readonly struct BoolGamePropertyName {
        public string Name         { get; }
        public bool   DefaultValue { get; }

        public BoolGamePropertyName(string name, bool defaultValue = false) {
            this.Name         = name;
            this.DefaultValue = defaultValue;
        }

        public static implicit operator BoolGamePropertyName(string name) => new(name);
    }

    public readonly struct IntGamePropertyName {
        public string Name         { get; }
        public int    DefaultValue { get; }

        public IntGamePropertyName(string name, int defaultValue = 0) {
            this.Name         = name;
            this.DefaultValue = defaultValue;
        }

        public static implicit operator IntGamePropertyName(string name) => new(name);
    }

    public readonly struct FloatGamePropertyName {
        public string Name         { get; }
        public float  DefaultValue { get; }

        public FloatGamePropertyName(string name, float defaultValue = 0.0f) {
            this.Name         = name;
            this.DefaultValue = defaultValue;
        }

        public static implicit operator FloatGamePropertyName(string name) => new(name);
    }
}