#if UNITY_EDITOR

using UnityEditor.IMGUI.Controls;

namespace _EditorTools.TextureExplorer {
    public class TextureExplorerTreeViewItem : TreeViewItem<int> {
        public TextureExplorerItem TextureInfo { get; }

        public TextureExplorerTreeViewItem(int id, int depth, string displayName, TextureExplorerItem info)
            : base(id, depth, displayName) {
            this.TextureInfo = info;
        }
    }
}
#endif