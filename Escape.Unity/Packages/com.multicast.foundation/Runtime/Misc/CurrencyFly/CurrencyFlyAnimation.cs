namespace Multicast.Misc.CurrencyFly {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using Numerics;
    using UnityEngine;
    using UnityEngine.Pool;

    public static partial class CurrencyFlyAnimation {
        private static readonly Dictionary<CurrencyFlySourceId, List<RectTransform>>      Sources      = new();
        private static readonly Dictionary<CurrencyFlyDestinationId, List<RectTransform>> Destinations = new();

        private static readonly Dictionary<CurrencyFlySourceId, CurrencyFlySourceInfo> LastSources = new();

        [PublicAPI]
        public static Func<string, BigDouble, IDisposable> CreateDelayedVisualization = (currencyKey, amount) => EmptyDelayedVisualization.Instance;

        [PublicAPI]
        public static TPlayer CreatePlayer<TPlayer>(
            CurrencyFlySourceId sourceId,
            CurrencyFlyDestinationId destinationId,
            [NotNull] string currencyKey, BigDouble currencyAmount,
            TPlayer player,
            int maxParticles = 10)
            where TPlayer : class, ICurrencyFlyAnimationPlayer {
            if (currencyKey == null) {
                throw new ArgumentNullException(nameof(currencyKey));
            }

            if (currencyAmount < 0) {
                throw new ArgumentOutOfRangeException(nameof(currencyAmount));
            }

            if (maxParticles < 0) {
                throw new ArgumentOutOfRangeException(nameof(maxParticles));
            }

            var destinations = GetDestinations(destinationId);
            if (destinations.Count == 0) {
                Debug.LogError($"No destination found for CurrencyFlyAnimation: destinationId = {destinationId}");
                return player;
            }

            var destination = destinations[^1];

            CurrencyFlySourceInfo sourceInfo;

            var sources = GetSources(sourceId);
            if (sources.Count > 0) {
                sourceInfo = new CurrencyFlySourceInfo {
                    position = sources[^1].position,
                    size     = sources[^1].rect.size,
                };
            }
            else if (LastSources.TryGetValue(sourceId, out sourceInfo)) {
                //
            }
            else {
                Debug.LogError($"No source found for CurrencyFlyAnimation: sourceId = {sourceId}");

                sourceInfo = new CurrencyFlySourceInfo {
                    position = Vector3.zero,
                    size     = new Vector2(100, 100),
                };
            }

            var particleCount     = BigDouble.Min(currencyAmount, maxParticles).RoundToIntUnsafe();
            var amountPerParticle = BigDouble.Floor(currencyAmount / particleCount);

            var delayedVisualizations = ListPool<IDisposable>.Get();

            using (ListPool<BigDouble>.Get(out var amountPerParticleList)) {
                for (var i = 0; i < particleCount; i++) {
                    amountPerParticleList.Add(amountPerParticle);
                }

                if (amountPerParticleList.Count > 0) {
                    amountPerParticleList[0] += currencyAmount - amountPerParticle * particleCount;
                }

#if UNITY_EDITOR
                if (amountPerParticleList.Aggregate(BigDouble.Zero, (s, it) => s + it) != currencyAmount) {
                    Debug.LogError($"Invalid amount calculation: {amountPerParticleList.Aggregate(BigDouble.Zero, (s, it) => s + it)}, {currencyAmount}");
                }
#endif

                foreach (var amount in amountPerParticleList) {
                    delayedVisualizations.Add(CreateDelayedVisualization.Invoke(currencyKey, amount));
                }
            }

            player.ParticleCount         = particleCount;
            player.Source                = sourceInfo;
            player.Destination           = destination;
            player.CurrencyKey           = currencyKey;
            player.DelayedVisualizations = delayedVisualizations;

            player.Setup();

            return player;
        }

        internal static void RegisterDestination(CurrencyFlyDestinationId destinationId, RectTransform destination) {
            var list = GetDestinations(destinationId);

            if (list.Contains(destination)) {
                return;
            }

            list.Add(destination);
        }

        internal static void UnregisterDestination(CurrencyFlyDestinationId destinationId, RectTransform destination) {
            var list = GetDestinations(destinationId);

            list.Remove(destination);
        }

        internal static void RegisterSource(CurrencyFlySourceId sourceId, RectTransform source) {
            var list = GetSources(sourceId);

            if (list.Contains(source)) {
                return;
            }

            list.Add(source);
            LastSources.Remove(sourceId);
        }

        internal static void UnregisterSource(CurrencyFlySourceId sourceId, RectTransform source) {
            var list = GetSources(sourceId);

            list.Remove(source);

            if (list.Count == 0 && source != null) {
                LastSources[sourceId] = new CurrencyFlySourceInfo {
                    position = source.position,
                    size     = source.rect.size,
                };
            }
        }

        private static List<RectTransform> GetDestinations(CurrencyFlyDestinationId destinationId) {
            if (!Destinations.TryGetValue(destinationId, out var list)) {
                Destinations.Add(destinationId, list = new List<RectTransform>());
            }

            return list;
        }

        private static List<RectTransform> GetSources(CurrencyFlySourceId source) {
            if (!Sources.TryGetValue(source, out var list)) {
                Sources.Add(source, list = new List<RectTransform>());
            }

            return list;
        }

        public class EmptyDelayedVisualization : IDisposable {
            public static readonly IDisposable Instance = new EmptyDelayedVisualization();

            public void Dispose() {
            }
        }
    }

    public interface ICurrencyFlyAnimationPlayer {
        int                   ParticleCount         { get; set; }
        string                CurrencyKey           { get; set; }
        CurrencyFlySourceInfo Source                { get; set; }
        RectTransform         Destination           { get; set; }
        List<IDisposable>     DelayedVisualizations { get; set; }

        void Setup();

        UniTask PlayParticle(int index);
    }

    public static class CurrencyFlyAnimationPlayerExtensions {
        [PublicAPI]
        public static async UniTask Play<TPlayer>(this TPlayer player, float delay = 0.0f)
            where TPlayer : class, ICurrencyFlyAnimationPlayer {
            if (player.DelayedVisualizations == null) {
                Debug.LogError("CurrencyFlyAnimationPlayer cannot be player multiple times");
                return;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(delay));
            await UniTask.WhenAll(Enumerable.Range(0, player.ParticleCount).Select(player.PlayParticle));

            ListPool<IDisposable>.Release(player.DelayedVisualizations);

            player.DelayedVisualizations = null;
            player.Destination           = null;
        }

        [PublicAPI]
        public static void PlayAndForget<TPlayer>(this TPlayer player, float delay = 0.0f)
            where TPlayer : class, ICurrencyFlyAnimationPlayer {
            player.Play(delay).Forget();
        }
    }

    [RequireFieldsInit, Serializable]
    public struct CurrencyFlySourceId {
        public string primaryKey;
        public string secondaryKey;

        public override string ToString() => $"{this.primaryKey} - {this.secondaryKey}";
    }

    [RequireFieldsInit, Serializable]
    public struct CurrencyFlyDestinationId {
        public string primaryKey;
        public string secondaryKey;

        public override string ToString() => $"{this.primaryKey} - {this.secondaryKey}";
    }

    [RequireFieldsInit, Serializable]
    public struct CurrencyFlySourceInfo {
        public Vector3 position;
        public Vector2 size;
    }
}