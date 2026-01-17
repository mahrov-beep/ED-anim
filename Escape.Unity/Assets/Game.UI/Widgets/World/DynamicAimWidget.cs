namespace Game.UI.Widgets.World {
    using Domain;
    using Domain.Game;
    using ECS.Scripts;
    using Multicast;
    using Quantum;
    using Services.Photon;
    using UniMob.UI;
    using UnityEngine;
    using Views.World;

    public class DynamicAimWidget : StatefulWidget {
        public DynamicAimUiDynamicData Data { get; }

        public DynamicAimWidget(DynamicAimUiDynamicData data) {
            this.Data = data;
        }
    }

    public class DynamicAimState : ViewState<DynamicAimWidget>, IDynamicAimState {
        [Inject] private PhotonService           photonService;
        [Inject] private GameLocalCharacterModel localCharacterModel;

        public override WidgetViewReference View => UiConstants.Views.World.DynamicAimView;

        public Vector3 TargetAimWorldPos  => Widget.Data.TargetAimWorldPos;
        public Vector3 ForwardAimWorldPos => Widget.Data.ForwardAimWorldPos;

        public float Bullets         => Widget.Data.Bullets;
        public float MaxBullets      => Widget.Data.MaxBullets;
        public float ShootingSpread  => Widget.Data.ShootingSpread;
        public float AimPercent      => Widget.Data.AimPercent;
        public float ItemsQuality    => Widget.Data.Quality;
        public bool  HasTarget       => Widget.Data.HasTarget;

        public bool TargetAimActive {
            get {
                if (this.photonService.PredictedFrame is not { } f) {
                    return false;
                }

                if (f.GameModeAiming is not FirstPersonAimingAsset) {
                    return true;
                }
                
                return !this.localCharacterModel.IsAiming;
            }
        }
        
        public bool NeedToSetAimPosition {
            get {
                if (this.photonService.PredictedFrame is not { } f) {
                    return false;
                }

                return f.GameModeAiming is not FirstPersonAimingAsset;
            }
        }
        
        public bool  IsReloading     => Widget.Data.IsReloading;
        public bool  IsTargetBlocked => Widget.Data.IsTargetBlocked;
        public bool  Deactivated     => Widget.Data.Deactivated;
        public bool  IsHealing       => this.localCharacterModel?.IsHealing ?? false;
        public float HealingProgress => Mathf.Clamp01(this.localCharacterModel?.HealingProgress ?? 0f);
        
        public string ItemsQualityKey {
            get {
                if (this.ItemsQuality > 100 && this.ItemsQuality <= 1000) {
                    return CoreConstants.Game.ItemQualityVisual.ADVANCED;
                }
                
                if (this.ItemsQuality > 1000) {
                    return CoreConstants.Game.ItemQualityVisual.DRAGON_SKIN;
                }

                return CoreConstants.Game.ItemQualityVisual.LIGHT;
            }
        }
    }
}