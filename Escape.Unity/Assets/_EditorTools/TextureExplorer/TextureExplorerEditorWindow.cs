#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace _EditorTools.TextureExplorer {
    using System;
    using UnityEditor.SceneManagement;
    using UnityEngine.SceneManagement;

    public class TextureExplorerEditorWindow : EditorWindow {
        private TreeViewState<int>      treeViewState;
        private TextureExplorerTreeView textureTree;
        private MultiColumnHeaderState  multiColumnHeaderState;
        private DateTime                lastReloadTime;

        private readonly List<TextureExplorerItem> textureItems = new List<TextureExplorerItem>();

        [MenuItem("Tools/Analysis/Texture Explorer")]
        public static void ShowWindow() {
            var window = GetWindow<TextureExplorerEditorWindow>("Texture Explorer");
            window.Show();
        }

        private void OnEnable() {
            this.treeViewState ??= new TreeViewState<int>();

            var headerState = TextureExplorerColumnsMethods.CreateDefaultMultiColumnHeaderState();

            if (MultiColumnHeaderState.CanOverwriteSerializedFields(this.multiColumnHeaderState, headerState)) {
                MultiColumnHeaderState.OverwriteSerializedFields(this.multiColumnHeaderState, headerState);
            }

            this.multiColumnHeaderState = headerState;

            var multiColumnHeader = new MultiColumnHeader(this.multiColumnHeaderState);
            this.textureTree = new TextureExplorerTreeView(this.treeViewState, multiColumnHeader);

            EditorSceneManager.sceneOpened += this.OnSceneOpened;
        }

        private void OnDisable() {
            EditorSceneManager.sceneOpened -= this.OnSceneOpened;
        }

        private void OnGUI() {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Reload", EditorStyles.toolbarButton)) {
                this.RefreshTextures();
                GUIUtility.ExitGUI();
            }

            var sinceLastReload = DateTime.Now - this.lastReloadTime;
            var sinceLastReloadStr = sinceLastReload.TotalSeconds < 30
                ? $"Last reload: {Math.Round(sinceLastReload.TotalSeconds)} seconds ago"
                : $"Last reload: {Math.Round(sinceLastReload.TotalMinutes)} minutes ago";
            GUILayout.Label(sinceLastReloadStr, EditorStyles.centeredGreyMiniLabel, GUILayout.Height(18));

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            var rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            if (this.textureTree != null && this.textureTree.HasAnyItem) {
                this.textureTree.OnGUI(rect);
            }
            else {
                GUI.Label(rect, "No textures found in current scene. Click 'Reload' to scan", EditorStyles.centeredGreyMiniLabel);
            }
        }

        private void RefreshTextures() {
            this.textureItems.Clear();
            this.textureItems.AddRange(TextureExplorerSearcher.Search());

            this.textureTree.SetItems(this.textureItems);
            if (this.textureTree.HasAnyItem) {
                this.textureTree.Reload();
                this.textureTree.multiColumnHeader.ResizeToFit();
            }

            this.lastReloadTime = DateTime.Now;
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode) {
            EditorApplication.delayCall += () => this.RefreshTextures();
        }
    }
}
#endif