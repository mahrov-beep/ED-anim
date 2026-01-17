// ReSharper disable InconsistentNaming

namespace DG.Tweening {
    using Core;
    using Plugins.Options;
    using UnityEngine;

    public static class DOTweenExtensions {
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOAnchoredMove(
            this RectTransform target, Vector2 endValue, float duration, bool snapping = false) {
            var t = DOTween.To(() => target.anchoredPosition, x => target.anchoredPosition = x, endValue, duration);
            t.SetOptions(snapping).SetTarget(target);
            return t;
        }
    }
}