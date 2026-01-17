using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[ExecuteAlways]
public class SkinColorizer : MonoBehaviour {
    [SerializeField]
    [OnValueChanged(nameof(Apply))]
    private Color color = Color.white;

    [SerializeField]
    [LabelText("Colorized Renderers")]
    [ReadOnly]
    [TableList(AlwaysExpanded = true, ShowPaging = false)]
    private List<ColorApplication> applied;

    [System.Serializable]
    private class ColorApplication {
        [Required]
        public Renderer renderer;

        [TableColumnWidth(30, false)]
        public int index;

        [ShowInInspector, DisplayAsString]
        public Material Material => this.renderer && this.index >= 0 && this.index < this.renderer.sharedMaterials.Length
            ? this.renderer.sharedMaterials[this.index]
            : null;

        [ShowInInspector, DisplayAsString]
        public Shader Shared => this.Material ? this.Material.shader : null;
    }

    private void OnEnable() {
        this.Apply();
    }

    private void OnDisable() {
        this.UnApply();
    }

    private void Apply() {
        this.applied.RemoveAll(it => it.renderer == null);

        foreach (var colorApplication in this.applied) {
            var block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", this.color);
            colorApplication.renderer.SetPropertyBlock(block, colorApplication.index);
        }
    }

    private void UnApply() {
        this.applied.RemoveAll(it => it.renderer == null);

        foreach (var colorApplication in this.applied) {
            var block = new MaterialPropertyBlock();
            colorApplication.renderer.SetPropertyBlock(block, colorApplication.index);
        }
    }

#if UNITY_EDITOR
    [Button]
    private void RefreshRenderers() {
        var renderers = this.GetComponentsInChildren<Renderer>(true);

        this.applied ??= new List<ColorApplication>();
        this.applied.Clear();

        foreach (var renderer in renderers) {
            var materials = renderer.sharedMaterials;

            for (var i = 0; i < materials.Length; i++) {
                var mat = materials[i];
                if (mat == null || !mat.HasProperty("_BaseColor")) continue;
                if (mat.shader.name == "Standard") continue;
                
                var block = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(block);
                block.SetColor("_BaseColor", color);
                renderer.SetPropertyBlock(block);
                
                this.applied.Add(new ColorApplication {
                    renderer = renderer,
                    index    = i
                });
            }
        }

        this.Apply();
    }

#endif

    private static bool IsChildOf(Transform transform, Transform parent) {
        while (transform != null) {
            if (transform == parent) {
                return true;
            }

            transform = transform.parent;
        }

        return false;
    }
}