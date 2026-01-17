namespace CodeWriter.ViewBinding.Extras {
    using TriInspector;
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("View Binding/[Binding] Image Group Sprite Applicator")]
    public class ImageGroupSpriteApplicator : ApplicatorBase {
        [SerializeField]
        private Sprite activeSprite;
        [SerializeField]
        private Sprite inactiveSprite;

        [SerializeField]
        private ViewVariableInt source;

        [SerializeField]
        [Required]
        private Image[] group;

        protected override void Apply() {
            if (this.group == null) {
                Debug.LogError($"Null applicator target at '{this.name}'", this);
                return;
            }

            if (this.source == null) {
                return;
            }

            for (var i = 0; i < this.group.Length; i++) {
                this.group[i].sprite = i < this.source.Value ? this.activeSprite : this.inactiveSprite;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying) {
                foreach (var image in this.group) {
                    UnityEditor.EditorUtility.SetDirty(image);
                }
            }
#endif
        }
    }
}