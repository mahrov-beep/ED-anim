using InfimaGames.LowPolyShooterPack;
using UnityEngine;

public class ItemBoxConfig : ScriptableObject {
    public AudioClipsSettings AudioClipsOpening, AudioClipsOpen, AudioClipsItemPickUp;

    public float OpeningAudioIntervalSeconds = 1f;
}