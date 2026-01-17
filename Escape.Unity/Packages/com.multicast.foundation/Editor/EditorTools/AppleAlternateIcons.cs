using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Multicast.Tools.AppleAlternateIconGen {
    using System;
    using System.Linq;

    [CreateAssetMenu(menuName = "CodeWriter/iOS Alternate Icons")]
    public class AppleAlternateIcons : ScriptableObject {
        [SerializeField]
        [TableList(AlwaysExpanded = true, ShowPaging = false)]
        private IconEntry[] icons = Array.Empty<IconEntry>();

        [Serializable]
        private struct IconEntry {
            [TableColumnWidth(60, false)]
            public bool included;

            [FolderPath(RequireExistingPath = true)]
            public string folder;
        }

        public string[] GetIncludedAlternateIconPaths() {
            return this.icons.Where(it => it.included).Select(it => it.folder).ToArray();
        }

#if UNITY_EDITOR
        [Button]
        private void Refresh() {
            this.icons = Array.ConvertAll(this.SearchAlternateIconPaths(), it => new IconEntry {folder = it});
        }

        [Button(ButtonSizes.Large)]
        public void ApplyTextureImportSettings() {
            foreach (var icon in this.icons) {
                var texGuids = AssetDatabase.FindAssets("t:Texture", new[] {icon.folder});

                foreach (var texGuid in texGuids) {
                    var texPath = AssetDatabase.GUIDToAssetPath(texGuid);

                    var importer = (TextureImporter) AssetImporter.GetAtPath(texPath);

                    importer.wrapMode           = TextureWrapMode.Clamp;
                    importer.textureType        = TextureImporterType.GUI;
                    importer.mipmapEnabled      = false;
                    importer.textureCompression = TextureImporterCompression.CompressedHQ;

                    importer.SaveAndReimport();
                }
            }
        }

        private string[] SearchAlternateIconPaths() {
            var path      = AssetDatabase.GetAssetPath(this);
            var directory = Path.GetDirectoryName(path) ?? "";
            return Array.ConvertAll(Directory.GetDirectories(directory), it => it.Replace('\\', '/'));
        }
#endif
    }
}