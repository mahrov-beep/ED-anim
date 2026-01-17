namespace Multicast.Misc.AnimationSequencer {
    using BrunoMikoski.AnimationSequencer;
    using CodeWriter.ViewBinding;
    using TriInspector;
    using UnityEngine;

    [DrawWithTriInspector]
    [RequireComponent(typeof(AnimationSequencerController))]
    [AddComponentMenu("UI/AnimationSequencer Global (Static Binding)", 250)]
    public class AnimationSequencerGlobalStaticBinding : ApplicatorBase {
        [SerializeField, Required] private AnimationSequencerController controller;

        [SerializeField, Required] private string primaryKey;
        [SerializeField, Required] private string secondaryKey;

        private (string primary, string secondary) lastId;

        private void OnDisable() {
            AnimationSequencerGlobal.Unregister(this.lastId.primary, this.lastId.secondary, this.controller);
        }

        protected override void Apply() {
            AnimationSequencerGlobal.Unregister(this.lastId.primary, this.lastId.secondary, this.controller);

            this.lastId = (this.primaryKey, this.secondaryKey);

            AnimationSequencerGlobal.Register(this.lastId.primary, this.lastId.secondary, this.controller);
        }

#if UNITY_EDITOR
        protected override void Reset() {
            base.Reset();

            this.controller = this.GetComponent<AnimationSequencerController>();
        }
#endif
    }
}