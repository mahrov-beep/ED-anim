namespace Multicast {
    using System;
    using BrunoMikoski.AnimationSequencer;
    using CodeWriter.ViewBinding;
    using DG.Tweening;
    using UnityEngine;
    using UnityEngine.Scripting;

    [Serializable]
    public abstract class SyncViewVariableAnimationStepBase : AnimationStepBase {
        public abstract ViewVariable ViewVariable { get; }
    }

    [Serializable]
    public abstract class SyncViewVariableAnimationStepBase<TViewVariable> : SyncViewVariableAnimationStepBase
        where TViewVariable : ViewVariable {
        [SerializeField]
        private TViewVariable viewVariable;

        public sealed override ViewVariable ViewVariable => this.viewVariable;

        public sealed override string DisplayName { get; } = $"Sync {typeof(TViewVariable).Name}";

        public sealed override void AddTweenToSequence(Sequence animationSequence) {
            var sequence = DOTween.Sequence();

            sequence.SetDelay(this.Delay);
            sequence.AppendCallback(this.Sync);

            if (this.FlowType == FlowType.Append) {
                animationSequence.Append(sequence);
            }
            else {
                animationSequence.Join(sequence);
            }
        }

        public sealed override void ResetToInitialState() {
        }

        public sealed override string GetDisplayNameForEditor(int index) {
            return $"{index}. {this.DisplayName}: {this.viewVariable.Name}";
        }

        private void Sync() {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return;
            }
#endif

            if (this.viewVariable.Context is AutoView autoView) {
                autoView.ForceSyncVariable(this.viewVariable);
            }
        }
    }

    [Serializable, Preserve]
    public sealed class SyncViewVariableIntAnimationStep : SyncViewVariableAnimationStepBase<ViewVariableInt> {
    }

    [Serializable, Preserve]
    public sealed class SyncViewVariableFloatAnimationStep : SyncViewVariableAnimationStepBase<ViewVariableFloat> {
    }

    [Serializable, Preserve]
    public sealed class SyncViewVariableBigDoubleAnimationStep : SyncViewVariableAnimationStepBase<ViewVariableBigDouble> {
    }

    [Serializable, Preserve]
    public sealed class SyncViewVariableBoolAnimationStep : SyncViewVariableAnimationStepBase<ViewVariableBool> {
    }

    [Serializable, Preserve]
    public sealed class SyncViewVariableStringAnimationStep : SyncViewVariableAnimationStepBase<ViewVariableString> {
    }
}