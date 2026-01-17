#if TUTORIAL_MASK

namespace Multicast.Misc.Tutorial {
    using CodeWriter.UIExtensions;
    using TriInspector;
    using UnityEngine;

    [DrawWithTriInspector]
    [RequireComponent(typeof(TutorialObject))]
    [AddComponentMenu("Tutorial/TutorialObject Global (Static Binding)")]
    public class TutorialObjectGlobalStaticBinding : TutorialObjectBindingBase {
        [SerializeField] private string secondaryKey;

        protected override string GetSecondaryKey() {
            return this.secondaryKey;
        }
    }
}

#endif