#if UNITY_EDITOR

namespace _EditorTools.TextureExplorer {
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Profiling;
    using UnityEngine.Rendering;

    public class TextureExplorerSearcher {
        public static List<TextureExplorerItem> Search() {
            var textureUsage = new Dictionary<Texture, TextureExplorerItem>();

            foreach (var lightmapData in LightmapSettings.lightmaps) {
                if (lightmapData.lightmapColor) {
                    AddTexture(lightmapData.lightmapColor);
                }

                if (lightmapData.lightmapColor) {
                    AddTexture(lightmapData.lightmapColor);
                }

                if (lightmapData.shadowMask) {
                    AddTexture(lightmapData.shadowMask);
                }
            }

            var renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var renderer in renderers) {
                var materials = renderer.sharedMaterials;

                foreach (var material in materials) {
                    if (material == null) {
                        continue;
                    }

                    var shader        = material.shader;
                    var propertyCount = shader.GetPropertyCount();

                    for (var i = 0; i < propertyCount; i++) {
                        if (shader.GetPropertyType(i) != ShaderPropertyType.Texture) {
                            continue;
                        }

                        var propertyName = shader.GetPropertyName(i);
                        var texture      = material.GetTexture(propertyName);

                        if (texture == null) {
                            continue;
                        }

                        var item = AddTexture(texture);

                        if (!item.Materials.Contains(material)) {
                            item.Materials.Add(material);
                        }
                    }
                }
            }

            return textureUsage.Values.ToList();

            TextureExplorerItem AddTexture(Texture texture) {
                if (textureUsage.TryGetValue(texture, out var existing)) {
                    return existing;
                }

                var info     = new TextureExplorerItem { TextureAsset = texture };
                var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));

                if (texture is Texture2D texture2D) {
                    info.TypeAndSize = $"{importer.textureType} / {texture2D.width}x{texture2D.height}";
                }
                else {
                    info.TypeAndSize = $"{importer.textureType}";
                }

                info.BytesMemory = Profiler.GetRuntimeMemorySizeLong(texture) / (texture.isReadable ? 1 : 2);

                return textureUsage[texture] = info;
            }
        }
    }
}
#endif