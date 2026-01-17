namespace Game.ECS.Components.WorldView {
    using System;
    using Scellecs.Morpeh;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    public struct WorldViewComponent : IComponent {
        [NonSerialized] public RectTransform transform;
        [NonSerialized] public Func<Vector3> targetFunc;
        [NonSerialized] public Canvas        canvas;
        [NonSerialized] public RectTransform canvasRect;
        [NonSerialized] public bool          hideWhenOffscreen;
        [NonSerialized] public CanvasGroup   canvasGroup;
    }
}