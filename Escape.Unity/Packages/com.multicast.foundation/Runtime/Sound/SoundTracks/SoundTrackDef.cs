namespace SoundTracks {
    using System;
    using Multicast;
    using Multicast.DirtyDataEditor;
    using UnityEngine;

    [DDEObject, Serializable]
    public class SoundTrackDef : Def {
        [DDE("audio_file"), DDEAddressable] public string audioFile;

        [DDE("volume", 1)] public float volume;
    }
}