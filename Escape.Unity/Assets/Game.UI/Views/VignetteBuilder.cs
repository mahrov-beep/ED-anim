using UnityEngine;
using UnityEngine.UI;

public class VignetteBuilder : MonoBehaviour {
    [SerializeField] Sprite  cornerSprite;
    [SerializeField] Vector2 cornerSize = new(256, 256);

    [Sirenix.OdinInspector.Button]
    void Rebuild() {
#if UNITY_EDITOR
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
#else
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
#endif

        var parent = (RectTransform)transform;
        var size   = parent.rect.size;
        var scale  = Mathf.Min(size.x * 0.5f / cornerSize.x, size.y * 0.5f / cornerSize.y, 1f);
        var fitted = cornerSize * scale;

        AddCorner("TL", new Vector2(0, 1), fitted, false, false);
        AddCorner("TR", new Vector2(1, 1), fitted, true, false);
        AddCorner("BR", new Vector2(1, 0), fitted, true, true);
        AddCorner("BL", new Vector2(0, 0), fitted, false, true);
    }

    void AddCorner(string name, Vector2 anchor, Vector2 size, bool flipX, bool flipY) {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(transform, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;

        var pivot          = anchor;
        if (flipX) pivot.x = 1f - pivot.x;
        if (flipY) pivot.y = 1f - pivot.y;
        rt.pivot = pivot;

        rt.sizeDelta = size;

        var img = go.GetComponent<Image>();
        img.sprite               = cornerSprite;
        img.raycastTarget        = false;
        img.preserveAspect       = true;
        img.transform.localScale = new Vector3(flipX ? -1 : 1, flipY ? -1 : 1, 1);
    }
}