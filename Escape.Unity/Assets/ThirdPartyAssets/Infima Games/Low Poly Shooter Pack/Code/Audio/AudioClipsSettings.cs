namespace InfimaGames.LowPolyShooterPack {
    using System;
    using UnityEngine;
    using Random = UnityEngine.Random;

    [Serializable]
    public class AudioClipsSettings {
        [SerializeField]
        private AudioClip[] clips = Array.Empty<AudioClip>();

        [SerializeField]
        private float volumeScale = 1f;

        public float VolumeScale => this.volumeScale;

        public AudioClip GetClip() {
            if (this.clips.Length == 0) {
                return null;
            }

            return this.clips[Random.Range(0, this.clips.Length)];
        }
    }
}