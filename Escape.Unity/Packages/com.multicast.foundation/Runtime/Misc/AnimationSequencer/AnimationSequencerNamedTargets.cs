// ReSharper disable ParameterHidesMember

namespace Multicast.Misc.AnimationSequencer {
    using System;
    using System.Collections.Generic;
    using TriInspector;
    using UnityEngine;

    [DisallowMultipleComponent]
    [DrawWithTriInspector]
    public class AnimationSequencerNamedTargets : AnimationSequencerNamedTargetsBase {
        [SerializeField]
        [TableList(AlwaysExpanded = true)]
        private List<TargetData> targets;

        [Serializable]
        private struct TargetData {
            [Required]
            public string name;

            public Transform target;
        }

        public override bool IsTargetDefined(string name) {
            if (this.targets == null) {
                return false;
            }

            foreach (var it in this.targets) {
                if (it.name == name) {
                    return true;
                }
            }

            return false;
        }

        public override IEnumerable<string> EnumerateTargetNames() {
            if (this.targets == null) {
                yield break;
            }

            foreach (var it in this.targets) {
                yield return it.name;
            }
        }

        public override Transform GetTarget(string name) {
            if (this.targets == null) {
                return null;
            }

            foreach (var it in this.targets) {
                if (it.name == name) {
                    return it.target;
                }
            }

            return null;
        }

        public void SetTarget(string name, Transform target) {
            this.targets ??= new List<TargetData>();

            for (var i = 0; i < this.targets.Count; i++) {
                if (this.targets[i].name != name) {
                    continue;
                }

                this.targets[i] = new TargetData {
                    name   = name,
                    target = target,
                };

                return;
            }

            this.targets.Add(new TargetData {
                name   = name,
                target = target,
            });
        }
    }
}