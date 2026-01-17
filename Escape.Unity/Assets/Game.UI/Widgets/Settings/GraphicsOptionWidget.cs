namespace Game.UI.Widgets.Settings {
    using System;
    using System.Collections.Generic;
    using Services.Graphics;
    using UniMob;
    using UniMob.UI;

    public interface IGraphicsOptionState : IViewState {
        List<IGraphicsOptionButtonState> Options { get; }
    }

    public interface IGraphicsOptionButtonState : IViewState {
        string Title { get; }

        bool IsSelected { get; }

        bool IsRecommended { get; }

        void Select();
    }

    // Generic widget for a single graphics setting (quality, shadows, etc.)
    public class GraphicsOptionWidget<T> : StatefulWidget {
        public IGraphicsSetting<T> Setting { get; set; }

        public override State CreateState() => new GraphicsOptionState<T>();
    }

    public class GraphicsOptionState<T> : ViewState<GraphicsOptionWidget<T>>, IGraphicsOptionState {
        private readonly List<StateHolder> optionHolders = new();
        private readonly List<IGraphicsOptionButtonState> optionStates = new();

        public override void InitState() {
            base.InitState();
            this.RebuildOptions();
        }

        public List<IGraphicsOptionButtonState> Options => this.optionStates;

        public override WidgetViewReference View => default;

        private void RebuildOptions() {
            this.optionHolders.Clear();

            var setting = this.Widget.Setting;
            var source  = setting?.Options;

            if (source == null || source.Count == 0) {
                this.CacheStates();
                return;
            }

            for (var i = 0; i < source.Count; i++) {
                var option = source[i];
                var holder = this.CreateChild(_ => new GraphicsOptionButtonWidget<T> {
                    Setting = setting,
                    Option  = option,
                    Key     = Key.Of(i),
                });
                this.optionHolders.Add(holder);
            }

            this.CacheStates();
        }

        private void CacheStates() {
            this.optionStates.Clear();
            if (this.optionHolders.Count == 0) {
                return;
            }

            for (var i = 0; i < this.optionHolders.Count; i++) {
                this.optionStates.Add((IGraphicsOptionButtonState)this.optionHolders[i].Value);
            }
        }
    }

    // Single option button (off/low/medium/high etc.)
    public class GraphicsOptionButtonWidget<T> : StatefulWidget {
        public IGraphicsSetting<T> Setting { get; set; }

        public GraphicsOption<T> Option { get; set; }

        public override State CreateState() => new GraphicsOptionButtonState<T>();
    }

    public class GraphicsOptionButtonState<T> : ViewState<GraphicsOptionButtonWidget<T>>, IGraphicsOptionButtonState {
        public override WidgetViewReference View => default;

        public string Title => this.Widget.Option.Name;

        public bool IsSelected {
            get {
                var setting = this.Widget.Setting;
                var current = setting != null ? setting.Current : default;
                return EqualityComparer<T>.Default.Equals(current, this.Widget.Option.Value);
            }
        }

        public bool IsRecommended => this.Widget.Option.IsRecommended;

        public void Select() {
            this.Widget.Setting?.Apply(this.Widget.Option.Value);
        }
    }
}
