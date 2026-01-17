namespace SoundEffects {
    using System;
    using Multicast;
    using Multicast.DirtyDataEditor;

    [DDEObject, Serializable]
    public class SoundEffectDef : Def {
        [DDE("audio_file"), DDEAddressable] public string audioFile;

        [DDE("volume", 1)] public float volume;
    }
}