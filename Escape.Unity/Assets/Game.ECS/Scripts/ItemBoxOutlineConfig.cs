using System;
using EPOOutline;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.ECS.Scripts
{
    [CreateAssetMenu(fileName = "ItemBoxOutlineConfig", menuName = "Game/Configs/Item Box Outline Config")]
    public class ItemBoxOutlineConfig : ScriptableObject
    {
        [Header("Visuals"), HideIf(nameof(IsFrontBack))]
        public Color color;
        public RenderStyle renderStyle = RenderStyle.Single;        
        public float activationRadius = 4f;

        [Header("Front/Back"), ShowIf(nameof(IsFrontBack)), InlineProperty, LabelWidth(70)]
        public OutlineSide front;
        [ShowIf(nameof(IsFrontBack)), InlineProperty, LabelWidth(70)]
        public OutlineSide back;

        [Header("Per object shifts (enables info buffer)")]
        [Range(0.1f, 3f)]
        [NonSerialized] public float dilateShift = 1f;
        [Range(0.1f, 3f)]
        [NonSerialized] public float blurShift = 1f;

        public bool IsFrontBack => renderStyle == RenderStyle.FrontBack;

        [Serializable]
        public struct OutlineSide
        {
            [LabelText("Enabled")]
            public bool enabled;

            [LabelText("Color")]
            public Color color;
        }
    }
}
