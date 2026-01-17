namespace Multicast {
    using System;
    using BrunoMikoski.AnimationSequencer;
    using DG.Tweening;
    using JetBrains.Annotations;
    using Misc.AnimationSequencer;
    using UnityEngine;

    [Serializable]
    public sealed class MoveToNamedTargetDoTweenAction : MoveDOTweenActionBase {
        public override Type TargetComponentType => typeof(Transform);

        [SerializeField]
        private AnimationSequencerNamedTarget namedTarget;

        [SerializeField]
        private bool useLocalPosition;

        [NonSerialized]
        private Transform target;

        [PublicAPI]
        public string NamedTarget {
            get => this.namedTarget;
            set => this.namedTarget = value;
        }

        [PublicAPI]
        public bool UseLocalPosition {
            get => this.useLocalPosition;
            set => this.useLocalPosition = value;
        }

        public override string DisplayName => "Move To Named Transform Position";

        protected override Tweener GenerateTween_Internal(GameObject target, float duration) {
            var collection = target.GetComponentInParent<AnimationSequencerNamedTargetsBase>();

            if (collection != null) {
                this.target = collection.GetTarget(this.namedTarget);
            }

            return base.GenerateTween_Internal(target, duration);
        }

        protected override Vector3 GetPosition() {
            return this.useLocalPosition ? this.target.localPosition : this.target.position;
        }
    }
}