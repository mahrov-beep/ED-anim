namespace Game.UI.Widgets.Game.Hud {
    using System.Collections.Generic;
    using _Project.Scripts.Minimap;
    using Domain.Game;
    using ECS.Systems.Core;
    using ECS.Systems.Player;
    using JetBrains.Annotations;
    using Multicast;
    using Quantum;
    using Services.Photon;
    using UniMob.UI;
    using UnityEngine;
    using Views.Game.Hud;

    public class MapWidget : StatefulWidget { }

    public class MapState : ViewState<MapWidget>, IMapState {
        [Inject] PhotonService           photonService;
        [Inject] LocalPlayerSystem       localPlayerSystem;
        [Inject] QuantumEntityViewSystem updater;

        [Inject] private MapModel mapModel;

        public override WidgetViewReference View => UiConstants.Views.HUD.Map;

        public Vector2Int LevelSize => new Vector2Int(F.Map.GridSizeX, F.Map.GridSizeY) * F.Map.GridNodeSize;

        public float VisiblyRadius => mapModel.VisiblyRadius;

        public Vector3 Origin     => HasLocalView ? LocalView!.Transform.position : default;
        public float   YawDegrees => HasLocalView ? LocalView!.Transform.rotation.eulerAngles.y : default;

        public Sprite MapSprite => F.GameMode.MapSprite;

        public List<Vector3> Waypoints => mapModel.Waypoints;

        public List<UnitOnMapData> Enemies => mapModel.Enemies;

        public List<UnitOnMapData> PartyMembers => mapModel.PartyMembers;

        public List<Vector3> ExitPoints => mapModel.ExitPoints;

        public List<Vector3> SpawnedItemBoxes {
            get {
                markersBuffer.Clear();
                return markersBuffer;

                // if (!Local.HasValue) {
                //     return markersBuffer;
                // }
                //
                // var sqrVisiblyRadius = VisiblyRadius * VisiblyRadius;
                // var filter           = F.Filter<ItemBox, Transform3D>(withoutDropFromUnitMarker);
                // while (filter.Next(out _, out _, out var box)) {
                //     var sqrDistTo = TransformHelper.DistanceSquared(box, LocalTransform).AsFloat;
                //     if (sqrDistTo > sqrVisiblyRadius) {
                //         continue;
                //     }
                //
                //     markersBuffer.Add(box.Position.ToUnityVector3());
                // }
                //
                // return markersBuffer;
            }
        }

        public List<Vector3> DroppedItemBoxes {
            get {
                markersBuffer.Clear();
                return markersBuffer;

                // if (!Local.HasValue) {
                //     return markersBuffer;
                // }
                //
                // var sqrVisiblyRadius = VisiblyRadius * VisiblyRadius;
                // var filter           = F.Filter<DropFromUnitMarker, ItemBox, Transform3D>();
                // while (filter.Next(out _, out _, out _, out var box)) {
                //     var sqrDistTo = TransformHelper.DistanceSquared(box, LocalTransform).AsFloat;
                //     if (sqrDistTo > sqrVisiblyRadius) {
                //         continue;
                //     }
                //
                //     markersBuffer.Add(box.Position.ToUnityVector3());
                // }
                //
                // return markersBuffer;
            }
        }

        public List<Vector3> InterestPositions => mapModel.InterestPoints;

        public List<Vector3> Grenades => mapModel.Grenades;

        Frame       F              => photonService.PredictedFrame;
        EntityRef?  Local          => localPlayerSystem.LocalRef;
        Transform3D LocalTransform => F.Get<Transform3D>(Local!.Value);
        [CanBeNull] QuantumEntityView LocalView {
            get {
                if (!Local.HasValue) {
                    return null;
                }

                if (!updater.TryGetEntityView(Local.Value, out var view)) {
                    return null;
                }

                return view;
            }
        }

        List<Vector3> markersBuffer = new(12);

        bool HasLocalView => Local.HasValue && LocalView;

        readonly ComponentSet withoutDropFromUnitMarker = ComponentSet.Create<DropFromUnitMarker>();
        readonly ComponentSet withoutDead               = ComponentSet.Create<CharacterStateDead>();
    }
}