#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace _EditorTools.TextureExplorer {
    internal class TextureExplorerTreeView : TreeView<int> {
        private List<TextureExplorerItem> textureItems;

        public TextureExplorerTreeView(TreeViewState<int> treeViewState, MultiColumnHeader multiColumnHeader)
            : base(treeViewState, multiColumnHeader) {
            this.showAlternatingRowBackgrounds = true;
            this.showBorder                    = true;

            multiColumnHeader.sortingChanged += this.OnSortingChanged;
        }

        public bool HasAnyItem => this.textureItems?.Count > 0;

        public void SetItems(List<TextureExplorerItem> items) {
            this.textureItems = items;
        }

        protected override TreeViewItem<int> BuildRoot() {
            var root      = new TreeViewItem<int> { id = 0, depth = -1, displayName = "Root" };
            var idCounter = 1;

            if (this.textureItems == null || this.textureItems.Count == 0) {
                SetupDepthsFromParentsAndChildren(root);
                return root;
            }

            foreach (var item in this.textureItems) {
                var textureNode = new TextureExplorerTreeViewItem(idCounter++, 0, item.TextureAsset.name, item);
                root.AddChild(textureNode);
            }

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        protected override void RowGUI(RowGUIArgs args) {
            var item = (TextureExplorerTreeViewItem)args.item;

            for (var visibleColIndex = 0; visibleColIndex < args.GetNumVisibleColumns(); ++visibleColIndex) {
                var column   = (TextureExplorerColumns)args.GetColumn(visibleColIndex);
                var cellRect = args.GetCellRect(visibleColIndex);

                this.CenterRectUsingSingleLineHeight(ref cellRect);
                TextureExplorerColumnsMethods.DrawCell(cellRect, item, column);
            }
        }

        private void OnSortingChanged(MultiColumnHeader header) {
            this.SortIfNeeded(this.rootItem, this.GetRows());
        }

        private void SortIfNeeded(TreeViewItem<int> root, IList<TreeViewItem<int>> rows) {
            if (rows.Count <= 1) {
                return;
            }

            if (this.multiColumnHeader.sortedColumnIndex == -1) {
                rows.Sort((x, y) => EditorUtility.NaturalCompare(x.displayName, y.displayName));
                return;
            }

            var sortedColumns = this.multiColumnHeader.state.sortedColumns;
            if (sortedColumns.Length == 0) {
                return;
            }

            var orderedItems = this.SortByMultipleColumns(rows, sortedColumns);
            root.children = orderedItems.Cast<TreeViewItem<int>>().ToList();

            rows.Clear();
            foreach (var item in orderedItems) {
                rows.Add(item);
            }

            this.Repaint();
        }

        private List<TextureExplorerTreeViewItem> SortByMultipleColumns(IList<TreeViewItem<int>> rows, int[] sortedColumns) {
            var orderedItems = rows.Cast<TextureExplorerTreeViewItem>().OrderBy(_ => 0);

            foreach (var columnIndex in sortedColumns) {
                var column    = (TextureExplorerColumns)columnIndex;
                var ascending = this.multiColumnHeader.IsSortedAscending(columnIndex);
                orderedItems = TextureExplorerColumnsMethods.Sort(orderedItems, column, ascending);
            }

            return orderedItems.ToList();
        }
    }
}

#endif