namespace Game.UI.Widgets.Settings {
    using System;
    using System.Collections.Generic;
    using ECS.Systems.Input;
    using Services.Graphics;
    using UniMob;
    using UniMob.UI;
    using UniMob.UI.Widgets;
    using Views.Settings;
    using Multicast;
    using UnityEngine;

    [RequireFieldsInit]
    public class GraphicsSettingsWidget : StatefulWidget {
        public Action OnClose { get; set; }
    }

    public class GraphicsSettingsState : ViewState<GraphicsSettingsWidget>, IGraphicsSettingsState {
        [Inject] private GraphicsSettingsService graphicsSettingsService;
        [Inject] private PlayerInputConfig       playerInputConfig;

        private StateHolder qualityHolder;
        private StateHolder shadowsHolder;
        private StateHolder texturesHolder;
        private StateHolder lightingHolder;
        private readonly List<IGraphicsOptionButtonState> emptyOptions = new();

        public override void InitState() {
            base.InitState();

            this.qualityHolder = this.CreateChild(_ => new GraphicsOptionWidget<int> {
                Setting = this.graphicsSettingsService.QualitySetting,
                Key     = Key.Of("quality"),
            });
            this.shadowsHolder = this.CreateChild(_ => new GraphicsOptionWidget<GraphicsShadowQuality> {
                Setting = this.graphicsSettingsService.ShadowsSetting,
                Key     = Key.Of("shadows"),
            });
            this.texturesHolder = this.CreateChild(_ => new GraphicsOptionWidget<GraphicsTextureQuality> {
                Setting = this.graphicsSettingsService.TexturesSetting,
                Key     = Key.Of("textures"),
            });
            this.lightingHolder = this.CreateChild(_ => new GraphicsOptionWidget<GraphicsLightingMode> {
                Setting = this.graphicsSettingsService.LightingSetting,
                Key     = Key.Of("lighting"),
            });
        }

        [Atom] public List<IGraphicsOptionButtonState> Presets   => this.ResolveOptions(this.qualityHolder);
        [Atom] public List<IGraphicsOptionButtonState> Shadows   => this.ResolveOptions(this.shadowsHolder);
        [Atom] public List<IGraphicsOptionButtonState> Textures  => this.ResolveOptions(this.texturesHolder);
        [Atom] public List<IGraphicsOptionButtonState> Lighting  => this.ResolveOptions(this.lightingHolder);

        [Atom]
        public float MouseSensitivity {
            get => PlayerPrefs.GetFloat("Settings_MouseSensitivity", this.playerInputConfig.MouseSensitivityDefault);
            set => PlayerPrefs.SetFloat("Settings_MouseSensitivity", value);
        }

        public bool  IsMouseSensitivityVisible {
            get {
#if UNITY_STANDALONE || UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        public float MouseSensitivityMin => this.playerInputConfig.MouseSensitivityMin;
        public float MouseSensitivityMax => this.playerInputConfig.MouseSensitivityMax;

        public void SetMouseSensitivity(float value) {
            this.MouseSensitivity = value;
        }

        public override WidgetViewReference View => UiConstants.Views.Settings.Graphics;

        public void Close() {
            this.Widget.OnClose?.Invoke();
        }

        private List<IGraphicsOptionButtonState> ResolveOptions(StateHolder holder) {
            if (holder?.Value is IGraphicsOptionState optionState) {
                return optionState.Options;
            }

            return this.emptyOptions;
        }
    }
}
