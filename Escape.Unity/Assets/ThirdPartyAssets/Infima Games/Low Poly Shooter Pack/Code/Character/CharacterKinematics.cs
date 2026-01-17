//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;
using System.Collections.Generic;

namespace InfimaGames.LowPolyShooterPack
{
    using System;
    using Sirenix.OdinInspector;

    /// <summary>
    /// Handles all the Inverse Kinematics needed for our Character.
    /// Very important. Uses Unity's IK code.
    /// </summary>
    public class CharacterKinematics : MonoBehaviour
    {
        #region FIELDS SERIALIZED

        [Title("References")]

        [Tooltip("Reference to the character's Animator component.")]
        [SerializeField, Required]
        private Animator characterAnimator;

        [SerializeField, Required]
        private CharacterBehaviour characterBehaviour;

        [Title("Settings Arm Left")]
        
        [Tooltip("Left Arm Target. Determines what the IK target is.")]
        [SerializeField, Required] 
        private Transform armLeftTarget;

        [Range(0.0f, 1.0f)]
        [Tooltip("Inverse Kinematics Weight for the left arm.")]
        [SerializeField]
        private float armLeftWeightPosition = 1.0f;
        
        [Range(0.0f, 1.0f)]
        [Tooltip("Inverse Kinematics Weight for the left arm.")]
        [SerializeField]
        private float armLeftWeightRotation = 1.0f;

        [Tooltip("Left Arm Hierarchy. Root, Mid, Tip.")]
        [SerializeField, Required]
        private Transform[] armLeftHierarchy;
        
        [Title("Settings Arm Right")]
        
        [Tooltip("Left Arm Target. Determines what the IK target is.")]
        [SerializeField, Required] 
        private Transform armRightTarget;
        
        [Range(0.0f, 1.0f)]
        [Tooltip("Inverse Kinematics Weight for the right arm.")]
        [SerializeField]
        private float armRightWeightPosition = 1.0f;
        
        [Range(0.0f, 1.0f)]
        [Tooltip("Inverse Kinematics Weight for the right arm.")]
        [SerializeField]
        private float armRightWeightRotation = 1.0f;

        [Tooltip("Right Arm Hierarchy. Root, Mid, Tip.")]
        [SerializeField, Required]
        private Transform[] armRightHierarchy;

        [Title("Generic")]

        [Tooltip("Hint.")]
        [SerializeField]
        private Transform hint;
        
        [Range(0.0f, 1.0f)]
        [Tooltip("Hint Weight.")]
        [SerializeField]
        private float weightHint;

        [BoxGroup("IK Weapon To Hand"), InlineProperty, HideLabel]
        [SerializeField] private IKWeaponToHandData ikWeaponToHand;

        [Serializable]
        private struct IKWeaponToHandData {
            [SerializeField, Required] public Transform IKHandRight;
            [SerializeField, Required] public Transform RealHandRight;
            [SerializeField, Required] public Transform WeaponSocket;
        }

        #endregion

        #region FIELDS

        /// <summary>
        /// Maintain Target Position Offset.
        /// </summary>
        private bool maintainTargetPositionOffset;
        /// <summary>
        /// Maintain Target Rotation Offset.
        /// </summary>
        private bool maintainTargetRotationOffset;

        private float alphaLeft;

        private float alphaRight;

        private Transform runtimeInitialWeaponSocketForIK;

        #endregion
        
        #region CONSTANTS

        /// <summary>
        /// Constant.
        /// </summary>
        private const float kSqrEpsilon = 1e-8f;

        #endregion
        
        #region UNITY

        private void Awake()
        {
            if (characterBehaviour == null)
                characterBehaviour = GetComponent<CharacterBehaviour>();
        }

        private void Start() {
            // так как this.ikWeaponToHand.WeaponSocket двигается из этого скрипта, то нужно запомнить дефолтное положение
            // делаем копию объекта чтобы не считать это положение математикой
            this.runtimeInitialWeaponSocketForIK = new GameObject("Runtime Initial Weapon Socket for IK").transform;
            this.runtimeInitialWeaponSocketForIK.SetParent(this.ikWeaponToHand.WeaponSocket.parent);
            this.ikWeaponToHand.WeaponSocket.GetLocalPositionAndRotation(out var pos, out var rot);
            this.runtimeInitialWeaponSocketForIK.SetLocalPositionAndRotation(pos, rot);
        }

