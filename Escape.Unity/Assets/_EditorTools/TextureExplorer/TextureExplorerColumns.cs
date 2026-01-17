#if UNITY_EDITOR
namespace _EditorTools.TextureExplorer {
    using System.Collections.Generic;
    using System.Linq;
    using Multicast;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;

    public enum TextureExplorerColumns {
        Preview,
        TextureName,
        TypeAndSize,
        Format,
        MemorySize,
        MipMaps,
        MipStreaming,
        UsagesCount,
        MaterialCount,
        MaterialsList,
    }

    public static class TextureExplorerColumnsMethods {
        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState() {
            var columns = new MultiColumnHeaderState.Column[10];

            columns[(int)TextureExplorerColumns.Preview] = new MultiColumnHeaderState.Column {
                headerContent         = new GUIContent("Preview"),
                contextMenuText       = "Preview",
                canSort               = false,
                width                 = 50,
                autoResize            = false,
                allowToggleVisibility = false,
            };
            columns[(int)TextureExplorerColumns.TextureName] = new MultiColumnHeaderState.Column {
                headerContent         = new GUIContent("Texture Name"),
                contextMenuText       = "Texture Name",
                canSort               = true,
                sortedAscending       = true,
                minWidth              = 100,
                autoResize            = true,
                allowToggleVisibility = true,
            };
            columns[(int)TextureExplorerColumns.TypeAndSize] = new MultiColumnHeaderState.Column {
                headerContent         = new GUIContent("Type / Size"),
                canSort               = true,
                width                 = 160,
                autoResize            = false,
                allowToggleVisibility = true,
            };
            columns[(int)TextureExplorerColumns.Format] = new MultiColumnHeaderState.Column {
                headerContent         = new GUIContent("Format"),
                canSort               = true,
                width                 = 160,
                autoResize            = false,
                allowToggleVisibility = true,
            };
            columns[(int)TextureExplorerColumns.MemorySize] = new MultiColumnHeaderState.Column {
                headerContent         = new GUIContent("Memory"),
                canSort               = true,
                width                 = 60,
                autoResize            = false,
                allowToggleVisibility = true,
            };
            columns[(int)TextureExplorerColumns.MipMaps] = new MultiColumnHeaderState.Column {
                headerContent         = new GUIContent("Mips", "MipMaps count"),
                canSort               = true,
                width                 = 50,
                autoResize            = false,
                allowToggleVisibility = true,
            };
            columns[(int)TextureExplorerColumns.MipStreaming] = new MultiColumnHeaderState.Column {
                headerContent         = new GUIContent("Streaming", "Is MipMap streaming enabled?"),
                canSort               = true,
                width                 = 60,
                autoResize            = false,
                allowToggleVisibility = true,
            };
            columns[(int)TextureExplorerColumns.MaterialCount] = new MultiColumnHeaderState.Column {
                headerContent         = new GUIContent("by Mat", "Count of usages by Materials"),
                canSort               = true,
                width                 = 50,
                autoResize            = false,
                allowToggleVisibility = true,
            };
            columns[(int)TextureExplorerColumns.UsagesCount] = new MultiColumnHeaderState.Column {
                headerContent         = new GUIContent("by Obj", "Count of usages by GameObjects"),
                canSort               = true,
                width                 = 50,
                autoResize            = false,
                allowToggleVisibility = true,
            };
            columns[(int)TextureExplorerColumns.MaterialsList] = new MultiColumnHeaderState.Column {
                headerContent         = new GUIContent("Materials List"),
                canSort               = false,
                minWidth              = 100,
                autoResize            = true,
                allowToggleVisibility = true,
            };

            return new MultiColumnHeaderState(columns);
        }

