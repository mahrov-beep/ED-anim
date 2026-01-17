using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class TextureMemoryStats : MonoBehaviour {
    [SerializeField, Required] private TMP_Text text;

    private void Update() {
        this.text.SetText(
            "<b>Textures count</b><br>" +
            "Streaming: {0}, nonStreaming: {1}<br>" +
            "<b>Texture memory (MB)</b><br>" +
            "Total: {2} ({3}), desired: {4}, theoretical: {5}<br>" +
            "  of that is nonStreaming: {6}"
            , Texture.streamingTextureCount
            , Texture.nonStreamingTextureCount
            , ToMb(Texture.targetTextureMemory)  // Total texture memory usage in bytes after applying the memory budget and loading all textures.
            , ToMb(Texture.currentTextureMemory) // The amount of memory that all Textures in the scene use.
            , ToMb(Texture.desiredTextureMemory) // The total amount of texture memory, in bytes, that Unity would use if no constraints are applied.
            , ToMb(Texture.totalTextureMemory)   // The total texture memory, in bytes, that Unity would use if all textures are loaded at full resolution.
            , ToMb(Texture.nonStreamingTextureMemory) // The amount of memory Unity allocates for non-streaming Textures in the scene. This only includes instances of Texture2D and CubeMap Textures. This does not include any other Texture types, or 2D and CubeMap Textures that Unity creates internally.
        );
    }

    private static ulong ToMb(ulong bytes) => bytes / 1024 / 1024;
}