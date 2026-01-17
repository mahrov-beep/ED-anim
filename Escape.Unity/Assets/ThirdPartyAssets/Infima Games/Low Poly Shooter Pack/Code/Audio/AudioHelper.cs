using System;
using System.Collections.Generic;
using Quantum;
using UnityEngine;

namespace Audio {
    public class AudioHelper {
        private const float BS_BASE_ROLLOFF_POWER = 1.35f;

        private static readonly Dictionary<int, AnimationCurve> CurveCache = new();

        private static AnimationCurve BuildRolloffCurve(float power, float floor = 0f) {
            // t = нормализованная дистанция [0..1], y = громкость [0..1]
            // y = (1 - t)^power  (меньше power -> слышим дальше)
            const int steps = 12;

            var keys = new Keyframe[steps + 1];

            for (var i = 0; i <= steps; i++) {
                var t = i / (float)steps;
                var y = Mathf.Pow(1f - t, power);

                y       = Mathf.Lerp(floor, 1f, y); // floor можно >0, если нужно не в абсолютный ноль
                keys[i] = new Keyframe(t, y);
            }

            return new AnimationCurve(keys);
        }

        private static void ApplyRolloff(AudioSource src, float minDistance, float maxDistance, float distanceBoostFactor) {
            if (Math.Abs(src.maxDistance - maxDistance) < 0.25f) {
                return;
            }
            
            // distanceBoostFactor = maxDistance / базовыйMaxDistance ( >1 если буст )
            // чем больше буст, тем положе кривая
            var power = Mathf.Lerp(BS_BASE_ROLLOFF_POWER * 1.8f, BS_BASE_ROLLOFF_POWER * 0.6f,
                Mathf.Clamp01(Mathf.InverseLerp(1f, 3f, distanceBoostFactor)));

            // округлим power для кэша
            var key = Mathf.RoundToInt(power * 100f);

            if (!CurveCache.TryGetValue(key, out var curve)) {
                curve           = BuildRolloffCurve(power, floor: 0f); // floor=0 => затухание в ноль
                CurveCache[key] = curve;
            }

            src.spatialBlend = 1f;
            src.rolloffMode  = AudioRolloffMode.Custom;
            src.minDistance  = minDistance;
            src.maxDistance  = maxDistance;
            src.dopplerLevel = 0f;

            src.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve);
        }

        public static void PlayAudioSourceWithBoost(
            float volume,
            float minDistance,
            float maxDistance,
            float volumeMultiplierBoost,
            float maxDistanceMultiplierBoost,
            int priority,
            AudioSource audioSource,
            AudioClip clip) {

            var prevMaxDistance = maxDistance;

            volume      *= 1 + volumeMultiplierBoost;
            maxDistance *= 1 + maxDistanceMultiplierBoost;

            audioSource.priority = priority;

            ApplyRolloff(audioSource, minDistance, maxDistance, maxDistance / prevMaxDistance);

            audioSource.PlayOneShot(clip, volume);
        }
    }
}