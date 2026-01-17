namespace Multicast.Misc.AnimationSequencer {
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class AnimationSequencerNamedTargetsBase : MonoBehaviour {
        public abstract bool IsTargetDefined(string name);

        public abstract Transform GetTarget(string name);

        public abstract IEnumerable<string> EnumerateTargetNames();
    }
}