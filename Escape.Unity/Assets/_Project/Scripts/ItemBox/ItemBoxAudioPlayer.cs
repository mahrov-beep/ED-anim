using InfimaGames.LowPolyShooterPack;
using Quantum;
using UnityEngine;

public class ItemBoxAudioPlayer : AudioPlayer<ItemBoxAudioLayers> {
    protected override bool AreEqual(ItemBoxAudioLayers a, ItemBoxAudioLayers b) => a == b;
}