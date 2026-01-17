namespace Game.UI.Views.Game.Hud {
    using System;
    using System.Collections.Generic;
    using Domain.Game;
    using Multicast;
    using UniMob.UI;
    using UnityEngine;
    using UnityEngine.UI;

    public interface IDamageSourceCueViewState : IViewState {
        List<DamageSourceData> DamageSources { get; }
    }

    public class DamageSourceCueView : AutoView<IDamageSourceCueViewState> {
        [SerializeField] private RectTransform cueRect;
        [SerializeField] private Image         arrowPrefab;
        [SerializeField] private Image         onScreenArrowPrefab;

        [Min(0f), SerializeField] private float arrowRadiusMul = 1f;
        [Min(0f), SerializeField] private float mergeDistance  = .24f;
        [Min(0f), SerializeField] private float markerLifetime = .6f;
        [SerializeField]          private Color arrowColor     = Color.red;

        private readonly Queue<Image> arrowPool    = new();
        private readonly List<Image>  activeArrows = new(32);
        private readonly List<Image>  activeOnScreenArrows = new(32);

        private readonly List<DamageSourceData> onScreen  = new(32);
        private readonly List<DamageSourceData> offScreen = new(32);

        private readonly List<Vector2> mergedDirs   = new(32);
        private readonly List<float>   mergedTimers = new(32);

        private float r;
        private float thrSqr;

        protected override void Activate() {
            base.Activate();
            r      = .5f * Mathf.Min(cueRect.rect.width, cueRect.rect.height);
            thrSqr = mergeDistance * mergeDistance;
            WarmUpArrow(16);
        }

        private void LateUpdate() {
            if (State == null) {
                return;
            }

            // Split damage sources by on-screen/off-screen
            SplitByScreenState(State.DamageSources, onScreen, offScreen);

            // Merge off-screen indicators
            Merge(offScreen, thrSqr, mergedDirs, mergedTimers);

            var arrowIdx = 0;
            var onScreenIdx = 0;

            // Render on-screen damage indicators
            var onScreenArrowToUse = onScreenArrowPrefab != null ? onScreenArrowPrefab : arrowPrefab;
            for (int i = 0, c = onScreen.Count; i < c; i++) {
                var data = onScreen[i];
                var alpha = data.Timer / markerLifetime;
                UseOnScreenArrow(ref onScreenIdx, onScreenArrowToUse, data.ScreenPosition, alpha);
            }

            // Render off-screen damage indicators
            for (int i = 0, c = mergedDirs.Count; i < c; i++) {
                var dir   = mergedDirs[i];
                var alpha = mergedTimers[i] / markerLifetime;
                UseArrow(ref arrowIdx, dir, arrowRadiusMul, arrowColor, alpha);
            }

            DisableUnused(activeArrows, arrowIdx);
            DisableUnused(activeOnScreenArrows, onScreenIdx);
        }

        private void SplitByScreenState(List<DamageSourceData> src, List<DamageSourceData> onScreen, List<DamageSourceData> offScreen) {
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

        private void Merge(List<DamageSourceData> src, float thrSqr, List<Vector2> outDirs, List<float> outTimers) {
            outDirs.Clear();
            outTimers.Clear();

            for (int i = 0, c = src.Count; i < c; i++) {
                var dir   = src[i].ScreenNormalizedDirection;
                var timer = src[i].Timer;

                bool merged = false;
                for (int j = 0, m = outDirs.Count; j < m; j++) {
                    if ((dir - outDirs[j]).sqrMagnitude <= thrSqr) {
                        outDirs[j] += dir;
                        if (timer > outTimers[j]) {
                            outTimers[j] = timer;
                        }
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

        private void UseOnScreenArrow(ref int idx, Image prefab, Vector3 screenPos, float alpha) {
            var img = idx < activeOnScreenArrows.Count ? activeOnScreenArrows[idx] : GetOnScreenArrow(prefab);
            if (!img.gameObject.activeSelf) img.gameObject.SetActive(true);

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
            var c = arrowColor;
            c.a = Mathf.Clamp01(alpha);
            img.color = c;
            idx++;
        }

        private void UseArrow(ref int idx, Vector2 dir, float radiusMul, Color baseColor, float alpha) {
            var img = idx < activeArrows.Count ? activeArrows[idx] : GetArrow();
            if (!img.gameObject.activeSelf) {
                img.gameObject.SetActive(true);
            }
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
                if (img.gameObject.activeSelf) {
                    img.gameObject.SetActive(false);
                }
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

        private Image GetOnScreenArrow(Image prefab) {
            if (arrowPool.Count == 0) {
                WarmUpArrow(4);
            }
            var img = arrowPool.Dequeue();
            activeOnScreenArrows.Add(img);
            return img;
        }

        private Image GetArrow() {
            if (arrowPool.Count == 0) {
                WarmUpArrow(4);
            }
            var img = arrowPool.Dequeue();
            activeArrows.Add(img);
            return img;
        }
    }
}