#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace _EditorTools.TextureExplorer {
    public class TextureExplorerItem {
        public Texture TextureAsset;
        public string  TypeAndSize; // "2D / 512x512"
        public long    BytesMemory;
        public int     Usages;

        public readonly List<Material> Materials = new List<Material>();

        public int MaterialCount => this.Materials.Count;
    }
}
#endif