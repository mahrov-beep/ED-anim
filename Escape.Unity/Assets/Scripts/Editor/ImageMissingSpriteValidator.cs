using Sirenix.OdinInspector.Editor.Validation;
using UnityEngine;
using UnityEngine.UI;

[assembly: RegisterValidationRule(typeof(ImageMissingSpriteValidator))]

public class ImageMissingSpriteValidator : RootObjectValidator<Image> {
    public Sprite NullSprite;

    protected override void Validate(ValidationResult result) {
        var image = this.Value;

        if (image.sprite != null) {
            return;
        }

        result.AddWarning("Do not use Image component without sprite")
            .WithFix("Fix", () => this.Value.sprite = this.NullSprite);
    }
}