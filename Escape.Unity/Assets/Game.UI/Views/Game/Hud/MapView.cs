namespace _Project.Scripts.Minimap {
    using System.Collections.Generic;
    using Game.Domain.Game;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI;
    using UnityEngine;
    using UnityEngine.Serialization;
    using UnityEngine.UI;

    public interface IMapState : IViewState {
        Sprite MapSprite { get; }

        List<Vector3>       Waypoints         { get; }
        List<UnitOnMapData> Enemies           { get; }
        List<UnitOnMapData> PartyMembers      { get; }
        List<Vector3>       InterestPositions { get; }
        List<Vector3>       ExitPoints        { get; }
        List<Vector3>       SpawnedItemBoxes  { get; }
        List<Vector3>       DroppedItemBoxes  { get; }
        List<Vector3>       Grenades          { get; }

        /// Center of the map in world space.
        Vector3 Origin { get; }
        Vector2Int LevelSize { get; }
        /// <summary>
        /// Visible radius in world units, to be translated to the minimap's rect.
        /// </summary>
        float VisiblyRadius { get; }
        float YawDegrees { get; }
    }

    public class MapView : AutoView<IMapState> {
#if MAP_PROFILE
        private bool enableProfiling = false;
#endif

        [InlineEditor]
        [SerializeField]
        private Image enemyIconPrefab;
        [InlineEditor]
        [SerializeField]
        private Image interestIconPrefab;
        [FormerlySerializedAs("pointerPrefab")]
        [InlineEditor]
        [SerializeField]
        private Image interestPointerPrefab;
        [InlineEditor]
        [SerializeField]
        private Image exitIconPrefab, exitPointerPrefab;
        [InlineEditor]
        [SerializeField]
        private Image itemBoxIconPrefab, droppedItemBoxIconPrefab;
        [InlineEditor]
        [SerializeField]
        private Image grenadeIconPrefab;

        [Title("Hierarchy")]
        [SerializeField]
        private RectTransform mapContainer;
        [SerializeField] private RectTransform minimapRect;
        [SerializeField] private RectTransform markersRoot;
        [SerializeField] private RectTransform playerArrow;

        [Space]
        [SerializeField]
        private Image map;

        private int levelHeight, levelWidth;

        private List<Image> activeIcons = new(100);

        private Color enemyBaseColor;

        private readonly struct MarkerState {
            public readonly List<Vector3> Positions;
            public readonly List<UnitOnMapData> Units;
            public readonly int Count;
            public readonly int Hash;

            public MarkerState(List<Vector3> positions) {
                Positions = positions;
                Units = null;
                Count = positions?.Count ?? 0;
                Hash = ComputeHash(positions);
            }

            public MarkerState(List<UnitOnMapData> units) {
                Positions = null;
                Units = units;
                Count = units?.Count ?? 0;
                Hash = ComputeHash(units);
            }

            private static int ComputeHash(List<Vector3> positions) {
                if (positions == null || positions.Count == 0) return 0;
                int hash = positions.Count * 397;
                int sampleCount = System.Math.Min(3, positions.Count);
                for (int i = 0; i < sampleCount; i++) {
                    hash = hash * 31 + positions[i].GetHashCode();
                }
                return hash;
            }

            private static int ComputeHash(List<UnitOnMapData> units) {
                if (units == null || units.Count == 0) return 0;
                int hash = units.Count * 397;
                int sampleCount = System.Math.Min(3, units.Count);
                for (int i = 0; i < sampleCount; i++) {
                    hash = hash * 31 + units[i].WorldPosition.GetHashCode();
                    hash = hash * 31 + units[i].alpha.GetHashCode();
                }
                return hash;
            }

            public bool HasChanged(MarkerState other) {
                return Count != other.Count || Hash != other.Hash;
            }
        }

#if MAP_PROFILE
        private MarkerState lastEnemyState;
        private MarkerState lastPartyState;
        private MarkerState lastWaypointState;
        private MarkerState lastExitState;
        private MarkerState lastInterestState;
        private MarkerState lastSpawnedBoxState;
        private MarkerState lastDroppedBoxState;
        private MarkerState lastGrenadeState;
#endif

        private HashSet<Image> markerUsedThisFrame = new(100);

        [SerializeField]         private Image wayLinePrefab;
        [Min(1)][SerializeField] private float dotStep  = 8f;
        [Min(1)][SerializeField] private float dotScale = 4f;

        /*[SerializeField] private Mesh     dotMesh;
        [SerializeField] private Material dotMaterial;



        private static readonly List<Matrix4x4> DotBatch = new List<Matrix4x4>(64);*/

        private bool RotateMap => false;

        // protected override AutoViewEventBinding[] Events => new[] {
        //                 this.Event("openMap", () => { }),
        //                 this.Event("closeMap", () => { }),
        // };

        private Dictionary<Sprite, Queue<Image>> pool;

        private void Start() {
#if MAP_PROFILE
            MapViewProfiler.Enabled = enableProfiling;
#endif

            /*if (dotMesh == null) {
                dotMesh = new Mesh { name = "MinimapDot" };
                dotMesh.vertices = new[] {
                                new Vector3(-0.5f, -0.5f, 0),
                                new Vector3(-0.5f, 0.5f, 0),
                                new Vector3(0.5f, 0.5f, 0),
                                new Vector3(0.5f, -0.5f, 0)
                };
                dotMesh.uv        = new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };
                dotMesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
                dotMesh.bounds    = new Bounds(Vector3.zero, Vector3.one); // безопасный AABB
            }

            if (dotMaterial == null) {
                var shader = Shader.Find("Unlit/Transparent");
                dotMaterial = new Material(shader) {
                                hideFlags        = HideFlags.HideAndDontSave,
                                mainTexture      = Texture2D.whiteTexture,
                                color            = Color.white,
                                enableInstancing = true
                };
            }*/

            pool = new Dictionary<Sprite, Queue<Image>>();
            var prefabs = new[] {
                            enemyIconPrefab, interestIconPrefab, interestPointerPrefab,
                            exitIconPrefab, exitPointerPrefab,
                            itemBoxIconPrefab, droppedItemBoxIconPrefab,
                            wayLinePrefab,
            };

            foreach (var p in prefabs) {
                pool[p.sprite] = new Queue<Image>(5);

                for (var i = 0; i < 5; i++) {
                    GetIcon(p);
                }
            }

            ClearAllMarkers();

            levelHeight = State.LevelSize.y;
            levelWidth  = State.LevelSize.x;

            map.sprite = State.MapSprite ? State.MapSprite : map.sprite;

            enemyBaseColor = enemyIconPrefab ? enemyIconPrefab.color : Color.white;
        }

        private void OnDestroy() {
#if MAP_PROFILE
            if (Application.isPlaying) {
                MapViewProfiler.ExportToCSV();
            }
#endif
        }

        private static readonly Vector2 OffScreenPosition = new Vector2(-10000f, -10000f);

        private Image GetIcon(Image prefab) {
            var   q = pool[prefab.sprite];
            Image icon;
            if (q.Count > 0) {
                icon = q.Dequeue();
            }
            else {
                icon = Instantiate(prefab, markersRoot);
                icon.transform.SetParent(markersRoot, false);
            }
            activeIcons.Add(icon);
            markerUsedThisFrame.Add(icon);
            return icon;
        }

        private void LateUpdate() {
            if (State == null) {
                return;
            }


#if MAP_PROFILE
            MapViewProfiler.ResetCounters();

            var totalSw = System.Diagnostics.Stopwatch.StartNew();
            var sw = System.Diagnostics.Stopwatch.StartNew();
#endif


            var frameData = new MapFrameData(State, mapContainer, minimapRect, levelWidth, levelHeight, RotateMap);

#if MAP_PROFILE
            sw.Restart();
#endif
            UpdateMapZoom(in frameData);
#if MAP_PROFILE
            var zoomTime = sw.Elapsed.TotalMilliseconds;
            MapViewProfiler.UpdateAverage(ref MapViewProfiler.UpdateZoomAvg, zoomTime);
            MapViewProfiler.RecordMetric(8, zoomTime);

            sw.Restart();
#endif
            UpdateMapPosition(in frameData);
#if MAP_PROFILE
            var posTime = sw.Elapsed.TotalMilliseconds;
            MapViewProfiler.UpdateAverage(ref MapViewProfiler.UpdatePositionAvg, posTime);
            MapViewProfiler.RecordMetric(9, posTime);

            sw.Restart();
#endif
            UpdateMapRotation(in frameData);
#if MAP_PROFILE
            var rotTime = sw.Elapsed.TotalMilliseconds;
            MapViewProfiler.UpdateAverage(ref MapViewProfiler.UpdateRotationAvg, rotTime);
            MapViewProfiler.RecordMetric(10, rotTime);
#endif

            markerUsedThisFrame.Clear();

#if MAP_PROFILE
            sw.Restart();
            var currentWaypointState = new MarkerState(State.Waypoints);
            var waypointsChanged = currentWaypointState.HasChanged(lastWaypointState);
            if (waypointsChanged) {
                ClearMarkersOfType(wayLinePrefab.sprite);
            }
#endif
            DrawWaypointPath(in frameData);
#if MAP_PROFILE
            lastWaypointState = currentWaypointState;
            var waypointTime = sw.Elapsed.TotalMilliseconds;
            MapViewProfiler.UpdateAverage(ref MapViewProfiler.DrawWaypointAvg, waypointTime);
            MapViewProfiler.RecordMetric(1, waypointTime);

            sw.Restart();
            var currentExitState = new MarkerState(State.ExitPoints);
            var exitsChanged = currentExitState.HasChanged(lastExitState);
            if (exitsChanged) {
                ClearMarkersOfType(exitIconPrefab.sprite);
                ClearMarkersOfType(exitPointerPrefab.sprite);
            }
#endif
            DrawExitMarkers(in frameData);
#if MAP_PROFILE
            lastExitState = currentExitState;
            var exitTime = sw.Elapsed.TotalMilliseconds;
            MapViewProfiler.UpdateAverage(ref MapViewProfiler.DrawExitAvg, exitTime);
            MapViewProfiler.RecordMetric(2, exitTime);

            sw.Restart();
            var currentInterestState = new MarkerState(State.InterestPositions);
            var interestsChanged = currentInterestState.HasChanged(lastInterestState);
            if (interestsChanged) {
                ClearMarkersOfType(interestIconPrefab.sprite);
                ClearMarkersOfType(interestPointerPrefab.sprite);
            }
#endif
            DrawInterestMarkers(in frameData);
#if MAP_PROFILE
            lastInterestState = currentInterestState;
            var interestTime = sw.Elapsed.TotalMilliseconds;
            MapViewProfiler.UpdateAverage(ref MapViewProfiler.DrawInterestAvg, interestTime);
            MapViewProfiler.RecordMetric(3, interestTime);

            sw.Restart();
            var currentSpawnedBoxState = new MarkerState(State.SpawnedItemBoxes);
            var currentDroppedBoxState = new MarkerState(State.DroppedItemBoxes);
            var boxesChanged = currentSpawnedBoxState.HasChanged(lastSpawnedBoxState) ||
                               currentDroppedBoxState.HasChanged(lastDroppedBoxState);
            if (boxesChanged) {
                ClearMarkersOfType(itemBoxIconPrefab.sprite);
                ClearMarkersOfType(droppedItemBoxIconPrefab.sprite);
            }
#endif
            DrawItemBoxMarkers(in frameData);
#if MAP_PROFILE
            lastSpawnedBoxState = currentSpawnedBoxState;
            lastDroppedBoxState = currentDroppedBoxState;
            var itemBoxTime = sw.Elapsed.TotalMilliseconds;
            MapViewProfiler.UpdateAverage(ref MapViewProfiler.DrawItemBoxAvg, itemBoxTime);
            MapViewProfiler.RecordMetric(4, itemBoxTime);

            sw.Restart();
            var currentEnemyState = new MarkerState(State.Enemies);
#endif
            DrawEnemyMarkers(in frameData);
#if MAP_PROFILE
            lastEnemyState = currentEnemyState;
            var enemyTime = sw.Elapsed.TotalMilliseconds;
            MapViewProfiler.UpdateAverage(ref MapViewProfiler.DrawEnemyAvg, enemyTime);
            MapViewProfiler.RecordMetric(5, enemyTime);

            sw.Restart();
            var currentPartyState = new MarkerState(State.PartyMembers);
#endif
            DrawPartyMarkers(in frameData);
#if MAP_PROFILE
            lastPartyState = currentPartyState;
            var partyTime = sw.Elapsed.TotalMilliseconds;
            MapViewProfiler.UpdateAverage(ref MapViewProfiler.DrawPartyAvg, partyTime);
            MapViewProfiler.RecordMetric(6, partyTime);

            sw.Restart();
            var currentGrenadeState = new MarkerState(State.Grenades);
            var grenadesChanged = currentGrenadeState.HasChanged(lastGrenadeState);
            if (grenadesChanged && grenadeIconPrefab != null) {
                ClearMarkersOfType(grenadeIconPrefab.sprite);
            }
#endif
            DrawGrenadeMarkers(in frameData);
#if MAP_PROFILE
            lastGrenadeState = currentGrenadeState;
            var grenadeTime = sw.Elapsed.TotalMilliseconds;
            MapViewProfiler.UpdateAverage(ref MapViewProfiler.DrawGrenadeAvg, grenadeTime);
            MapViewProfiler.RecordMetric(7, grenadeTime);

            sw.Restart();
#endif
            ClearUnusedMarkers();
#if MAP_PROFILE
            MapViewProfiler.ActiveIconsCount = activeIcons.Count;
            var clearTime = sw.Elapsed.TotalMilliseconds;
            MapViewProfiler.UpdateAverage(ref MapViewProfiler.ClearMarkersAvg, clearTime);
            MapViewProfiler.RecordMetric(0, clearTime);

            totalSw.Stop();
            var totalTime = totalSw.Elapsed.TotalMilliseconds;
            MapViewProfiler.UpdateAverage(ref MapViewProfiler.TotalFrameAvg, totalTime);
            MapViewProfiler.RecordMetric(11, totalTime);

            MapViewProfiler.IncrementFrame();
            MapViewProfiler.LogStats();
#endif
        }

        void DrawWaypointPath(in MapFrameData f) {
            var wp = State.Waypoints;
            if (wp.Count == 0) {
                return;
            }

            var prevWorld = f.Origin;
            for (var i = 0; i < wp.Count; i++) {
                var nextWorld = wp[i];

                var a = f.RotateVector(f.GetWorldToMapDelta(prevWorld, levelWidth, levelHeight));
                var b = f.RotateVector(f.GetWorldToMapDelta(nextWorld, levelWidth, levelHeight));
                if (a.magnitude > f.MaskRadius && b.magnitude > f.MaskRadius) {
                    prevWorld = nextWorld;
                    continue;
                }

                var dots     = Mathf.CeilToInt(Vector2.Distance(a, b) / dotStep);
                var firstDot = i == 0 ? 0 : 1;
                var lastDot  = i == wp.Count - 1 ? dots : dots - 1;

                for (var d = firstDot; d <= lastDot; d++) {
                    var p = Vector2.Lerp(a, b, d / (float)dots);
                    if (p.magnitude > f.MaskRadius) {
                        return;
                    }

                    var icon = GetIcon(wayLinePrefab);
                    icon.rectTransform.anchoredPosition = p;
                    icon.rectTransform.localScale       = Vector3.one * (dotScale / 10f);
#if MAP_PROFILE
                    MapViewProfiler.WaypointDotsCount++;
#endif
                }

                prevWorld = nextWorld;
            }
        }

        private void UpdateMapZoom(in MapFrameData f) {
            minimapRect.localScale = Vector3.one * f.Scale;
        }

        private void UpdateMapPosition(in MapFrameData f) {
            var desiredPos = new Vector2(-f.PlayerLocalPosition.x, -f.PlayerLocalPosition.y);
            minimapRect.anchoredPosition = desiredPos;
            playerArrow.anchoredPosition = Vector2.zero;
        }

        private void UpdateMapRotation(in MapFrameData f) {
            if (f.RotateMap) {
                mapContainer.localRotation = Quaternion.Euler(0f, 0f, f.YawDegrees);
                playerArrow.localRotation  = Quaternion.identity;
            }
            else {
                mapContainer.localRotation = Quaternion.identity;
                playerArrow.localRotation  = Quaternion.Euler(0f, 0f, -f.YawDegrees);
            }
        }

        private void ClearAllMarkers() {
            foreach (var icon in activeIcons) {
                icon.rectTransform.anchoredPosition = OffScreenPosition;
                pool[icon.sprite].Enqueue(icon);
            }
            activeIcons.Clear();
        }

        private void ClearMarkersOfType(Sprite sprite) {
            if (sprite == null) return;
            for (int i = activeIcons.Count - 1; i >= 0; i--) {
                var icon = activeIcons[i];
                if (icon.sprite == sprite) {
                    icon.rectTransform.anchoredPosition = OffScreenPosition;
                    pool[icon.sprite].Enqueue(icon);

                    int lastIndex = activeIcons.Count - 1;
                    if (i != lastIndex) {
                        activeIcons[i] = activeIcons[lastIndex];
                    }
                    activeIcons.RemoveAt(lastIndex);
                }
            }
        }

        private void ClearUnusedMarkers() {
            for (int i = activeIcons.Count - 1; i >= 0; i--) {
                var icon = activeIcons[i];
                if (!markerUsedThisFrame.Contains(icon)) {
                    icon.rectTransform.anchoredPosition = OffScreenPosition;
                    pool[icon.sprite].Enqueue(icon);

                    int lastIndex = activeIcons.Count - 1;
                    if (i != lastIndex) {
                        activeIcons[i] = activeIcons[lastIndex];
                    }
                    activeIcons.RemoveAt(lastIndex);
                }
            }
        }

        private void DrawExitMarkers(in MapFrameData f) {
            for (var i = 0; i < State.ExitPoints.Count; i++) {
                var worldPos     = State.ExitPoints[i];
                var delta        = f.GetWorldToMapDelta(worldPos, levelWidth, levelHeight);
                var rotatedDelta = f.RotateVector(delta);

                var isOnMap = delta.sqrMagnitude <= f.MaskRadiusSqr;
                var prefab  = isOnMap ? exitIconPrefab : exitPointerPrefab;
                var icon    = GetIcon(prefab);

                if (isOnMap) {
                    icon.rectTransform.anchoredPosition = rotatedDelta;
                    icon.rectTransform.rotation         = Quaternion.identity;
#if MAP_PROFILE
                    MapViewProfiler.ExitIconCount++;
#endif
                }
                else {
                    icon.rectTransform.anchoredPosition = rotatedDelta.normalized * f.MaskRadius;
                    var angle = Mathf.Atan2(rotatedDelta.y, rotatedDelta.x) * Mathf.Rad2Deg;
                    icon.rectTransform.rotation = Quaternion.Euler(0f, 0f, angle);
#if MAP_PROFILE
                    MapViewProfiler.ExitPointerCount++;
#endif
                }
            }
        }

        private void DrawInterestMarkers(in MapFrameData f) {
            var positions = State.InterestPositions;
            for (var i = 0; i < positions.Count; i++) {
                var pos          = positions[i];
                var delta        = f.GetWorldToMapDelta(pos, levelWidth, levelHeight);
                var rotatedDelta = f.RotateVector(delta);

                var isOnMap = delta.sqrMagnitude <= f.MaskRadiusSqr;
                var prefab = isOnMap ? interestIconPrefab : interestPointerPrefab;
                var icon   = GetIcon(prefab);

                if (isOnMap) {
                    icon.rectTransform.anchoredPosition = rotatedDelta;
                    icon.rectTransform.rotation         = Quaternion.identity;
#if MAP_PROFILE
                    MapViewProfiler.InterestIconCount++;
#endif
                }
                else {
                    var pointerDelta = f.RotateMap ? rotatedDelta : f.RotateVector(delta);
                    icon.rectTransform.anchoredPosition = pointerDelta.normalized * f.MaskRadius;
                    var angle = Mathf.Atan2(pointerDelta.y, pointerDelta.x) * Mathf.Rad2Deg;
                    icon.rectTransform.rotation = Quaternion.Euler(0f, 0f, angle);
#if MAP_PROFILE
                    MapViewProfiler.InterestPointerCount++;
#endif
                }
            }
        }

        private void DrawItemBoxMarkers(in MapFrameData f) {
#if MAP_PROFILE
            DrawItemBoxList(State.SpawnedItemBoxes, itemBoxIconPrefab, in f, ref MapViewProfiler.SpawnedItemBoxCount);
            DrawItemBoxList(State.DroppedItemBoxes, droppedItemBoxIconPrefab, in f, ref MapViewProfiler.DroppedItemBoxCount);
#else
            int dummy = 0;
            DrawItemBoxList(State.SpawnedItemBoxes, itemBoxIconPrefab, in f, ref dummy);
            DrawItemBoxList(State.DroppedItemBoxes, droppedItemBoxIconPrefab, in f, ref dummy);
#endif
        }

        private void DrawItemBoxList(List<Vector3> positions, Image prefab, in MapFrameData f, ref int counter) {
            for (var i = 0; i < positions.Count; i++) {
                var pos = positions[i];
                if (!f.IsInVisibleRadius(pos)) {
                    continue;
                }

                var delta        = f.GetWorldToMapDelta(pos, levelWidth, levelHeight);
                var rotatedDelta = f.RotateVector(delta);

                var marker = GetIcon(prefab);
                marker.rectTransform.anchoredPosition = rotatedDelta;
#if MAP_PROFILE
                counter++;
#endif
            }
        }

        private void DrawEnemyMarkers(in MapFrameData f) {
            var unitsOnMap = State.Enemies;
            foreach (var unitOnMap in unitsOnMap) {
                var position = unitOnMap.WorldPosition;

                if (!f.IsInVisibleRadius(position)) {
                    continue;
                }

                var delta        = f.GetWorldToMapDelta(position, levelWidth, levelHeight);
                var rotatedDelta = f.RotateVector(delta);

                var icon = GetIcon(enemyIconPrefab);
                icon.rectTransform.anchoredPosition = rotatedDelta;

                var color = enemyBaseColor;
                color.a = unitOnMap.alpha;
                icon.color = color;
#if MAP_PROFILE
                MapViewProfiler.EnemyCount++;
#endif
            }
        }

        private void DrawPartyMarkers(in MapFrameData f) {
            var unitsOnMap = State.PartyMembers;
            foreach (var unitOnMap in unitsOnMap) {
                var position = unitOnMap.WorldPosition;

                if (!f.IsInVisibleRadius(position)) {
                    continue;
                }

                var delta        = f.GetWorldToMapDelta(position, levelWidth, levelHeight);
                var rotatedDelta = f.RotateVector(delta);

                var icon = GetIcon(enemyIconPrefab);
                icon.rectTransform.anchoredPosition = rotatedDelta;

                var color = Color.green;
                color.a = unitOnMap.alpha;
                icon.color = color;
#if MAP_PROFILE
                MapViewProfiler.PartyCount++;
#endif
            }
        }

        private void DrawGrenadeMarkers(in MapFrameData f) {
            if (this.grenadeIconPrefab is null) {
                return;
            }

            var grenades = State.Grenades;
            for (var i = 0; i < grenades.Count; i++) {
                var position = grenades[i];

                if (!f.IsInVisibleRadius(position)) {
                    continue;
                }

                var delta        = f.GetWorldToMapDelta(position, levelWidth, levelHeight);
                var rotatedDelta = f.RotateVector(delta);

                var icon = GetIcon(grenadeIconPrefab);
                icon.rectTransform.anchoredPosition = rotatedDelta;
#if MAP_PROFILE
                MapViewProfiler.GrenadeCount++;
#endif
            }
        }
    }

    internal readonly struct MapFrameData {
        public readonly Vector3 Origin;
        public readonly Vector2 PlayerNormalizedPosition;
        public readonly Vector2 PlayerLocalPosition;
        public readonly float   YawDegrees;
        public readonly float   VisiblyRadius;
        public readonly float   Scale;
        public readonly float   YawRadians;
        public readonly float   YawCos;
        public readonly float   YawSin;
        public readonly float   MaskRadius;
        public readonly float   MaskRadiusSqr;
        public readonly float   HalfLevelWidth;
        public readonly float   HalfLevelHeight;
        public readonly float   ScaledMapRectWidth;
        public readonly float   ScaledMapRectHeight;
        public readonly bool    RotateMap;

        public MapFrameData(
                        IMapState state,
                        RectTransform mapContainer,
                        RectTransform minimapRect,
                        int levelWidth,
                        int levelHeight,
                        bool rotateMap) {

            Origin        = state.Origin;
            YawDegrees    = state.YawDegrees;
            VisiblyRadius = state.VisiblyRadius;
            RotateMap     = rotateMap;

            HalfLevelWidth  = levelWidth * 0.5f;
            HalfLevelHeight = levelHeight * 0.5f;

            var playerPx = Mathf.Clamp(Origin.x + HalfLevelWidth, 0, levelWidth);
            var playerPy = Mathf.Clamp(Origin.z + HalfLevelHeight, 0, levelHeight);

            PlayerNormalizedPosition = new Vector2(
                            playerPx / levelWidth,
                            playerPy / levelHeight
            );

            var mapRect = minimapRect.rect;
            Scale = (mapContainer.rect.width * 0.5f) * ((levelWidth / mapRect.width + levelHeight / mapRect.height) * 0.5f) / VisiblyRadius;

            ScaledMapRectWidth  = mapRect.width * Scale;
            ScaledMapRectHeight = mapRect.height * Scale;

            PlayerLocalPosition = new Vector2(
                            (PlayerNormalizedPosition.x - 0.5f) * ScaledMapRectWidth,
                            (PlayerNormalizedPosition.y - 0.5f) * ScaledMapRectHeight
            );

            YawRadians = YawDegrees * Mathf.Deg2Rad;
            YawCos     = Mathf.Cos(YawRadians);
            YawSin     = Mathf.Sin(YawRadians);

            var mapContainerRect = mapContainer.rect;
            MaskRadius = Mathf.Min(mapContainerRect.width, mapContainerRect.height) * 0.5f;
            MaskRadiusSqr = MaskRadius * MaskRadius;
        }

        public Vector2 RotateVector(Vector2 vector) {
            return RotateMap
                            ? new Vector2(vector.x * YawCos - vector.y * YawSin, vector.x * YawSin + vector.y * YawCos)
                            : vector;
        }

        public bool IsInVisibleRadius(Vector3 worldPosition) {
            var dx = worldPosition.x - Origin.x;
            var dz = worldPosition.z - Origin.z;
            return dx * dx + dz * dz <= VisiblyRadius * VisiblyRadius;
        }

        public Vector2 GetWorldToMapDelta(Vector3 worldPos, int levelWidth, int levelHeight) {
            var px = Mathf.Clamp(worldPos.x + HalfLevelWidth, 0, levelWidth);
            var py = Mathf.Clamp(worldPos.z + HalfLevelHeight, 0, levelHeight);

            var normalizedX = px / levelWidth;
            var normalizedY = py / levelHeight;

            var localX = (normalizedX - 0.5f) * ScaledMapRectWidth;
            var localY = (normalizedY - 0.5f) * ScaledMapRectHeight;

            return new Vector2(
                            localX - PlayerLocalPosition.x,
                            localY - PlayerLocalPosition.y
            );
        }
    }
}