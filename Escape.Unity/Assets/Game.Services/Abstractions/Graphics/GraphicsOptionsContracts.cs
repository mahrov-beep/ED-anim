namespace Game.Services.Graphics {
    using System.Collections.Generic;

    public readonly struct GraphicsOption<T> {
        public GraphicsOption(T value, string name, bool isRecommended = false) {
            this.Value         = value;
            this.Name          = name;
            this.IsRecommended = isRecommended;
        }

        public T      Value         { get; }
        public string Name          { get; }
        public bool   IsRecommended { get; }

        public override string ToString() {
            return $"{this.Name}:{this.Value}";
        }
    }

    public interface IGraphicsSetting<T> {
        string Key { get; }

        List<GraphicsOption<T>> Options { get; }

        T Current { get; }

        T Recommended { get; }

        void Apply(T value, bool userInitiated = true);
    }
}
