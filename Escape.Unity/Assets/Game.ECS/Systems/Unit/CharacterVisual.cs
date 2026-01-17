namespace Game.ECS.Systems.Unit {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain;
    using JetBrains.Annotations;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class CharacterVisual : MonoBehaviour {
        [SerializeField]
        [HideInInspector]
        private List<GameObject> visuals;

#if UNITY_EDITOR
        [ShowInInspector]
        [NonSerialized]
        [Searchable]
        [TableList(AlwaysExpanded = true, IsReadOnly = true, ShowPaging = false, DrawScrollView = true, ScrollViewHeight = 410)]
        private List<VisualPreview> visualPreviews;

        [Serializable]
        private struct VisualPreview {
            [ReadOnly, EnableGUI]
            public GameObject obj;

            [ShowInInspector]
            [PropertyOrder(-1)]
            [TableColumnWidth(20, false)]
            public bool On {
                get => this.obj.activeSelf;
                set => this.obj.SetActive(value);
            }

            [ShowInInspector]
            [TableColumnWidth(200, false)]
            [EnableGUI]
            [GUIColor("@this.Color")]
            public string Validation => this.IsMatchDde ? "Valid" : "Do not match any item in DDE";

            [UsedImplicitly]
            private Color Color => this.IsMatchDde ? new Color(0.7f, 1f, 0.7f) : new Color(1f, 0.5f, 0.5f);

            private bool IsMatchDde => CoreConstants.GameDefAccessEditorOnly().Items.TryGet(this.obj.name, out _);
        }

        [NonSerialized]
        private bool visualsIsMissingError;

        [OnInspectorInit]
        private void OnInspectorInit() {
            this.visuals ??= new List<GameObject>();
            this.visuals.RemoveAll(it => it == null);

            this.RevalidateVisuals();
            this.GeneratePreviews();
        }

        [Button(ButtonSizes.Large)]
        private void DisableAllVisuals() {
            this.RefreshVisualsEditorOnly();

            this.visuals.ForEach(it => it.gameObject.SetActive(false));
        }

        [Button(ButtonSizes.Large), PropertyOrder(-1), GUIColor(1.0f, 0.65f, 0.5f), ShowIf(nameof(visualsIsMissingError))]
        [InfoBox("Some visuals is missing", InfoMessageType.Error, visibleIfMemberName: nameof(visualsIsMissingError))]
        private void RefreshVisualsEditorOnly() {
            this.visuals.Clear();
            this.visuals.AddRange(this.SearchListeners());

            this.RevalidateVisuals();
            this.GeneratePreviews();
        }

        private void GeneratePreviews() {
            this.visualPreviews ??= new List<VisualPreview>();
            this.visualPreviews.Clear();
            this.visualPreviews.AddRange(this.visuals.Select(it => new VisualPreview { obj = it }));
        }

        private GameObject[] SearchListeners() {
            return this.transform.GetComponentsInChildren<Transform>(includeInactive: true)
                .Select(it => it.gameObject)
                .Where(it => it.name.StartsWith("item_"))
                .OrderBy(it => it.name)
                .ToArray();
        }

        private void RevalidateVisuals() {
            if (Application.isPlaying) {
                return;
            }

            this.visualsIsMissingError = !this.visuals.SequenceEqual(this.SearchListeners());
        }
#endif
        
        [NonSerialized]
        private Dictionary<string, string> enabledVisualForKey = new Dictionary<string, string>();
        
        public void SetVisual(string key, [CanBeNull] string itemKey) {
            if (this.enabledVisualForKey.TryGetValue(key, out var existing)) {
                if (existing == itemKey) {
                    return;
                }
                
                this.SetEnabled(existing, false);
                this.enabledVisualForKey.Remove(key);
            }

            if (!string.IsNullOrEmpty(itemKey)) {
                this.enabledVisualForKey[key] = itemKey;
                this.SetEnabled(itemKey, true);
            }
        }

        [NonSerialized]
        private Dictionary<string, GameObject> cache;

        private void SetEnabled(string itemKey, bool enabled) {
            if (this.cache == null) {
                this.cache = new Dictionary<string, GameObject>();

                foreach (var visual in this.visuals) {
                    this.cache[visual.name] = visual;
                }
            }

            if (this.cache.TryGetValue(itemKey, out var itemVisual)) {
                itemVisual.SetActive(enabled);
            }
        }

        public bool IsExisting(string key, [CanBeNull] string itemKey) {
            if (this.enabledVisualForKey.TryGetValue(key, out var existing)) {
                return existing == itemKey;
            }

            return false;
        }
        
        public bool IsNotEmpty(string key) {
            if (this.enabledVisualForKey.TryGetValue(key, out var existing)) {
                return true;
            }

            return false;
        }
    }
}