        /// <summary>
        /// Update.
        /// </summary>
        private void Update()
        {
            //Get Left Constraint Alpha.
            alphaLeft = characterAnimator.GetFloat(AHashes.AlphaIKHandLeft);
            //Get Right Constraint Alpha.
            alphaRight = characterAnimator.GetFloat(AHashes.AlphaIKHandRight);
        }

        /// <summary>
        /// Late Update.
        /// </summary>
        private void LateUpdate()
        {
            //Check References.
            if (characterAnimator == null)
            {
                //ReferenceError.
                Log.ReferenceError(this, gameObject);
                
                //Return.
                return;
            }

            if (characterBehaviour != null && characterBehaviour.IsHealing())
            {
               
                // When healing we zero the weights so LateUpdate can skip IK.
                alphaLeft = 0.0f;
                alphaRight = 0.0f;
               
            }

            
            //Compute.
            Compute(alphaLeft, alphaRight);
            
            ComputeWeaponToHand();
        }
        
        #endregion

        #region METHODS

        /// <summary>
        /// Передвигает трансформ WEAPON_SOCKET в руку персонажа, чтобы во время анимаций,
        /// например, смены оружия, оружие оставалось в руке
        /// </summary>
        /// <remarks>
        /// У нас есть кость weapon_socket которая чуть смещена относительно кости ik_hand.
        /// Данный метод выставляет для weapon_socket позицию и поворот относительно кости read_hand,
        /// но при этом сохраняет этот отступ
        /// </remarks>
        private void ComputeWeaponToHand() {
            var ikHand       = this.ikWeaponToHand.IKHandRight;
            var realHand     = this.ikWeaponToHand.RealHandRight;
            var weaponSocket = this.ikWeaponToHand.WeaponSocket;

            var defaultWeaponSocketPosition = this.runtimeInitialWeaponSocketForIK.position;
            var defaultWeaponSocketRotation = this.runtimeInitialWeaponSocketForIK.rotation;

            // смещение (по умолчанию) точки крепления оружия относительно положения правой руки
            var localPosition = ikHand.InverseTransformVector(defaultWeaponSocketPosition - ikHand.position);
            var localRotation = Quaternion.Inverse(ikHand.rotation) * defaultWeaponSocketRotation;

            var alphaWeaponToHand = 1f; // 0 is disabled, 1 is enabled

            weaponSocket.position = Vector3.Lerp(defaultWeaponSocketPosition, realHand.TransformPoint(localPosition), alphaWeaponToHand);
            weaponSocket.rotation = Quaternion.Lerp(defaultWeaponSocketRotation, realHand.rotation * localRotation, alphaWeaponToHand);
        }

        /// <summary>
        /// Computes the Inverse Kinematics for both arms.
        /// </summary>
        private void Compute(float weightLeft = 1.0f, float weightRight = 1.0f)
        {
            //Compute Left Arm.
            ComputeOnce(armLeftHierarchy, armLeftTarget, 
                armLeftWeightPosition * weightLeft, 
                armLeftWeightRotation * weightLeft);
            
            //Compute Right Arm.
            ComputeOnce(armRightHierarchy, armRightTarget, 
                armRightWeightPosition * weightRight, 
                armRightWeightRotation * weightRight);
        }

