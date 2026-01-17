namespace Multicast {
    using System;
    using BrunoMikoski.AnimationSequencer;
    using DG.Tweening;
    using JetBrains.Annotations;
    using Misc.AnimationSequencer;
    using UnityEngine;

    [Serializable]
    public sealed class RotateToNamedTransformDoTweenAction : RotateDOTweenActionBase {
        public override string DisplayName => "Rotate To Named Transform Euler Angles";

        [SerializeField]
        private AnimationSequencerNamedTarget namedTarget;

        [SerializeField]
        private bool useLocalEulerAngles;

        [NonSerialized]
        private Transform target;

        [PublicAPI]
        public string NamedTarget {
            get => this.namedTarget;
            set => this.namedTarget = value;
        }

        [PublicAPI]
        public bool UseLocalEulerAngles {
            get => this.useLocalEulerAngles;
            set => this.useLocalEulerAngles = value;
        }

        protected override Tweener GenerateTween_Internal(GameObject target, float duration) {
            var collection = target.GetComponentInParent<AnimationSequencerNamedTargetsBase>();

            if (collection != null) {
                this.target = collection.GetTarget(this.namedTarget);
            }

            return base.GenerateTween_Internal(target, duration);
        }

        protected override Vector3 GetRotation() {
            return this.useLocalEulerAngles ? this.target.localEulerAngles : this.target.eulerAngles;
        }
    }
}