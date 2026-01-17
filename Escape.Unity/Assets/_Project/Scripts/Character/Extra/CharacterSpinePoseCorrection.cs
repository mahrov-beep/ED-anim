using InfimaGames.LowPolyShooterPack;
using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterSpinePoseCorrection : MonoBehaviour {
    [SerializeField, Required] private CharacterBehaviour characterBehaviour;

    [SerializeField] private Quaternion hipsPose = Quaternion.identity;

    [SerializeField, Range(0, 1f)] private float spineStabilizationWeight;
    [SerializeField, Range(0, 1f)] private float chestCorrectionWeight;
    [SerializeField, Range(0, 1f)] private float turnPlacementCorrectionWeight;

    private Transform hipsTransform;
    private Transform spineTransform;
    private Transform chestTransform;
    private Transform upperChestTransform;

    // Armature hint:
    // - UpperChest (spine03), child of chest
    // - Chest (spine02, грудь), child of spine
    // - Spine (spine01, спина), child of hips
    // - Hips (pelvis, бёдра, таз)

    private void Start() {
        var animator = this.characterBehaviour.GetCharacterAnimator();

        this.hipsTransform       = animator.GetBoneTransform(HumanBodyBones.Hips);
        this.spineTransform      = animator.GetBoneTransform(HumanBodyBones.Spine);
        this.chestTransform      = animator.GetBoneTransform(HumanBodyBones.Chest);
        this.upperChestTransform = animator.GetBoneTransform(HumanBodyBones.UpperChest);
    }

    private void LateUpdate() {
        var weight = 1f;

        if (this.characterBehaviour.GetFullBodyState() != CharacterFullBodyStates.Default) {
            weight = 0f;
        }
        
        var animator = this.characterBehaviour.GetCharacterAnimator();

        var animatorSpineRotation       = this.spineTransform.rotation;
        var stabilizedSpineRotation     = animator.rootRotation * this.hipsPose * this.spineTransform.localRotation;
        var finalSpineRotation          = Quaternion.Slerp(animatorSpineRotation, stabilizedSpineRotation, this.spineStabilizationWeight * weight);
        var spineCorrectedRotationDelta = Quaternion.Inverse(finalSpineRotation) * animatorSpineRotation;

        // стабилизация спины
        // анимация нод может вращать Spine как угодно, поэтому после анимации
        // нужно корректировать вращение спины чтобы оружие смотрело точно вперёд
        this.spineTransform.rotation = finalSpineRotation;

        // интерполяция груди
        // после стабилизации спины поворот между нижней и верхней частями спины
        // может быть слишком некрасивый, поэтому плавно интерполируем Chest примерно в среднее положение
        var cachedUpperChestRotation = this.upperChestTransform.rotation;
        this.chestTransform.rotation      *= Quaternion.Slerp(Quaternion.identity, spineCorrectedRotationDelta, this.chestCorrectionWeight * weight);
        this.upperChestTransform.rotation =  cachedUpperChestRotation;

        // Turn тела
        // при повороте туловища вокруг своей оси корректируем Hips так
        // чтобы визуально выглядело что это вращается не верхняя часть туловища, а нижняя
        var turnAngle = animator.GetFloat("Turn X");
        this.hipsTransform.rotation *= Quaternion.AngleAxis(turnAngle, new Vector3(1, 0, 0));

        // Попытка скорректировать эффект "плавающих ног" который появляется после
        // Turn тела. Не уверен что это формула вообще имеет смысл, но выглядит чуть лучше
        var hipsLocalPosition = this.hipsTransform.localPosition;
        hipsLocalPosition                = Quaternion.AngleAxis(-turnAngle, new Vector3(0, 0, 1)) * hipsLocalPosition;
        this.hipsTransform.localPosition = Vector3.Lerp(this.hipsTransform.localPosition, hipsLocalPosition, this.turnPlacementCorrectionWeight * weight);
    }
}