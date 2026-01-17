using DG.Tweening;
using UnityEngine;

public class WidgetPosition {
    public enum Position {
        Left,
        Center,
        Right,
    }

    public static void SetPosition(RectTransform root, Position position, bool useAnimation = true) {
        var pivot = position switch {
            Position.Left => new Vector2(0, 0.5f),
            Position.Center => new Vector2(0.5f, 0.5f),
            Position.Right => new Vector2(1f, 0.5f),
            _ => new Vector2(0.5f, 0.5f),
        };

        var rect = root.rect;
            
        var startPosition = position switch {
            Position.Left => new Vector2(-rect.width, 0f),
            Position.Center => new Vector2(0f, -rect.height),
            Position.Right => new Vector2(rect.width, 0.5f),
            _ => new Vector2(0f, 0f),
        };
            
        root.DOKill();

        root.anchorMin        = pivot;
        root.anchorMax        = pivot;
        root.pivot            = pivot;
        root.anchoredPosition = useAnimation ?  startPosition : Vector2.zero;

        if (useAnimation) {
            root.DOAnchorPos(Vector2.zero, 0.1f);
        }
    }
}