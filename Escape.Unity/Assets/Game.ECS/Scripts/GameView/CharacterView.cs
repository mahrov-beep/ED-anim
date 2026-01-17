namespace _Project.Scripts.GameView {
    using System;
    using System.Collections.Generic;
    using Game.Domain.GameProperties;
    using Game.ECS.Components.Camera;
    using Multicast;
    using Multicast.GameProperties;
    using Photon.Deterministic;
    using Quantum;
    using Scellecs.Morpeh;
    using Sirenix.OdinInspector;
    using Unity.Cinemachine;
    using UnityEngine;
    using UnityEngine.Rendering;

    public sealed class CharacterView : QuantumEntityViewComponent<CustomViewContext> {
        [SerializeField, Required] private GameObject localPlayerRoot, remotePlayerRoot;

        [SerializeField, Required] private GameObject thirdPersonSpectatorCamera;
        [SerializeField, Required] private QuantumEntityPrototype quantumPrototype;

        private IDisposable localPlayerAddSubscription;

        private GamePropertiesModel gamePropertiesModel;
        private bool                lastUseTpSpectator;
        private FP                  basePrototypeHeight;
        private FP                  basePrototypeCenterY;
        private float               lastPrototypeRatio = -1f;

        public bool IsLocal { get; private set; }

        public override void OnActivate(Frame f) {
            base.OnActivate(f);

            this.gamePropertiesModel = App.Get<GamePropertiesModel>();

            this.CachePrototypeCollider();
            this.ApplyPrototypeHeight(FP._1, force: true);
            this.RefreshViewIsLocal(f);

            this.localPlayerAddSubscription = QuantumCallback.SubscribeManual(this, (CallbackLocalPlayerAddConfirmed e)
                => this.RefreshViewIsLocal(e.Frame));
        }
        
        public override void OnDeactivate() {
            base.OnDeactivate();

            this.ApplyPrototypeHeight(FP._1, force: true);
            this.localPlayerAddSubscription.Dispose();
        }

        public override void OnUpdateView() {
            base.OnUpdateView();

            var newUseTpSpectator = this.gamePropertiesModel.Get(DebugGameProperties.Booleans.DebugThirdPersonSpectatorMode);
            if (this.lastUseTpSpectator != newUseTpSpectator) {
                this.lastUseTpSpectator = newUseTpSpectator;

                this.RefreshViewIsLocal(this.VerifiedFrame);
            }

            this.UpdatePrototypeHeight(this.PredictedFrame ?? this.VerifiedFrame);
        }

        private void OnDrawGizmos() {
            if (this.EntityRef == EntityRef.None) {
                return;
            }

            if (Camera.current != Camera.main) {
                return;
            }

            if (this.Game.PlayerIsLocal(this.VerifiedFrame.Get<Unit>(this.EntityRef).PlayerRef)) {
                return;
            }

            var shape  = this.VerifiedFrame.Get<PhysicsCollider3D>(this.EntityRef).Shape;
            
            var verifiedPos  = this.VerifiedFrame.Get<Transform3D>(this.EntityRef).Position;

            DrawCapsule(verifiedPos.ToUnityVector3(), Color.green);
            DrawCapsule(this.transform.position, Color.cyan);
  

            void DrawCapsule(Vector3 pos, Color color) {
                var capsule = shape.Capsule;
                GizmoUtils.DrawGizmosCapsule(pos + shape.Centroid.ToUnityVector3(), 
                    capsule.Radius.AsFloat, capsule.Extent.AsFloat, color, style: new QuantumGizmoStyle {
                        DisableFill = false,
                    });
            }
        }

        private void RefreshViewIsLocal(Frame f) {
            var isPlayer = f.TryGet(this.EntityRef, out Unit unit);

            var isLocal = false;
            
            if (isPlayer) {
                if (this.Game.PlayerIsLocal(unit.PlayerRef)) {
                    this.ViewContext.LocalView = this;

                    // Local player is always predicted.
                    this.EntityView.InterpolationMode = QuantumEntityViewInterpolationMode.Prediction;

                    this.name = $"{this.EntityRef} (Player-Local {unit.PlayerRef})";

                    isLocal = true;
                }
                else {
                    // Other player views are snapshot interpolated.
                    this.EntityView.InterpolationMode = QuantumEntityViewInterpolationMode.SnapshotInterpolation;

                    if (f.Has<Bot>(this.EntityRef)) {
                        this.name = $"{this.EntityRef} (Player-Bot {unit.PlayerRef})";
                    }
                    else {
                        this.name = $"{this.EntityRef} (Player-Remote {unit.PlayerRef})";
                    }
                }
            }
            else {
                // Other player views are snapshot interpolated.
                this.EntityView.InterpolationMode = QuantumEntityViewInterpolationMode.SnapshotInterpolation;

                if (f.Has<Bot>(this.EntityRef)) {
                    this.name = $"{this.EntityRef} (NPC-Bot)";
                }
            }

            this.IsLocal = isLocal;
            
            var noView = f.GameMode.rule is GameRules.MainMenuStorage or GameRules.MainMenuGameResults;

            bool useLocalView, useRemoteView, useTpSpectatorCamera;
            if (this.lastUseTpSpectator && isLocal && !noView) {
                useLocalView         = false;
                useRemoteView        = true;
                useTpSpectatorCamera = true;
            }
            else {
                useLocalView         = isLocal && !noView;
                useRemoteView        = !isLocal && !noView;
                useTpSpectatorCamera = false;
            }

            this.localPlayerRoot.SetActive(useLocalView);
            this.remotePlayerRoot.SetActive(useRemoteView);
            this.thirdPersonSpectatorCamera.SetActive(useTpSpectatorCamera);
        }

        void CachePrototypeCollider() {
            if (!this.quantumPrototype) {
                this.quantumPrototype = this.GetComponent<QuantumEntityPrototype>();
            }

            if (!this.quantumPrototype) {
                return;
            }

            var shape = this.quantumPrototype.PhysicsCollider.Shape3D;
            if (shape.ShapeType != Shape3DType.Capsule) {
                return;
            }

            this.basePrototypeHeight  = shape.CapsuleHeight;
            this.basePrototypeCenterY = shape.PositionOffset.Y;
        }

        void UpdatePrototypeHeight(Frame frame) {
            if (frame == null || this.EntityRef == EntityRef.None) {
                return;
            }

            if (this.basePrototypeHeight <= FP._0 || !this.quantumPrototype || this.quantumPrototype.PhysicsCollider.Shape3D.ShapeType != Shape3DType.Capsule) {
                return;
            }

            var ratio = UnitColliderHeightHelper.GetCurrentHeightRatio(frame, this.EntityRef);
            this.ApplyPrototypeHeight(ratio, force: false);
        }

        void ApplyPrototypeHeight(FP ratio, bool force) {
            if (!this.quantumPrototype || this.quantumPrototype.PhysicsCollider.Shape3D.ShapeType != Shape3DType.Capsule || this.basePrototypeHeight <= FP._0) {
                return;
            }

        }
    }
}