        /// <summary>
        /// Computes the Inverse Kinematics for one arm, or hierarchy.
        /// </summary>
        /// <param name="hierarchy">Arm Hierarchy. Root, Mid, Tip.</param>
        /// <param name="target">IK Target.</param>
        /// <param name="weightPosition">Position Weight.</param>
        /// <param name="weightRotation">Rotation Weight.</param>
        private void ComputeOnce(IReadOnlyList<Transform> hierarchy, Transform target, float weightPosition = 1.0f, float weightRotation = 1.0f)
        {
            Vector3 targetOffsetPosition = Vector3.zero;
            Quaternion targetOffsetRotation = Quaternion.identity;
            
            if (maintainTargetPositionOffset)
                targetOffsetPosition = hierarchy[2].position - target.position;
            if (maintainTargetRotationOffset)
                targetOffsetRotation = Quaternion.Inverse(target.rotation) * hierarchy[2].rotation;
            
            Vector3 aPosition = hierarchy[0].position;
            Vector3 bPosition = hierarchy[1].position;
            Vector3 cPosition = hierarchy[2].position;
            Vector3 targetPos = target.position;
            Quaternion targetRot = target.rotation;
            Vector3 tPosition = Vector3.Lerp(cPosition, targetPos + targetOffsetPosition, weightPosition);
            Quaternion tRotation = Quaternion.Lerp(hierarchy[2].rotation, targetRot * targetOffsetRotation, weightRotation);
            bool hasHint = hint != null && weightHint > 0f;

            Vector3 ab = bPosition - aPosition;
            Vector3 bc = cPosition - bPosition;
            Vector3 ac = cPosition - aPosition;
            Vector3 at = tPosition - aPosition;

            float abLen = ab.magnitude;
            float bcLen = bc.magnitude;
            float acLen = ac.magnitude;
            float atLen = at.magnitude;

            float oldAbcAngle = TriangleAngle(acLen, abLen, bcLen);
            float newAbcAngle = TriangleAngle(atLen, abLen, bcLen);

            // Bend normal strategy is to take whatever has been provided in the animation
            // stream to minimize configuration changes, however if this is collinear
            // try computing a bend normal given the desired target position.
            // If this also fails, try resolving axis using hint if provided.
            Vector3 axis = Vector3.Cross(ab, bc);
            if (axis.sqrMagnitude < kSqrEpsilon)
            {
                axis = hasHint ? Vector3.Cross(hint.position - aPosition, bc) : Vector3.zero;

                if (axis.sqrMagnitude < kSqrEpsilon)
                    axis = Vector3.Cross(at, bc);

                if (axis.sqrMagnitude < kSqrEpsilon)
                    axis = Vector3.up;
            }
            axis = Vector3.Normalize(axis);

            float a = 0.5f * (oldAbcAngle - newAbcAngle);
            float sin = Mathf.Sin(a);
            float cos = Mathf.Cos(a);
            Quaternion deltaR = new Quaternion(axis.x * sin, axis.y * sin, axis.z * sin, cos);
            hierarchy[1].rotation = deltaR * hierarchy[1].rotation;

            cPosition = hierarchy[2].position;
            ac = cPosition - aPosition;
            hierarchy[0].rotation = Quaternion.FromToRotation(ac, at) * hierarchy[0].rotation;

            if (hasHint)
            {
                float acSqrMag = ac.sqrMagnitude;
                if (acSqrMag > 0f)
                {
                    bPosition = hierarchy[1].position;
                    cPosition = hierarchy[2].position;
                    ab = bPosition - aPosition;
                    ac = cPosition - aPosition;

                    Vector3 acNorm = ac / Mathf.Sqrt(acSqrMag);
                    Vector3 ah = hint.position - aPosition;
                    Vector3 abProj = ab - acNorm * Vector3.Dot(ab, acNorm);
                    Vector3 ahProj = ah - acNorm * Vector3.Dot(ah, acNorm);

                    float maxReach = abLen + bcLen;
                    if (abProj.sqrMagnitude > (maxReach * maxReach * 0.001f) && ahProj.sqrMagnitude > 0f)
                    {
                        Quaternion hintR = Quaternion.FromToRotation(abProj, ahProj);
                        hintR.x *= weightHint;
                        hintR.y *= weightHint;
                        hintR.z *= weightHint;
                        hintR = Quaternion.Normalize(hintR);
                        hierarchy[0].rotation = hintR * hierarchy[0].rotation;
                    }
                }
            }

            hierarchy[2].rotation = tRotation;
        }
        
        private static float TriangleAngle(float aLen, float aLen1, float aLen2)
        {
            float c = Mathf.Clamp((aLen1 * aLen1 + aLen2 * aLen2 - aLen * aLen) / (aLen1 * aLen2) / 2.0f, -1.0f, 1.0f);
            return Mathf.Acos(c);
        }
        
        #endregion
    }
}