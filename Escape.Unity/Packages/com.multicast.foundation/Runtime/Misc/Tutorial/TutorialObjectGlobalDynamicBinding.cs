#if TUTORIAL_MASK

namespace Multicast.Misc.Tutorial {
    using CodeWriter.UIExtensions;
    using CodeWriter.ViewBinding;
    using TriInspector;
    using UnityEngine;

    [DrawWithTriInspector]
    [RequireComponent(typeof(TutorialObject))]
    [AddComponentMenu("Tutorial/TutorialObject Global (Dynamic Binding)")]
    public class TutorialObjectGlobalDynamicBinding : TutorialObjectBindingBase {
        [SerializeField] private ViewVariableString secondaryKey;

        protected override string GetSecondaryKey() {
            return this.secondaryKey.Value;
        }
    }
}

#endif