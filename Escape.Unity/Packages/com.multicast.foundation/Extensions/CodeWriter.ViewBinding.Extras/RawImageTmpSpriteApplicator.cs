namespace CodeWriter.ViewBinding.Applicators.UI {
    using System;
    using TMPro;
    using TriInspector;
    using UnityEngine;
    using UnityEngine.TextCore;
    using UnityEngine.UI;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(RawImage))]
    [AddComponentMenu("View Binding/UI/[Binding] RawImage TMP Sprite Applicator")]
    public class RawImageTmpSpriteApplicator : ComponentApplicatorBase<RawImage, ViewVariableString> {
        [SerializeField, Required] private string spriteNameFormat = "{0}";

        [NonSerialized] private TMP_SpriteAsset currentSpriteAsset;

        protected override void Apply(RawImage target, ViewVariableString source) {
            var spriteName     = string.Format(this.spriteNameFormat, source.Value);
            var spriteNameHash = TMP_TextParsingUtilities.GetHashCodeCaseSensitive(spriteName);

            this.currentSpriteAsset = TMP_SpriteAsset.SearchForSpriteByHashCode(this.currentSpriteAsset, spriteNameHash, true, out var spriteIndex);

            if (spriteIndex == -1) {
                this.currentSpriteAsset = TMP_SpriteAsset.SearchForSpriteByHashCode(TMP_Settings.defaultSpriteAsset, spriteNameHash, true, out spriteIndex);
            }

            if (spriteIndex == -1) {
                Debug.LogError($"No TMP sprite found with name '{spriteName}' at '{this.name}'", this);
                target.texture = null;
                return;
            }

            var spriteChar  = this.currentSpriteAsset.spriteCharacterTable[spriteIndex];
            var spriteGlyph = this.currentSpriteAsset.spriteGlyphTable[(int) spriteChar.glyphIndex];

            if (spriteChar.unicode == TMP_Settings.missingCharacterSpriteUnicode) {
                Debug.LogError($"No TMP sprite found with name '{spriteName}' at '{this.name}'", this);
            }

            target.texture = this.currentSpriteAsset.spriteSheet;
            target.uvRect  = GetUvRect(spriteGlyph.glyphRect, target.texture);
        }

        private static Rect GetUvRect(GlyphRect r, Texture t) {
            var s = new Vector2(t.width, t.height);
            return new Rect(r.x / s.x, r.y / s.y, r.width / s.x, r.height / s.y);
        }
    }
}