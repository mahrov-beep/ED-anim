namespace Game.UI.Views.Game.Hud {
    using System.Collections.Generic;
    using Domain.Game;
    using Multicast;
    using Sirenix.OdinInspector;
    using UniMob.UI;
    using UnityEngine;
    using UnityEngine.UI;

    public interface IGrenadeIndicatorViewState : IViewState {
        List<GrenadeIndicatorData> GrenadeIndicators { get; }
    }

    public class GrenadeIndicatorView : AutoView<IGrenadeIndicatorViewState> {
        [TitleGroup("Common"), SerializeField]
        private RectTransform indicatorRect;
        
        [TitleGroup("Prefabs"), SerializeField]
        private Image onScreenIndicatorPrefab;
        
        [TitleGroup("Prefabs"), SerializeField]
        private Image offScreenIndicatorPrefab;
        
        [TitleGroup("Settings"), SerializeField]
        private Color indicatorColor = Color.red;
        
        [TitleGroup("Settings"), Min(0f), SerializeField]
        private float offScreenMargin = 50f;

        private readonly Queue<Image> onScreenPool = new();
        private readonly Queue<Image> offScreenPool = new();
        private readonly List<Image> activeOnScreenIndicators = new(8);
        private readonly List<Image> activeOffScreenIndicators = new(8);

        private float halfWidth;
        private float halfHeight;

        protected override void Activate() {
            base.Activate();
            
            halfWidth = indicatorRect.rect.width * 0.5f;
            halfHeight = indicatorRect.rect.height * 0.5f;

            WarmUpPool(onScreenIndicatorPrefab, onScreenPool, 8);
            WarmUpPool(offScreenIndicatorPrefab, offScreenPool, 8);
        }

        private void LateUpdate() {
            if (State == null) return;

            var indicators = State.GrenadeIndicators;
            int onScreenIdx = 0;
            int offScreenIdx = 0;

            for (int i = 0, c = indicators.Count; i < c; i++) {
                var data = indicators[i];
                
                if (data.IsOnScreen) {
                    ShowOnScreenIndicator(ref onScreenIdx, data);
                } else {
                    ShowOffScreenIndicator(ref offScreenIdx, data);
                }
            }

            DisableUnused(activeOnScreenIndicators, onScreenIdx);
            DisableUnused(activeOffScreenIndicators, offScreenIdx);
        }

        private void ShowOnScreenIndicator(ref int idx, GrenadeIndicatorData data) {
            var img = GetIndicator(ref idx, activeOnScreenIndicators, onScreenIndicatorPrefab, onScreenPool);
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                indicatorRect, 
                data.ScreenPosition, 
                null, 
                out var localPos
            );

            img.rectTransform.anchoredPosition = localPos;
            img.rectTransform.rotation = Quaternion.identity;
            img.color = indicatorColor;
        }

        private void ShowOffScreenIndicator(ref int idx, GrenadeIndicatorData data) {
            var img = GetIndicator(ref idx, activeOffScreenIndicators, offScreenIndicatorPrefab, offScreenPool);
            
            var screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            var dir = new Vector2(data.ScreenPosition.x, data.ScreenPosition.y) - screenCenter;
            dir.Normalize();

            var maxX = halfWidth - offScreenMargin;
            var maxY = halfHeight - offScreenMargin;

            float tX = dir.x != 0 ? Mathf.Abs(maxX / dir.x) : float.MaxValue;
            float tY = dir.y != 0 ? Mathf.Abs(maxY / dir.y) : float.MaxValue;
            float t = Mathf.Min(tX, tY);

            var edgePos = new Vector2(dir.x * t, dir.y * t);

            img.rectTransform.anchoredPosition = edgePos;
            img.rectTransform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
            img.color = indicatorColor;
        }

        private Image GetIndicator(ref int idx, List<Image> activeList, Image prefab, Queue<Image> pool) {
            Image img;
            if (idx < activeList.Count) {
                img = activeList[idx];
            } else {
                if (pool.Count == 0) {
                    WarmUpPool(prefab, pool, 4);
                }
                img = pool.Dequeue();
                activeList.Add(img);
            }

            if (!img.gameObject.activeSelf) {
                img.gameObject.SetActive(true);
            }
            img.sprite = prefab.sprite;
            idx++;
            return img;
        }

        private static void DisableUnused(List<Image> activeList, int from) {
            for (int i = from, c = activeList.Count; i < c; i++) {
                var img = activeList[i];
                if (img.gameObject.activeSelf) {
                    img.gameObject.SetActive(false);
                }
            }
        }

        private void WarmUpPool(Image prefab, Queue<Image> pool, int count) {
            if (indicatorRect == null || prefab == null) return;
            
            for (var i = 0; i < count; i++) {
                var img = Instantiate(prefab);
                img.transform.SetParent(indicatorRect, false);
                img.gameObject.SetActive(false);
                pool.Enqueue(img);
            }
        }
    }
}

