namespace Multicast.Misc.AnimationSequencer {
    using System;
    using System.Collections.Generic;
    using BrunoMikoski.AnimationSequencer;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using UnityEngine;
    using UnityEngine.Pool;

    public class AnimationSequencerGlobal {
        private static readonly Dictionary<(string primary, string secondary), HashSet<AnimationSequencerController>> Dict = new();

        private static readonly Action<AnimationSequencerController, bool, Action> RewindCall = static(it, _, callback) => {
            it.Rewind();
            callback();
        };

        private static readonly Action<AnimationSequencerController, bool, Action> CompleteCall = static(it, _, callback) => {
            it.Complete();
            callback();
        };

        private static readonly Action<AnimationSequencerController, bool, Action> ClearPlayingSequenceCall = static(it, _, callback) => {
            it.ClearPlayingSequence();
            callback();
        };

        private static readonly Action<AnimationSequencerController, bool, Action> PlayCall
            = static(it, _, callback) => it.Play(callback);

        private static readonly Action<AnimationSequencerController, bool, Action> PlayForwardCall
            = static(it, resetFirst, callback) => it.PlayForward(resetFirst, callback);

        private static readonly Action<AnimationSequencerController, bool, Action> PlayBackwardsCall
            = static(it, completeFirst, callback) => it.PlayBackwards(completeFirst, callback);

        public static void Register(string primary, string secondary, AnimationSequencerController controller) {
            GetCollection(primary, secondary).Add(controller);
        }

        public static void Unregister(string primary, string secondary, AnimationSequencerController controller) {
            GetCollection(primary, secondary).Remove(controller);
        }

        // Rewind

        [PublicAPI]
        public static UniTask Rewind(string primary, string secondary) {
            return PlayInternal(primary, secondary, RewindCall, default);
        }

        [PublicAPI]
        public static UniTask RewindAll(string primary, IEnumerable<string> secondary) {
            return PlayInternalSequenced(primary, secondary, RewindCall, default);
        }

        // Complete

        [PublicAPI]
        public static UniTask Complete(string primary, string secondary) {
            return PlayInternal(primary, secondary, CompleteCall, default);
        }

        [PublicAPI]
        public static UniTask CompleteAll(string primary, IEnumerable<string> secondary) {
            return PlayInternalSequenced(primary, secondary, CompleteCall, default);
        }

        // ClearPlayingSequence

        [PublicAPI]
        public static UniTask ClearPlayingSequence(string primary, string secondary) {
            return PlayInternal(primary, secondary, ClearPlayingSequenceCall, default);
        }

        [PublicAPI]
        public static UniTask ClearPlayingSequenceAll(string primary, IEnumerable<string> secondary) {
            return PlayInternalSequenced(primary, secondary, ClearPlayingSequenceCall, default);
        }

        // Play

        [PublicAPI]
        public static UniTask Play(string primary, string secondary) {
            return PlayInternal(primary, secondary, PlayCall, default);
        }

        [PublicAPI]
        public static UniTask PlayAllParallel(string primary, IEnumerable<string> secondary) {
            return PlayInternalParallel(primary, secondary, PlayCall, default);
        }

        [PublicAPI]
        public static UniTask PlayAllSequenced(string primary, IEnumerable<string> secondary) {
            return PlayInternalSequenced(primary, secondary, PlayCall, default);
        }

        // PlayForward

        [PublicAPI]
        public static UniTask PlayForward(string primary, string secondary, bool resetFirst = true) {
            return PlayInternal(primary, secondary, PlayForwardCall, resetFirst);
        }

        [PublicAPI]
        public static UniTask PlayAllForwardParallel(string primary, IEnumerable<string> secondary, bool resetFirst = true) {
            return PlayInternalParallel(primary, secondary, PlayForwardCall, resetFirst);
        }

        [PublicAPI]
        public static UniTask PlayAllForwardSequenced(string primary, IEnumerable<string> secondary, bool resetFirst = true) {
            return PlayInternalSequenced(primary, secondary, PlayForwardCall, resetFirst);
        }

        // PlayBackward

        [PublicAPI]
        public static UniTask PlayBackwards(string primary, string secondary, bool completeFirst = true) {
            return PlayInternal(primary, secondary, PlayBackwardsCall, completeFirst);
        }

        [PublicAPI]
        public static UniTask PlayAllBackwardsParallel(string primary, IEnumerable<string> secondary, bool completeFirst = true) {
            return PlayInternalParallel(primary, secondary, PlayBackwardsCall, completeFirst);
        }

        [PublicAPI]
        public static UniTask PlayAllBackwardsSequenced(string primary, IEnumerable<string> secondary, bool completeFirst = true) {
            return PlayInternalSequenced(primary, secondary, PlayBackwardsCall, completeFirst);
        }

        // Internal

        private static UniTask PlayInternal<TState>(string primary, string secondary,
            Action<AnimationSequencerController, TState, Action> call, TState state) {
            var list = GetCollection(primary, secondary);

            if (list.Count == 0) {
                Debug.LogError($"Failed to play animation sequencer ({primary}, {secondary}): no sequencers found");
            }

            var runningAnimations = 0;

            // ReSharper disable once AccessToModifiedClosure
            void FinishCall() => --runningAnimations;

            foreach (var it in list) {
                ++runningAnimations;

                call.Invoke(it, state, FinishCall);
            }

            return UniTask.WaitWhile(() => runningAnimations > 0);
        }

        private static async UniTask PlayInternalParallel<TState>(string primary, IEnumerable<string> secondary,
            Action<AnimationSequencerController, TState, Action> call, TState state) {
            using (ListPool<UniTask>.Get(out var tasks)) {
                foreach (var it in secondary) {
                    tasks.Add(PlayInternal(primary, it, call, state));
                }

                await UniTask.WhenAll(tasks);
            }
        }

        private static async UniTask PlayInternalSequenced<TState>(string primary, IEnumerable<string> secondary,
            Action<AnimationSequencerController, TState, Action> call, TState state) {
            foreach (var it in secondary) {
                await PlayInternal(primary, it, call, state);
            }
        }

        private static HashSet<AnimationSequencerController> GetCollection(string primary, string secondary) {
            var key = (primary, secondary);

            if (!Dict.TryGetValue(key, out var list)) {
                Dict.Add(key, list = new HashSet<AnimationSequencerController>());
            }

            return list;
        }
    }
}