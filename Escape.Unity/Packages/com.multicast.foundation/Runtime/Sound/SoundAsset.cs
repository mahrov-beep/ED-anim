namespace Sound {
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class SoundAsset : ScriptableObject {
        [Required] public AudioClip[] clipVariants;

        public AudioClip GetClip() {
            return this.clipVariants[Random.Range(0, this.clipVariants.Length)];
        }
    }
}