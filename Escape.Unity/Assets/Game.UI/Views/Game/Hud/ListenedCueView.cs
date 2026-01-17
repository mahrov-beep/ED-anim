namespace Game.UI.Views.Game.Hud {
    using System.Collections.Generic;
    using Domain.Game;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI;
    using UnityEngine;
    using UnityEngine.UI;

    public interface IListenedCueViewState : IViewState {
        List<CueData> StepsScreenNormalizedDirections { get; }
        List<CueData> ShootScreenNormalizedDirections { get; }
    }

    public class ListenedCueView : AutoView<IListenedCueViewState> {
        [TitleGroup("Common"), SerializeField]
        private RectTransform cueRect;
        [TitleGroup("Common"), SerializeField]
        private Image arrowPrefab;

        [TitleGroup("Step/Prefabs"), SerializeField]
        private Image stepIconPrefab;
        [TitleGroup("Step/Prefabs"), SerializeField]
        private Image stepOnScreenPrefab;
        [TitleGroup("Step"), Min(0f), SerializeField]
        private float stepIconRadiusMul = 1f;
        [TitleGroup("Step"), Min(0f), SerializeField]
        private float stepArrowRadiusMul = 1f;
        [TitleGroup("Step"), Min(0f), SerializeField]
        private float stepMarkerLifetime = 2f;
        [TitleGroup("Step"), Min(0f), SerializeField]
        private float stepMergeDistance = .24f;
        [TitleGroup("Step/Appearance"), SerializeField]
        private Color stepArrowColor = Color.white;
        [TitleGroup("Step/Appearance"), SerializeField]
        private float stepIconRadiusOffset = 0f;

        [TitleGroup("Shoot/Prefabs"), SerializeField]
        private Image shootIconPrefab;
        [TitleGroup("Shoot/Prefabs"), SerializeField]
        private Image shootOnScreenPrefab;
        [TitleGroup("Shoot"), Min(0f), SerializeField]
        private float shootIconRadiusMul = 1f;
        [TitleGroup("Shoot"), Min(0f), SerializeField]
        private float shootArrowRadiusMul = 1f;
        [TitleGroup("Shoot"), Min(0f), SerializeField]
        private float shootMarkerLifetime = 2f;
        [TitleGroup("Shoot"), Min(0f), SerializeField]
        private float shootMergeDistance = .24f;
        [TitleGroup("Shoot/Appearance"), SerializeField]
        private Color shootArrowColor = Color.white;
        [TitleGroup("Shoot/Appearance"), SerializeField]
        private float shootIconRadiusOffset = 0f;

        private readonly Dictionary<Sprite, Queue<Image>> iconPools    = new();
        private readonly Queue<Image>                     arrowPool    = new();
        private readonly List<Image>                      activeIcons  = new(32);
        private readonly List<Image>                      activeArrows = new(32);
        private readonly List<Image>                      activeOnScreenIcons = new(32);

        private readonly List<CueData> stepOnScreen  = new(32);
        private readonly List<CueData> stepOffScreen = new(32);
        private readonly List<CueData> shootOnScreen  = new(32);
        private readonly List<CueData> shootOffScreen = new(32);

        private readonly List<Vector2> stepMergedDirs    = new(32);
        private readonly List<float>   stepMergedTimers  = new(32);
        private readonly List<Vector2> shootMergedDirs   = new(32);
        private readonly List<float>   shootMergedTimers = new(32);

        private float r;
        private float stepThrSqr;
        private float shootThrSqr;

        protected override void Activate() {
            base.Activate();
            r           = .5f * Mathf.Min(cueRect.rect.width, cueRect.rect.height);
            stepThrSqr  = stepMergeDistance * stepMergeDistance;
            shootThrSqr = shootMergeDistance * shootMergeDistance;

            WarmUpIcon(stepIconPrefab, 8);
            WarmUpIcon(shootIconPrefab, 8);
            if (stepOnScreenPrefab != null) WarmUpIcon(stepOnScreenPrefab, 8);
            if (shootOnScreenPrefab != null) WarmUpIcon(shootOnScreenPrefab, 8);
            WarmUpArrow(16);
        }

        private void LateUpdate() {
            if (State == null) return;

            // Split indicators by on-screen/off-screen
            SplitByScreenState(State.StepsScreenNormalizedDirections, stepOnScreen, stepOffScreen);
            SplitByScreenState(State.ShootScreenNormalizedDirections, shootOnScreen, shootOffScreen);

            // Merge off-screen indicators
            Merge(stepOffScreen, stepThrSqr, stepMergedDirs, stepMergedTimers);
            Merge(shootOffScreen, shootThrSqr, shootMergedDirs, shootMergedTimers);

            var iconIdx  = 0;
            var arrowIdx = 0;
            var onScreenIdx = 0;

            // Render on-screen step indicators
            var stepOnScreenPrefabToUse = stepOnScreenPrefab != null ? stepOnScreenPrefab : stepIconPrefab;
            for (int i = 0, c = stepOnScreen.Count; i < c; i++) {
                var data = stepOnScreen[i];
                var alpha = data.Timer / stepMarkerLifetime;
                UseOnScreenIcon(ref onScreenIdx, stepOnScreenPrefabToUse, data.ScreenPosition, alpha);
            }

            // Render on-screen shoot indicators
            var shootOnScreenPrefabToUse = shootOnScreenPrefab != null ? shootOnScreenPrefab : shootIconPrefab;
            for (int i = 0, c = shootOnScreen.Count; i < c; i++) {
                var data = shootOnScreen[i];
                var alpha = data.Timer / shootMarkerLifetime;
                UseOnScreenIcon(ref onScreenIdx, shootOnScreenPrefabToUse, data.ScreenPosition, alpha);
            }

            // Render off-screen step indicators
            for (int i = 0, c = stepMergedDirs.Count; i < c; i++) {
                var dir   = stepMergedDirs[i];
                var alpha = stepMergedTimers[i] / stepMarkerLifetime;
                UseIcon(ref iconIdx, stepIconPrefab, dir, stepIconRadiusMul, alpha, stepIconRadiusOffset);
                UseArrow(ref arrowIdx, dir, stepArrowRadiusMul, stepArrowColor, alpha);
            }

            // Render off-screen shoot indicators
            for (int i = 0, c = shootMergedDirs.Count; i < c; i++) {
                var dir   = shootMergedDirs[i];
                var alpha = shootMergedTimers[i] / shootMarkerLifetime;
                UseIcon(ref iconIdx, shootIconPrefab, dir, shootIconRadiusMul, alpha, shootIconRadiusOffset);
                UseArrow(ref arrowIdx, dir, shootArrowRadiusMul, shootArrowColor, alpha);
            }

            DisableUnused(activeIcons, iconIdx);
            DisableUnused(activeArrows, arrowIdx);
            DisableUnused(activeOnScreenIcons, onScreenIdx);
        }

        private void SplitByScreenState(List<CueData> src, List<CueData> onScreen, List<CueData> offScreen) {
            onScreen.Clear();
            offScreen.Clear();

            for (int i = 0, c = src.Count; i < c; i++) {
                var data = src[i];
                if (data.IsOnScreen) {
                    onScreen.Add(data);
                }
                else {
                    offScreen.Add(data);
                }
            }
        }

        private void Merge(List<CueData> src, float thrSqr, List<Vector2> outDirs, List<float> outTimers) {
            outDirs.Clear();
            outTimers.Clear();

            for (int i = 0, c = src.Count; i < c; i++) {
                var dir   = src[i].ScreenNormalizedDirection;
                var timer = src[i].Timer;

                bool merged = false;
                for (int j = 0, m = outDirs.Count; j < m; j++) {
                    if ((dir - outDirs[j]).sqrMagnitude <= thrSqr) {
                        outDirs[j] += dir;
                        if (timer > outTimers[j]) outTimers[j] = timer;
                        merged = true;
                        break;
                    }
                }

                if (!merged) {
                    outDirs.Add(dir);
                    outTimers.Add(timer);
                }
            }

            for (int i = 0, c = outDirs.Count; i < c; i++) {
                outDirs[i] = outDirs[i].normalized;
            }
        }

        private void UseOnScreenIcon(ref int idx, Image prefab, Vector3 screenPos, float alpha) {
            var img = idx < activeOnScreenIcons.Count ? activeOnScreenIcons[idx] : GetOnScreenIcon(prefab);
            if (!img.gameObject.activeSelf) img.gameObject.SetActive(true);
            img.transform.SetAsLastSibling();

            // Convert screen position to local position in RectTransform
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                cueRect,
                screenPos,
                null,
                out var localPos
            );

            img.sprite = prefab.sprite;
            img.rectTransform.anchoredPosition = localPos;
            img.rectTransform.rotation = Quaternion.identity;
            var c = img.color;
            c.a = Mathf.Clamp01(alpha);
            img.color = c;
            idx++;
        }

        private void UseIcon(ref int idx, Image prefab, Vector2 dir, float radiusMul, float alpha, float radiusOffset = 0f) {
            var img = idx < activeIcons.Count ? activeIcons[idx] : GetIcon(prefab);
            if (!img.gameObject.activeSelf) img.gameObject.SetActive(true);
            img.transform.SetAsLastSibling();
            img.sprite = prefab.sprite;
            var distance = r * radiusMul + radiusOffset;
            var position = dir * distance;
            img.rectTransform.anchoredPosition = position;
            img.rectTransform.rotation = Quaternion.identity;
            var c = img.color;
            c.a       = Mathf.Clamp01(alpha);
            img.color = c;
            idx++;
        }

        private void UseArrow(ref int idx, Vector2 dir, float radiusMul, Color baseColor, float alpha) {
            var img = idx < activeArrows.Count ? activeArrows[idx] : GetArrow();
            if (!img.gameObject.activeSelf) img.gameObject.SetActive(true);
            img.transform.SetAsFirstSibling();
            img.rectTransform.anchoredPosition = dir * (r * radiusMul);
            img.rectTransform.rotation         = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
            var c = baseColor;
            c.a       = Mathf.Clamp01(alpha);
            img.color = c;
            idx++;
        }

        private void DisableUnused(List<Image> list, int from) {
            for (int i = from, c = list.Count; i < c; i++) {
                var img = list[i];
                if (img.gameObject.activeSelf) img.gameObject.SetActive(false);
            }
        }

        private void WarmUpIcon(Image prefab, int count) {
            var sprite = prefab.sprite;
            if (!iconPools.TryGetValue(sprite, out var q)) {
                q                 = new Queue<Image>(count);
                iconPools[sprite] = q;
            }
            for (var i = 0; i < count; i++) {
                var img = Instantiate(prefab, cueRect);
                img.transform.SetParent(cueRect, false);
                img.gameObject.SetActive(false);
                q.Enqueue(img);
            }
        }

        private void WarmUpArrow(int count) {
            for (var i = 0; i < count; i++) {
                var img = Instantiate(arrowPrefab, cueRect);
                img.transform.SetParent(cueRect, false);
                img.gameObject.SetActive(false);
                arrowPool.Enqueue(img);
            }
        }

        private Image GetOnScreenIcon(Image prefab) {
            var sprite = prefab.sprite;
            if (!iconPools.TryGetValue(sprite, out var q) || q.Count == 0) {
                WarmUpIcon(prefab, 4);
                q = iconPools[sprite];
            }
            var img = q.Dequeue();
            activeOnScreenIcons.Add(img);
            return img;
        }

        private Image GetIcon(Image prefab) {
            var sprite = prefab.sprite;
            if (!iconPools.TryGetValue(sprite, out var q) || q.Count == 0) {
                WarmUpIcon(prefab, 4);
                q = iconPools[sprite];
            }
            var img = q.Dequeue();
            activeIcons.Add(img);
            return img;
        }

        private Image GetArrow() {
            if (arrowPool.Count == 0) WarmUpArrow(4);
            var img = arrowPool.Dequeue();
            activeArrows.Add(img);
            return img;
        }
    }
}