        public static void DrawCell(Rect rect, TextureExplorerTreeViewItem item, TextureExplorerColumns column) {
            switch (column) {
                case TextureExplorerColumns.Preview:
                    EditorGUI.DrawPreviewTexture(rect, item.TextureInfo.TextureAsset);
                    break;

                case TextureExplorerColumns.TextureName:
                    EditorGUI.ObjectField(rect, item.TextureInfo.TextureAsset, typeof(Texture), false);
                    break;

                case TextureExplorerColumns.TypeAndSize:
                    EditorGUI.LabelField(rect, item.TextureInfo.TypeAndSize);
                    break;

                case TextureExplorerColumns.Format:
                    EditorGUI.LabelField(rect, item.TextureInfo.TextureAsset.graphicsFormat.ToString());
                    break;

                case TextureExplorerColumns.MemorySize:
                    var bytes = item.TextureInfo.BytesMemory;
                    var str = bytes >= 1024 * 1024 ? $"{bytes / 1024 / 1024} MB"
                        : bytes >= 1024 ? $"{bytes / 1024} kb"
                        : $"{bytes} b";
                    EditorGUI.LabelField(rect, str);
                    break;

                case TextureExplorerColumns.MipMaps:
                    if (item.TextureInfo.TextureAsset.mipmapCount > 1) {
                        EditorGUI.IntField(rect, item.TextureInfo.TextureAsset.mipmapCount);
                    }

                    break;

                case TextureExplorerColumns.MipStreaming:
                    if (item.TextureInfo.TextureAsset is Texture2D t2d) {
                        EditorGUI.Toggle(rect, t2d.streamingMipmaps);
                    }

                    break;

                case TextureExplorerColumns.UsagesCount:
                    EditorGUI.IntField(rect, item.TextureInfo.Usages);
                    break;

                case TextureExplorerColumns.MaterialCount:
                    EditorGUI.IntField(rect, item.TextureInfo.MaterialCount);
                    break;

                case TextureExplorerColumns.MaterialsList:
                    var materialNames = string.Join(", ", item.TextureInfo.Materials.Select(m => m.name));
                    EditorGUI.TextField(rect, materialNames);
                    break;
            }
        }

        public static IOrderedEnumerable<TextureExplorerTreeViewItem> Sort(
            IOrderedEnumerable<TextureExplorerTreeViewItem> items, TextureExplorerColumns column, bool ascending) {
            return column switch {
                TextureExplorerColumns.TextureName => ThenBy(items, l => l.TextureInfo.TextureAsset.name, ascending),
                TextureExplorerColumns.TypeAndSize => ThenBy(items, l => l.TextureInfo.TypeAndSize, ascending),
                TextureExplorerColumns.Format => ThenBy(items, l => (int)l.TextureInfo.TextureAsset.graphicsFormat, ascending),
                TextureExplorerColumns.MemorySize => ThenBy(items, l => l.TextureInfo.BytesMemory, ascending),
                TextureExplorerColumns.MipMaps => ThenBy(items, l => l.TextureInfo.TextureAsset.mipmapCount, ascending),
                TextureExplorerColumns.MipStreaming => ThenBy(items, l => l.TextureInfo.TextureAsset is Texture2D t2d ? (t2d.streamingMipmaps ? 1 : 0) : -1, ascending),
                TextureExplorerColumns.MaterialCount => ThenBy(items, l => l.TextureInfo.MaterialCount, ascending),
                TextureExplorerColumns.UsagesCount => ThenBy(items, l => l.TextureInfo.Usages, ascending),
                _ => items,
            };
        }

        private static IOrderedEnumerable<T> ThenBy<T, TKey>(IOrderedEnumerable<T> source, System.Func<T, TKey> keySelector, bool ascending) {
            return ascending ? source.ThenBy(keySelector) : source.ThenByDescending(keySelector);
        }

        private static IOrderedEnumerable<T> ThenBy<T>(IOrderedEnumerable<T> source, System.Func<T, string> keySelector, bool ascending) {
            return ascending
                ? source.ThenBy(keySelector, NaturalComparer.Instance)
                : source.ThenByDescending(keySelector, NaturalComparer.Instance);
        }

        private class NaturalComparer : IComparer<string> {
            public static readonly NaturalComparer Instance = new NaturalComparer();

            public int Compare(string x, string y) {
                return EditorUtility.NaturalCompare(x, y);
            }
        }
    }
}
#endif